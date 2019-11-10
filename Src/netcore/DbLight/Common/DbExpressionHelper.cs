using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DbLight.Exceptions;
using DbLight.Sql;

namespace DbLight.Common
{
    public static class DbExpressionHelper
    {
        #region Column Model Expression

        public static List<DbColumnModelInfo> ReadColumnExpression(
            Expression expression, DbModelInfo modelInfo){
            if (modelInfo.Kind == DbModelKind.Tuple){
                return ReadColumnTupleModelExpression(expression, modelInfo);
            }
            else{
                return ReadColumnObjectModelExpression(expression, modelInfo);
            }
        }

        private static List<DbColumnModelInfo> ReadColumnTupleModelExpression(
            Expression expression, DbModelInfo modelInfo){
            List<List<Expression>> groups;
            try{
                groups = ExpandModelExpression(expression);
            }
            catch (Exception ex){
                throw new Exception("Invalid Column Expression.\n" +
                                    "Error Expression: " + expression, ex);
            }

            //Tuple(Posts Post, Blog Blog, int MaxPostId)
            //0 (x)
            //.Select(x => x)
            //1
            //.Select(x => x.MaxPostId) : (Custom Express) AS [Item2.Item]
            //2
            //.Select(x => x.Post) : Item1.*
            //3
            //.Select(x => x.Post.PostId) : [Item1].[PostId] AS [Item1.PostId]
            //4 (x)
            //.Select(x => x.Post.Blog)
            //5 (x)
            //.Select(x => x.Post.Blog.BlogId)

            var columns = new List<DbColumnModelInfo>();

            foreach (var group in groups){
                if (group.Count == 1){
                    //0 (x)
                    //.Select(x => x)
                    throw new Exception("Match all columns not allowed at tuple mode.\n" +
                                        "Error Member: " + group[group.Count - 1] + "\n" +
                                        "Error Expression: " + expression);
                }

                if (group.Count == 2){
                    //1
                    //.Select(x => x.MaxPostId) : (Custom Express) AS [Item2.Item]
                    //2
                    //.Select(x => x.Post) : Item1.*

                    var memberExpression = (MemberExpression) group[1];
                    var memberInfo = modelInfo.Members.Find(x => x.MemberName == memberExpression.Member.Name);
                    if (memberInfo == null){
                        throw new DbCrashException(
                            "Failed to get member info from cache model.\n" +
                            "Error Member: " + group[group.Count - 1] + "\n" +
                            "Error Expression: " + expression);
                    }

                    if (memberInfo.Model.Kind == DbModelKind.Object){
                        memberInfo.Model.Members.ForEach(x => {
                            if (x.Model.Kind == DbModelKind.Value &&
                                x.NotMapped == false){
                                columns.Add(new DbColumnModelInfo(){
                                    Member = memberInfo.ColumnName,
                                    Column = x.ColumnName
                                });
                            }
                        });
                    }
                    else if (memberInfo.Model.Kind == DbModelKind.Tuple){
                        memberInfo.Model.Members.ForEach(x => {
                            if (x.Model.Kind == DbModelKind.Value &&
                                x.NotMapped == false){
                                columns.Add(new DbColumnModelInfo(){
                                    Member = memberInfo.ColumnName,
                                    Column = x.ColumnName
                                });
                            }
                        });
                    }
                    else if (memberInfo.Model.Kind == DbModelKind.Value){
                        columns.Add(new DbColumnModelInfo(){
                            Member = memberInfo.ColumnName,
                            Column = memberInfo.ColumnName
                        });
                    }
                    else{
                        throw new Exception(
                            "The member must be a value, object or tuple type.\n" +
                            "Error Member: " + group[group.Count - 1] + "\n" +
                            "Error Expression: " + expression);
                    }
                }

                if (group.Count == 3){
                    //3
                    //.Select(x => x.Post.PostId) : [Item1].[PostId] AS [Item1.PostId]
                    //4 (x)
                    //.Select(x => x.Post.Blog)

                    var memberExpression = (MemberExpression) group[1];
                    var memberInfo = modelInfo.Members.Find(x => x.MemberName == memberExpression.Member.Name);
                    if (memberInfo == null){
                        throw new DbCrashException(
                            "Failed to get member info from cache model.\n" +
                            "Error Member: " + group[group.Count - 1] + "\n" +
                            "Error Expression: " + expression);
                    }

                    var childExpression = (MemberExpression) group[2];
                    var childInfo = memberInfo.Model.Members.Find(x => x.MemberName == childExpression.Member.Name);
                    if (childInfo == null){
                        throw new DbCrashException(
                            "Failed to get member info from cache model.\n" +
                            "Error Member: " + group[group.Count - 1] + "\n" +
                            "Error Expression: " + expression);
                    }

                    if (childInfo.Model.Kind == DbModelKind.Value){
                        //3
                        //.Select(x => x.Post.PostId) : [Item1].[PostId] AS [Item1.PostId]
                        columns.Add(new DbColumnModelInfo(){
                            Member = memberInfo.ColumnName,
                            Column = childInfo.ColumnName
                        });
                    }
                    else{
                        //4 (x)
                        //.Select(x => x.Post.Blog)
                        throw new Exception(
                            "The member must be a value type.\n" +
                            "Error Member: " + group[group.Count - 1] + "\n" +
                            "Error Expression: " + expression);
                    }
                }

                if (group.Count > 3){
                    //5 (x)
                    //.Select(x => x.Post.Blog.BlogId)
                    throw new Exception(
                        "The member is too deep.\n" +
                        "Error Member: " + group[group.Count - 1] + "\n" +
                        "Error Expression: " + expression);
                }
            }

            return columns;
        }

        private static List<DbColumnModelInfo> ReadColumnObjectModelExpression(
            Expression expression, DbModelInfo modelInfo){
            List<List<Expression>> groups;
            try{
                groups = ExpandModelExpression(expression);
            }
            catch (Exception ex){
                throw new Exception("Invalid Column Expression.\n" +
                                    "Error Expression: " + expression, ex);
            }

            //Post
            //0
            //.Select(x => x) : Post.*
            //1
            //.Select(x => x.PostId) : Post.PostId
            //2
            //.Select(x => x.PostBlog) : PostBlog.*
            //3
            //.Select(x => x.PostBlog.BlogId) : [PostBlog].[BlogId]
            //4 (x)
            //.Select(x => x.Post.PostBlog.PostId)

            var columns = new List<DbColumnModelInfo>();

            foreach (var group in groups){
                if (group.Count == 1){
                    //0
                    //.Select(x => x) : Post.*
                    modelInfo.Members.ForEach(x => {
                        if (x.Model.Kind == DbModelKind.Value &&
                            x.NotMapped == false){
                            columns.Add(new DbColumnModelInfo(){
                                Member = "a",
                                Column = x.ColumnName
                            });
                        }
                    });
                }

                if (group.Count == 2){
                    //1
                    //.Select(x => x.PostId) : Post.PostId
                    //2
                    //.Select(x => x.PostBlog) : PostBlog.*

                    var memberExpression = (MemberExpression) group[1];
                    var memberInfo = modelInfo.Members.Find(x => x.MemberName == memberExpression.Member.Name);
                    if (memberInfo == null){
                        throw new DbCrashException(
                            "Failed to get member info from cache model.\n" +
                            "Error Member: " + group[group.Count - 1] + "\n" +
                            "Error Expression: " + expression);
                    }

                    if (memberInfo.Model.Kind == DbModelKind.Object){
                        memberInfo.Model.Members.ForEach(x => {
                            if (x.Model.Kind == DbModelKind.Value &&
                                x.NotMapped == false){
                                columns.Add(new DbColumnModelInfo(){
                                    Member = memberInfo.ColumnName,
                                    Column = x.ColumnName
                                });
                            }
                        });
                    }
                    else if (memberInfo.Model.Kind == DbModelKind.Value){
                        columns.Add(new DbColumnModelInfo(){
                            Member = "a",
                            Column = memberInfo.ColumnName
                        });
                    }
                    else{
                        throw new Exception(
                            "The member must be a value or object type.\n" +
                            "Error Member: " + group[group.Count - 1] + "\n" +
                            "Error Expression: " + expression);
                    }
                }

                if (group.Count == 3){
                    //3
                    //.Select(x => x.PostBlog.BlogId) : [PostBlog].[BlogId]

                    var memberExpression = (MemberExpression) group[1];
                    var memberInfo = modelInfo.Members.Find(x => x.MemberName == memberExpression.Member.Name);
                    if (memberInfo == null){
                        throw new DbCrashException(
                            "Failed to get member info from cache model.\n" +
                            "Error Member: " + group[group.Count - 1] + "\n" +
                            "Error Expression: " + expression);
                    }

                    var childExpression = (MemberExpression) group[2];
                    var childInfo = memberInfo.Model.Members.Find(x => x.MemberName == childExpression.Member.Name);
                    if (childInfo == null){
                        throw new DbCrashException(
                            "Failed to get member info from cache model.\n" +
                            "Error Member: " + group[group.Count - 1] + "\n" +
                            "Error Expression: " + expression);
                    }

                    if (childInfo.Model.Kind == DbModelKind.Value){
                        //3
                        //.Select(x => x.Post.PostId) : [Item1].[PostId] AS [Item1.PostId]
                        columns.Add(new DbColumnModelInfo(){
                            Member = memberInfo.ColumnName,
                            Column = childInfo.ColumnName
                        });
                    }
                    else{
                        //4 (x)
                        //.Select(x => x.Post.Blog)
                        throw new Exception(
                            "The member must be a value type.\n" +
                            "Error Member: " + group[group.Count - 1] + "\n" +
                            "Error Expression: " + expression);
                    }
                }

                if (group.Count > 3){
                    //4 (x)
                    //.Select(x => x.Post.PostBlog.PostId)
                    throw new Exception(
                        "The member is too deep.\n" +
                        "Error Member: " + group[group.Count - 1] + "\n" +
                        "Error Expression: " + expression);
                }
            }

            return columns;
        }

        #endregion

        #region Table Model Expression

        public static DbTableModelInfo ReadTableExpression(
            Expression expression, DbModelInfo modelInfo){
            if (modelInfo.Kind == DbModelKind.Tuple){
                return ReadTableTupleModelExpression(expression, modelInfo);
            }
            else{
                return ReadTableObjectModelExpression(expression, modelInfo);
            }
        }

        private static DbTableModelInfo ReadTableTupleModelExpression(
            Expression expression, DbModelInfo modelInfo){
            List<List<Expression>> groups;
            try{
                groups = ExpandModelExpression(expression);
            }
            catch (Exception ex){
                throw new Exception("Invalid Table Expression.\n" +
                                    "Error Expression: " + expression, ex);
            }

            if (groups.Count != 1){
                throw new Exception("Specify one member for table name.\n" +
                                    "Error Expression: " + expression);
            }

            //Tuple(Posts Post, Blog Blog, int MaxPostId)
            //0 (x)
            //.Select(x => x)
            //1
            //.Select(x => x.Post) : Item1.*
            //2 (x)
            //.Select(x => x.Post.Blog)

            var group = groups[0];

            if (group.Count == 1){
                //0 (x)
                //.Select(x => x)
                throw new Exception("Special a object member for table name\n" +
                                    "Error Member: " + group[group.Count - 1] + "\n" +
                                    "Error Expression: " + expression);
            }
            else if (group.Count == 2){
                //1
                //.Select(x => x.Post) : Item1

                var memberExpression = (MemberExpression) group[1];
                var memberInfo = modelInfo.Members.Find(x => x.MemberName == memberExpression.Member.Name);
                if (memberInfo == null){
                    throw new DbCrashException(
                        "Failed to get member info from cache model.\n" +
                        "Error Member: " + group[group.Count - 1] + "\n" +
                        "Error Expression: " + expression);
                }

                if (memberInfo.Model.Kind == DbModelKind.Object){
                    return new DbTableModelInfo(){
                        Member = memberInfo.ColumnName,
                        Table = memberInfo.Model.TableName,
                        Schema = memberInfo.Model.SchemaName,
                        Database = memberInfo.Model.DatabaseName
                    };
                }
                else if (memberInfo.Model.Kind == DbModelKind.Tuple){
                    return new DbTableModelInfo(){
                        Member = memberInfo.ColumnName,
                    };
                }
                else if (memberInfo.Model.Kind == DbModelKind.Value){
                    return new DbTableModelInfo(){
                        Member = memberInfo.ColumnName,
                    };
                }
                else{
                    throw new Exception(
                        "The member must be a value, object or tuple type.\n" +
                        "Error Member: " + group[group.Count - 1] + "\n" +
                        "Error Expression: " + expression);
                }
            }
            else{
                //2 (x)
                //.Select(x => x.Post.Blog)
                throw new Exception(
                    "The member is too deep.\n" +
                    "Error Member: " + group[group.Count - 1] + "\n" +
                    "Error Expression: " + expression);
            }
        }

        private static DbTableModelInfo ReadTableObjectModelExpression(
            Expression expression, DbModelInfo modelInfo){
            List<List<Expression>> groups;
            try{
                groups = ExpandModelExpression(expression);
            }
            catch (Exception ex){
                throw new Exception("Invalid Table Expression.\n" +
                                    "Error Expression: " + expression, ex);
            }

            if (groups.Count != 1){
                throw new Exception("Specify one member for table name.\n" +
                                    "Error Expression: " + expression);
            }

            //Post
            //0
            //.Select(x => x) : Post.*
            //1 (x)
            //.Select(x => x.PostId) : Post.PostId
            //2 
            //.Select(x => x.PostBlog) : PostBlog.*

            var group = groups[0];

            if (group.Count == 1){
                //0
                //.Select(x => x) : Post.*
                return new DbTableModelInfo(){
                    Member = "a",
                    Table = modelInfo.TableName,
                    Schema = modelInfo.SchemaName,
                    Database = modelInfo.DatabaseName
                };
            }
            else if (group.Count == 2){
                //1
                //.Select(x => x.PostId) : Post.PostId
                //2
                //.Select(x => x.PostBlog) : PostBlog.*

                var memberExpression = (MemberExpression) group[1];
                var memberInfo = modelInfo.Members.Find(x => x.MemberName == memberExpression.Member.Name);
                if (memberInfo == null){
                    throw new DbCrashException(
                        "Failed to get member info from cache model.\n" +
                        "Error Member: " + group[group.Count - 1] + "\n" +
                        "Error Expression: " + expression);
                }

                if (memberInfo.Model.Kind == DbModelKind.Object){
                    return new DbTableModelInfo(){
                        Member = memberInfo.ColumnName,
                        Table = memberInfo.Model.TableName,
                        Schema = memberInfo.Model.SchemaName,
                        Database = memberInfo.Model.DatabaseName
                    };
                }
                else{
                    throw new Exception(
                        "The member must be a object type.\n" +
                        "Error Member: " + group[group.Count - 1] + "\n" +
                        "Error Expression: " + expression);
                }
            }
            else{
                //4 (x)
                //.Select(x => x.Post.PostBlog.PostId)
                throw new Exception(
                    "The member is too deep.\n" +
                    "Error Member: " + group[group.Count - 1] + "\n" +
                    "Error Expression: " + expression);
            }
        }

        #endregion

        #region Where Expression

        public static string ReadQueryWhereExpression(
            DbConnection connection, DbModelInfo modelInfo,
            Expression expression){
            return new DbWhereExpression(connection, modelInfo, expression, DbWhereType.Query).ToString();
        }

        public static string ReadEditWhereExpression(
            DbConnection connection, DbModelInfo modelInfo,
            Expression expression){
            return new DbWhereExpression(connection, modelInfo, expression, DbWhereType.Edit).ToString();
        }

        #endregion

        #region Where Compare Expression

        public static string ReadQueryWhereCompareExpression(
            DbConnection connection, DbModelInfo modelInfo,
            Expression expression, ExpressionType expressionType,
            object value){
            return new DbWhereExpression(connection, modelInfo,
                    expression, DbWhereType.Query)
                .ToCompareSql(expressionType, value);
        }

        public static string ReadEditWhereCompareExpression(
            DbConnection connection, DbModelInfo modelInfo,
            Expression expression, ExpressionType expressionType,
            object value){
            return new DbWhereExpression(connection, modelInfo,
                    expression, DbWhereType.Edit)
                .ToCompareSql(expressionType, value);
        }

        #endregion

        #region Where Like Expression

        internal static string ReadQueryWhereLikeExpression(
            DbConnection connection, DbModelInfo modelInfo,
            Expression expression, DbWhereLikeType likeType,
            string value){
            return new DbWhereExpression(connection, modelInfo,
                    expression, DbWhereType.Query)
                .ToLikeSql(likeType, value);
        }

        internal static string ReadEditWhereLikeExpression(
            DbConnection connection, DbModelInfo modelInfo,
            Expression expression, DbWhereLikeType likeType,
            string value){
            return new DbWhereExpression(connection, modelInfo,
                    expression, DbWhereType.Edit)
                .ToLikeSql(likeType, value);
        }

        #endregion

        #region Where In Expression

        internal static string ReadQueryWhereInExpression<T2>(
            DbConnection connection, DbModelInfo modelInfo,
            Expression expression, IEnumerable<T2> values){
            return new DbWhereExpression(connection, modelInfo,
                    expression, DbWhereType.Query)
                .ToInSql(values);
        }

        internal static string ReadEditWhereInExpression<T2>(
            DbConnection connection, DbModelInfo modelInfo,
            Expression expression, IEnumerable<T2> values){
            return new DbWhereExpression(connection, modelInfo,
                    expression, DbWhereType.Edit)
                .ToInSql(values);
        }

        #endregion

        #region Any Expression

        public static string ReadEditAnyExpression<T, T2>(DbConnection connection, DbModelInfo modelInfo,
            string expression, Expression<Func<T, T2>> parameters){
            return new DbAnyExpression(connection, modelInfo,
                expression, parameters,
                DbWhereType.Edit).ToString();
        }

        public static string ReadQueryAnyExpression<T, T2>(DbConnection connection, DbModelInfo modelInfo,
            string expression, Expression<Func<T, T2>> parameters){
            return new DbAnyExpression(connection, modelInfo,
                expression, parameters,
                DbWhereType.Query).ToString();
        }

        #endregion

        public static bool IsModelExpression(Expression expression){
            while (true){
                if (expression is LambdaExpression lambdaExpression){
                    expression = lambdaExpression.Body;
                }
                else if (expression is NewExpression){
                    return true;
                }
                else if (expression is MemberExpression){
                    expression = ((MemberExpression) expression).Expression;
                }
                else if (expression is ParameterExpression){
                    return true;
                }
                else{
                    return false;
                }
            }
        }

        public static List<List<Expression>> ExpandModelExpression(Expression expression){
            List<List<Expression>> items = new List<List<Expression>>();

            if (expression is LambdaExpression lambdaExpression){
                var subItems = ExpandModelExpression(lambdaExpression.Body);
                subItems.ForEach(x => items.Add(x));
            }
            else if (expression is NewExpression newExpression){
                if (newExpression.Arguments.Count == 0){
                    throw new Exception("No members in the new expression.");
                }

                foreach (var argument in newExpression.Arguments){
                    var subItems = ExpandModelExpression(argument);
                    subItems.ForEach(x => items.Add(x));
                }
            }
            else if (expression is MemberExpression){
                List<Expression> item = new List<Expression>();

                while (true){
                    item.Add(expression);

                    if (expression is MemberExpression){
                        expression = ((MemberExpression) expression).Expression;
                        continue;
                    }

                    if (expression is ParameterExpression){
                        break;
                    }

                    throw new Exception("Invalid Model Expression.");
                }

                var copy = new List<Expression>();
                for (var i = item.Count - 1; i >= 0; i--){
                    copy.Add(item[i]);
                }

                items.Add(copy);
            }
            else if (expression is ParameterExpression){
                var item = new List<Expression>{expression};
                items.Add(item);
            }
            else{
                throw new Exception("Invalid Model Expression.");
            }

            return items;
        }
    }

    class DbWhereExpression
    {
        private readonly DbConnection _connection;
        private readonly DbModelInfo _modelInfo;
        private readonly Expression _expression;
        private readonly DbWhereType _whereType;

        public DbWhereExpression(DbConnection connection, DbModelInfo modelInfo,
            Expression expression, DbWhereType whereType){
            _connection = connection;
            _modelInfo = modelInfo;
            _expression = expression;
            _whereType = whereType;
        }

        public string ToCompareSql(ExpressionType expressionType, object value){
            var column = ColumnToSql(_expression);
            string connect;
            switch (expressionType){
                case ExpressionType.Equal:
                    connect = " = ";
                    break;
                case ExpressionType.NotEqual:
                    connect = " <> ";
                    break;
                case ExpressionType.GreaterThan:
                    connect = " > ";
                    break;
                case ExpressionType.LessThan:
                    connect = " < ";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    connect = " >= ";
                    break;
                case ExpressionType.LessThanOrEqual:
                    connect = " <= ";
                    break;
                default:
                    throw new DbUnknownException("Unexpected Expression Type.\n" +
                                                 "Expression Type: " + expressionType);
            }

            return $"{column}{connect}{DbSql.ValueToWhereSql(_connection, value)}";
        }

        public string ToLikeSql(DbWhereLikeType likeType, string value){
            var column = ColumnToSql(_expression);
            return DbSql.ToLikeSql(_connection, column, likeType, value);
        }

        public string ToInSql<T2>(IEnumerable<T2> values){
            var column = ColumnToSql(_expression);
            var strValues = new List<string>();

            foreach (var value in values){
                strValues.Add(DbSql.ValueToWhereSql(_connection, value));
            }

            return $"{column} IN ({string.Join(", ", strValues)})";
        }

        public override string ToString(){
            return ToSql(_expression);
        }

        private string ToSql(Expression expression){
            if (expression.NodeType == ExpressionType.Lambda){
                var l = expression as LambdaExpression;
                if (l == null){
                    throw new DbUnknownException("Unexpected LambdaExpression.\n" +
                                                 "Expression: " + expression);
                }

                return ToSql(l.Body);
            }
            else if (expression.NodeType == ExpressionType.AndAlso ||
                     expression.NodeType == ExpressionType.OrElse){
                var connect = "";
                switch (expression.NodeType){
                    case ExpressionType.AndAlso:
                        connect = " AND ";
                        break;
                    case ExpressionType.OrElse:
                        connect = " OR ";
                        break;
                }

                var l = expression as BinaryExpression;
                if (l == null){
                    throw new DbUnknownException("Unexpected BinaryExpression.\n" +
                                                 "Expression: " + expression);
                }

                var leftNodeType = ExpressionType.Add;
                if (l.Left is BinaryExpression left){
                    leftNodeType = left.NodeType;
                }

                var rightNodeType = ExpressionType.Add;
                if (l.Right is BinaryExpression right){
                    rightNodeType = right.NodeType;
                }

                var leftNeedBracket = false;
                var rightNeedBracket = false;
                if (expression.NodeType == ExpressionType.AndAlso){
                    leftNeedBracket = leftNodeType == ExpressionType.OrElse;
                    rightNeedBracket = rightNodeType == ExpressionType.OrElse;
                }
                else if (expression.NodeType == ExpressionType.OrElse){
                    leftNeedBracket = leftNodeType == ExpressionType.AndAlso;
                    rightNeedBracket = rightNodeType == ExpressionType.AndAlso;
                }

                return (leftNeedBracket ? "(" : "") + ToSql(l.Left) + (leftNeedBracket ? ")" : "") +
                       connect +
                       (rightNeedBracket ? "(" : "") + ToSql(l.Right) + (rightNeedBracket ? ")" : "");
            }
            else if (expression.NodeType == ExpressionType.Equal ||
                     expression.NodeType == ExpressionType.NotEqual ||
                     expression.NodeType == ExpressionType.GreaterThan ||
                     expression.NodeType == ExpressionType.LessThan ||
                     expression.NodeType == ExpressionType.GreaterThanOrEqual ||
                     expression.NodeType == ExpressionType.LessThanOrEqual ||
                     expression.NodeType == ExpressionType.Add ||
                     expression.NodeType == ExpressionType.Subtract ||
                     expression.NodeType == ExpressionType.Multiply ||
                     expression.NodeType == ExpressionType.Divide){
                var connect = "";
                switch (expression.NodeType){
                    case ExpressionType.Equal:
                        connect = " = ";
                        break;
                    case ExpressionType.NotEqual:
                        connect = " <> ";
                        break;
                    case ExpressionType.GreaterThan:
                        connect = " > ";
                        break;
                    case ExpressionType.LessThan:
                        connect = " < ";
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        connect = " >= ";
                        break;
                    case ExpressionType.LessThanOrEqual:
                        connect = " <= ";
                        break;
                    case ExpressionType.Add:
                        connect = " + ";
                        break;
                    case ExpressionType.Subtract:
                        connect = " - ";
                        break;
                    case ExpressionType.Multiply:
                        connect = " * ";
                        break;
                    case ExpressionType.Divide:
                        connect = " / ";
                        break;
                }

                var l = expression as BinaryExpression;
                if (l == null){
                    throw new DbUnknownException("Unexpected BinaryExpression.\n" +
                                                 "Expression: " + expression);
                }

                {
                    var temp = ProcessNullCompare(l);
                    if (temp != null){
                        return temp;
                    }
                }

                {
                    var temp = ProcessCompareToMethod(l);
                    if (temp != null){
                        return temp;
                    }
                }

                return ToSql(l.Left) + connect + ToSql(l.Right);
            }
            else if (expression.NodeType == ExpressionType.MemberAccess ||
                     expression.NodeType == ExpressionType.Constant){
                if (expression.NodeType == ExpressionType.MemberAccess &&
                    DbExpressionHelper.IsModelExpression(expression)){
                    return ColumnToSql(expression);
                }
                else{
                    var value = Expression.Lambda(expression).Compile().DynamicInvoke();
                    try{
                        return DbSql.ValueToWhereSql(_connection, value);
                    }
                    catch (Exception ex){
                        throw new Exception("Failed to read value from the expression.\n" +
                                            "Expression: " + expression, ex);
                    }
                }
            }
            else if (expression.NodeType == ExpressionType.Call){
                var l = expression as MethodCallExpression;
                if (l == null){
                    throw new DbUnknownException("Unexpected MethodCallExpression for ExpressionType.Call.\n" +
                                                 "Expression: " + expression);
                }

                return ProcessMethod(l);
            }
            else{
                throw new Exception("Unexpected expression: " + expression);
            }
        }

        private string ColumnToSql(Expression expression){
            var columns = DbExpressionHelper.ReadColumnExpression(expression, _modelInfo);
            if (columns.Count != 1){
                throw new Exception("Specify one column in the where part.\n" +
                                    "Expression: " + expression);
            }

            if (_whereType == DbWhereType.Query){
                return string.Format("{0}.{1}",
                    DbSql.GetColumnName(_connection, columns[0].Member),
                    DbSql.GetColumnName(_connection, columns[0].Column));
            }
            else{
                return string.Format("{0}", DbSql.GetColumnName(_connection, columns[0].Column));
            }
        }

        private string ProcessCompareToMethod(BinaryExpression expression){
            if (expression.NodeType == ExpressionType.Equal ||
                expression.NodeType == ExpressionType.GreaterThan ||
                expression.NodeType == ExpressionType.LessThan ||
                expression.NodeType == ExpressionType.GreaterThanOrEqual ||
                expression.NodeType == ExpressionType.LessThanOrEqual){
                //ok
            }
            else{
                return null;
            }

            //may be x.Name.Compare("xxx") >= 0
            if (expression.Right.NodeType == ExpressionType.Constant && expression.Right.ToString() == "0"){
                //ok
            }
            else{
                return null;
            }

            if (expression.Left.NodeType != ExpressionType.Call){
                return null;
            }

            var left = expression.Left as MethodCallExpression;
            if (left == null){
                throw new DbUnknownException("Unexpected MethodCallExpression for ExpressionType.Call.\n" +
                                             "Expression: " + expression);
            }

            if (left.Arguments.Count == 1 &&
                left.Object != null &&
                left.Method.Name == "CompareTo"){
                //ok
            }
            else{
                return null;
            }

            if (DbExpressionHelper.IsModelExpression(left.Object)){
                //ok
            }
            else{
                return null;
            }


            var connect = "";
            switch (expression.NodeType){
                case ExpressionType.Equal:
                    connect = " = ";
                    break;
                case ExpressionType.GreaterThan:
                    connect = " > ";
                    break;
                case ExpressionType.LessThan:
                    connect = " < ";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    connect = " >= ";
                    break;
                case ExpressionType.LessThanOrEqual:
                    connect = " <= ";
                    break;
            }

            //x.Name.Compare("xx") > 0
            var column = ColumnToSql(left.Object);
            var objValue = Expression.Lambda(left.Arguments[0]).Compile().DynamicInvoke();
            string strValue;
            try{
                strValue = DbSql.ValueToWhereSql(_connection, objValue);
            }
            catch (Exception ex){
                throw new Exception("Failed to read value from the expression.\n" +
                                    "Expression: " + expression, ex);
            }

            return string.Format("{0}{1}{2}", column, connect, strValue);
        }

        private string ProcessNullCompare(BinaryExpression expression){
            if (expression.NodeType == ExpressionType.Equal ||
                expression.NodeType == ExpressionType.NotEqual){
                //ok
            }
            else{
                return null;
            }

            //may be x.Id = null
            if (expression.Right.NodeType == ExpressionType.Constant && expression.Right.ToString() == "null"){
                //ok
            }
            else{
                return null;
            }

            Expression columnExpression;
            if (expression.Left.NodeType == ExpressionType.Convert){
                //use not nullable x.Int == null then the left side is a Convert
                var left = expression.Left as UnaryExpression;
                if (left == null){
                    throw new DbUnknownException(
                        "Unexpected UnaryExpression for ExpressionType.Convert.\n" +
                        "Expression: " + expression);
                }

                columnExpression = left.Operand;
            }
            else if (expression.Left.NodeType == ExpressionType.MemberAccess){
                //use x.string == null then left side is a MemberAccess
                columnExpression = expression.Left;
            }
            else{
                throw new Exception("The left side must be a column for [NULL] compare.\n" +
                                    "Expression: " + expression);
            }

            return ColumnToSql(columnExpression) + " IS" +
                   (expression.NodeType == ExpressionType.Equal ? "" : " NOT") + " NULL";
        }

        private string ProcessMethod(MethodCallExpression expression){
            //todo
            //hard code method name
            if (expression.Arguments.Count == 0 && expression.Object != null && expression.Method.Name == "To"){
                if (expression.Object.Type == typeof(SqlQuery) ||
                    expression.Object.Type.IsSubclassOf(typeof(SqlQuery))){
                    //SqlQuery.To<xxx>()
                    var query = Expression.Lambda(expression.Object).Compile().DynamicInvoke() as SqlQuery;
                    if (query == null){
                        throw new DbUnknownException("Unexpected SqlQuery Expression for Method SqlQuery.To()\n" +
                                                     "Expression: " + expression);
                    }

                    return "(" + query + ")";
                }
                else if (expression.Object.Type == typeof(SqlExp)){
                    //SqlExp.To<xxx>()
                    var exp = Expression.Lambda(expression.Object).Compile().DynamicInvoke() as SqlExp;
                    if (exp == null){
                        throw new DbUnknownException("Unexpected SqlExp Expression for Method SqlExp.To()\n" +
                                                     "Expression: " + expression);
                    }

                    return "(" + exp + ")";
                }
                else{
                    throw GetCallException(expression);
                }
            }
            else if (expression.Arguments.Count == 1 && expression.Object != null &&
                     expression.Method.Name == "Contains"){
                if (expression.Object.Type == typeof(SqlQuery) ||
                    expression.Object.Type.IsSubclassOf(typeof(SqlQuery))){
                    //SqlQuery.Contains(x.Id)
                    var query = Expression.Lambda(expression.Object).Compile().DynamicInvoke() as SqlQuery;
                    if (query == null){
                        throw new DbUnknownException("Unexpected SqlQuery Expression for Method SqlQuery.Contains()\n" +
                                                     "Expression: " + expression);
                    }

                    if (!DbExpressionHelper.IsModelExpression(expression.Arguments[0])){
                        throw new Exception("The parameter must be a column in SqlQuery.Contains()\n" +
                                            "Expression: " + expression);
                    }

                    var column = ColumnToSql(expression.Arguments[0]);
                    return string.Format("{0} IN({1})", column, query.ToString());
                }
                else if (expression.Object.Type == typeof(SqlExp)){
                    //SqlExp.Contains(x.Id)
                    var exp = Expression.Lambda(expression.Object).Compile().DynamicInvoke() as SqlExp;
                    if (exp == null){
                        throw new DbUnknownException("Unexpected SqlExp Expression for Method SqlExp.Contains()\n" +
                                                     "Expression: " + expression);
                    }

                    if (!DbExpressionHelper.IsModelExpression(expression.Arguments[0])){
                        throw new Exception("The parameter must be a column in SqlExp.Contains()\n" +
                                            "Expression: " + expression);
                    }

                    var column = ColumnToSql(expression.Arguments[0]);
                    return string.Format("{0} IN({1})", column, exp.ToString());
                }
                else if (DbExpressionHelper.IsModelExpression(expression.Object)){
                    //x.Name.Contains("xx")
                    var value = Expression.Lambda(expression.Arguments[0]).Compile().DynamicInvoke();
                    if (value is string str){
                        var column = ColumnToSql(expression.Object);
                        return DbSql.ToLikeSql(_connection, column, DbWhereLikeType.Middle, str);
                    }
                    else{
                        throw GetCallException(expression);
                    }
                }
                else{
                    throw GetCallException(expression);
                }
            }
            else if (expression.Arguments.Count == 2
                     && expression.Object == null
                     && expression.Method.Name == "Contains"){
                if (!DbExpressionHelper.IsModelExpression(expression.Arguments[1])){
                    throw GetCallException(expression);
                }

                //new int[]{1,2,3}.Contains(x.Id)
                var array = Expression.Lambda(expression.Arguments[0]).Compile().DynamicInvoke();
                if (array is Array values){
                    var strValues = new List<string>();

                    foreach (var value in values){
                        string valueSql;
                        try{
                            valueSql = DbSql.ValueToWhereSql(_connection, value);
                        }
                        catch (Exception ex){
                            throw new Exception("Failed to read value from the expression.\n" +
                                                "Expression: " + expression, ex);
                        }

                        strValues.Add(valueSql);
                    }

                    var column = ColumnToSql(expression.Arguments[1]);
                    return string.Format("{0} IN ({1})", column, string.Join(", ", strValues));
                }
                else{
                    throw GetCallException(expression);
                }
            }
            else if (expression.Arguments.Count == 1
                     && expression.Object != null
                     && expression.Method.Name == "StartsWith"){
                if (DbExpressionHelper.IsModelExpression(expression.Object)){
                    //x.Name.Contains("xx")
                    var value = Expression.Lambda(expression.Arguments[0]).Compile().DynamicInvoke();
                    if (value is string str){
                        var column = ColumnToSql(expression.Object);
                        return DbSql.ToLikeSql(_connection, column, DbWhereLikeType.Before, str);
                    }
                    else{
                        throw GetCallException(expression);
                    }
                }
                else{
                    throw GetCallException(expression);
                }
            }
            else if (expression.Arguments.Count == 1
                     && expression.Object != null
                     && expression.Method.Name == "EndsWith"){
                if (DbExpressionHelper.IsModelExpression(expression.Object)){
                    //x.Name.Contains("xx")
                    var value = Expression.Lambda(expression.Arguments[0]).Compile().DynamicInvoke();
                    if (value is string str){
                        var column = ColumnToSql(expression.Object);
                        return DbSql.ToLikeSql(_connection, column, DbWhereLikeType.After, str);
                    }
                    else{
                        throw GetCallException(expression);
                    }
                }
                else{
                    throw GetCallException(expression);
                }
            }
            else{
                throw GetCallException(expression);
            }
        }

        private Exception GetCallException(Expression expression){
            return new Exception("Unexpected MethodCallExpression.\n" +
                                 "Only supports the following Methods:\n" +
                                 "Array.Contains \n" +
                                 "string.Contains \n" +
                                 "string.StartsWith \n" +
                                 "string.EndsWith\n" +
                                 "string.EndsWith\n" +
                                 "SqlQuery.To()\n" +
                                 "SqlQuery.Contains()\n" +
                                 "SqlExp.To()\n" +
                                 "SqlExp.Contains()\n" +
                                 "Expression: " + expression);
        }
    }

    class DbAnyExpression
    {
        private readonly DbConnection _connection;
        private readonly DbModelInfo _modelInfo;
        private readonly string _expression;
        private readonly Expression _parameterExpression;
        private readonly DbWhereType _whereType;

        public DbAnyExpression(DbConnection connection, DbModelInfo modelInfo,
            string expression, Expression parameterExpression,
            DbWhereType whereType){
            _connection = connection;
            _modelInfo = modelInfo;
            _expression = expression;
            _parameterExpression = parameterExpression;
            _whereType = whereType;
        }

        public override string ToString(){
            var sql = _expression;
            var parameters = new List<DbColumnModelInfo>();
            if (_parameterExpression != null){
                parameters = DbExpressionHelper.ReadColumnExpression(_parameterExpression, _modelInfo);
            }

            var copyParameters = new object[parameters.Count];
            if (_whereType == DbWhereType.Query){
                for (var i = 0; i < parameters.Count; i++){
                    copyParameters[i] = string.Format("{0}.{1}",
                        DbSql.GetColumnName(_connection, parameters[i].Member),
                        DbSql.GetColumnName(_connection, parameters[i].Column));
                }
            }
            else{
                for (var i = 0; i < parameters.Count; i++){
                    copyParameters[i] = string.Format("{0}",
                        DbSql.GetColumnName(_connection, parameters[i].Column));
                }
            }

            try{
                return string.Format(sql, copyParameters);
            }
            catch (Exception ex){
                throw new Exception("Parameters not match expression.\n" +
                                    ex.Message + "\n" +
                                    "Error Expression: " + sql + "\n" +
                                    "Error Parameters: " + string.Join(", ", copyParameters));
            }
        }
    }

    enum DbWhereType
    {
        Query,
        Edit
    }

    public enum DbWhereLikeType
    {
        Before,
        After,
        Middle
    }
}