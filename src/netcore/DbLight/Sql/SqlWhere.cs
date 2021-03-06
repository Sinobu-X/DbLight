﻿using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;
using DbLight.Common;

namespace DbLight.Sql
{
    public abstract class SqlWhere
    {
        public DbConnection Connection{ get; protected set; }
        public DbModelInfo ModelInfo{ get; protected set; }
        public SqlWhereJoinType JoinType{ get; set; } = SqlWhereJoinType.And;
        public SqlWhereType WhereType{ get; protected set; } = SqlWhereType.Query;
        //string|SqlWhere
        private readonly List<object> _wheres = new List<object>();

        protected void AddWhere(Expression expression){
            if (WhereType == SqlWhereType.Query){
                _wheres.Add(DbExpressionHelper.ReadQueryWhereExpression(Connection, ModelInfo, expression));
            }
            else{
                _wheres.Add(DbExpressionHelper.ReadEditWhereExpression(Connection, ModelInfo, expression));
            }
        }

        protected void AddWhere(string expression){
            _wheres.Add(expression);
        }

        protected void AddWhere(SqlWhere where){
            _wheres.Add(where);
        }

        public override string ToString(){
            var sql = new StringBuilder();

            var rows = new List<string>();
            _wheres.ForEach(x => {
                if (x is SqlWhere subWhere){
                    var s = subWhere.ToString();
                    if (!string.IsNullOrEmpty(s)){
                        rows.Add(s);
                    }
                }
                else if (x is string s){
                    if (!string.IsNullOrEmpty(s)){
                        rows.Add(s);
                    }
                }
            });

            var isFirst = true;
            rows.ForEach(x => {
                sql.Append(isFirst ? "" : JoinType == SqlWhereJoinType.And ? " AND " : " OR ");
                isFirst = false;

                var needBracket = false;
                if (JoinType == SqlWhereJoinType.And &&
                    x.ToUpper().IndexOf(" OR ", StringComparison.Ordinal) >= 0){
                    needBracket = true;
                }
                else if (JoinType == SqlWhereJoinType.Or &&
                         x.ToUpper().IndexOf(" AND ", StringComparison.Ordinal) >= 0){
                    needBracket = true;
                }

                sql.Append(needBracket ? "(" : "");
                sql.Append(x);
                sql.Append(needBracket ? ")" : "");
            });

            return sql.ToString();
        }
    }

    public class SqlWhere<T> : SqlWhere
    {
        public SqlWhere(DbConnection connection, SqlWhereType whereType){
            Connection = connection;
            WhereType = whereType;
        }

        public SqlWhere<T> Add(Expression<Func<T, bool>> expression){
            AddWhere(expression);
            return this;
        }

        public SqlWhere<T> Add(string expression){
            AddWhere(expression);
            return this;
        }

        public SqlWhere<SqlWhere<T>, T> WhereBegin(SqlWhereJoinType joinType = SqlWhereJoinType.And){
            var where = new SqlWhere<SqlWhere<T>, T>(this, joinType);
            AddWhere(where);
            return where;
        }
    }

    public class SqlWhere<TP, T> : SqlWhere where TP : class
    {
        private readonly TP _parent;

        public SqlWhere(TP parent, SqlWhereJoinType joinType = SqlWhereJoinType.And){
            _parent = parent;
            JoinType = joinType;
            if (_parent is SqlQuery sq){
                Connection = sq.Connection;
                ModelInfo = sq.ModelInfo;
                WhereType = SqlWhereType.Query;
            }
            else if (_parent is SqlDelete sd){
                Connection = sd.Connection;
                ModelInfo = sd.ModelInfo;
                WhereType = SqlWhereType.Delete;
            }
            else if (_parent is SqlUpdate su){
                Connection = su.Connection;
                ModelInfo = su.ModelInfo;
                WhereType = SqlWhereType.Update;
            }
            else if (_parent is SqlWhere sw){
                Connection = sw.Connection;
                ModelInfo = sw.ModelInfo;
                WhereType = sw.WhereType;
            }
            else{
                throw new Exception("P only be SqlQuery, SqlDelete, SqlUpdate, SqlWhere.");
            }
        }

        public SqlWhere<TP, T> Add(Expression<Func<T, bool>> expression){
            AddWhere(expression);
            return this;
        }

        public SqlWhere<TP, T> Compare<T2>(Expression<Func<T, T2>> expression, SqlCompareType compareType, T2 value){
            ExpressionType expressionType;
            switch (compareType){
                case SqlCompareType.Equal:
                    expressionType = ExpressionType.Equal;
                    break;
                case SqlCompareType.NotEqual:
                    expressionType = ExpressionType.NotEqual;
                    break;
                case SqlCompareType.Greater:
                    expressionType = ExpressionType.GreaterThan;
                    break;
                case SqlCompareType.Less:
                    expressionType = ExpressionType.LessThan;
                    break;
                case SqlCompareType.GreaterOrEqual:
                    expressionType = ExpressionType.GreaterThanOrEqual;
                    break;
                case SqlCompareType.LessOrEqual:
                    expressionType = ExpressionType.LessThanOrEqual;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(compareType), compareType, null);
            }

            if (WhereType == SqlWhereType.Query){
                AddWhere(DbExpressionHelper.ReadQueryWhereCompareExpression(Connection, ModelInfo,
                    expression, expressionType, value));
            }
            else{
                AddWhere(DbExpressionHelper.ReadEditWhereCompareExpression(Connection, ModelInfo,
                    expression, expressionType, value));
            }

            return this;
        }

        public SqlWhere<TP, T> Like(Expression<Func<T, string>> expression, SqlLikeType likeType, string value){
            DbWhereLikeType expressionType;
            switch (likeType){
                case SqlLikeType.Before:
                    expressionType = DbWhereLikeType.Before;
                    break;
                case SqlLikeType.After:
                    expressionType = DbWhereLikeType.After;
                    break;
                case SqlLikeType.Middle:
                    expressionType = DbWhereLikeType.Middle;
                    break;
                case SqlLikeType.Equal:
                    expressionType = DbWhereLikeType.Equal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(likeType), likeType, null);
            }

            if (WhereType == SqlWhereType.Query){
                AddWhere(DbExpressionHelper.ReadQueryWhereLikeExpression(Connection, ModelInfo,
                    expression, expressionType, value));
            }
            else{
                AddWhere(DbExpressionHelper.ReadEditWhereLikeExpression(Connection, ModelInfo,
                    expression, expressionType, value));
            }

            return this;
        }

        public SqlWhere<TP, T> In<T2>(Expression<Func<T, T2>> expression, IEnumerable<T2> values){
            if (WhereType == SqlWhereType.Query){
                AddWhere(DbExpressionHelper.ReadQueryWhereInExpression(Connection, ModelInfo,
                    expression, values));
            }
            else{
                AddWhere(DbExpressionHelper.ReadEditWhereInExpression(Connection, ModelInfo,
                    expression, values));
            }

            return this;
        }

        public SqlWhere<TP, T> Add(string expression){
            AddWhere(expression);
            return this;
        }

        public SqlWhere<TP, T> Add<T2>(string expression, Expression<Func<T, T2>> parameters){
            if (WhereType == SqlWhereType.Query){
                AddWhere(DbExpressionHelper.ReadQueryAnyExpression(Connection, ModelInfo, expression, parameters));
            }
            else{
                AddWhere(DbExpressionHelper.ReadEditAnyExpression(Connection, ModelInfo, expression, parameters));
            }

            return this;
        }

        public SqlWhere<SqlWhere<TP, T>, T> WhereBegin(SqlWhereJoinType joinType = SqlWhereJoinType.And){
            var where = new SqlWhere<SqlWhere<TP, T>, T>(this, joinType);
            AddWhere(where);
            return where;
        }

        public TP WhereEnded(){
            return _parent;
        }

        public TP WhereEnded(string level){
            return _parent;
        }
    }


    public enum SqlWhereJoinType
    {
        And,
        Or
    }

    public enum SqlWhereType
    {
        Query,
        Delete,
        Update
    }

    public enum SqlCompareType
    {
        Equal,
        NotEqual,
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual
    }

    public enum SqlLikeType
    {
        Before,
        After,
        Middle,
        Equal,
    }
}