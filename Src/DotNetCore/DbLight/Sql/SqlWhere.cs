using System;
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
        private readonly List<string> _wheres = new List<string>();
        private SqlWhere _subWhere;

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
            _wheres.Add(null);
            _subWhere = where;
        }

        public override string ToString(){
            var sql = new StringBuilder();

            var rows = new List<string>();
            _wheres.ForEach(x => {
                if (x == null && _subWhere != null){
                    var s = _subWhere.ToString();
                    if (s != ""){
                        rows.Add(s);
                    }
                }
                else{
                    if (x != ""){
                        rows.Add(x);
                    }
                }
            });

            var isFirst = true;
            rows.ForEach(x => {
                sql.Append(isFirst ? "" : JoinType == SqlWhereJoinType.And ? " AND " : " OR ");
                isFirst = false;

                bool needBracket = false;
                if (JoinType == SqlWhereJoinType.And && x.ToUpper().IndexOf(" OR ", StringComparison.Ordinal) >= 0){
                    needBracket = true;
                }
                else if (JoinType == SqlWhereJoinType.Or && x.ToUpper().IndexOf(" AND ", StringComparison.Ordinal) >= 0){
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

        public SqlWhere<SqlWhere<T>, T> WhereStart(SqlWhereJoinType joinType = SqlWhereJoinType.And){
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
            if (_parent is SqlQuery){
                var q = _parent as SqlQuery;
                Connection = q.Connection;
                ModelInfo = q.ModelInfo;
                WhereType = SqlWhereType.Query;
            }
            else if (_parent is SqlDelete){
                var d = _parent as SqlDelete;
                Connection = d.Connection;
                ModelInfo = d.ModelInfo;
                WhereType = SqlWhereType.Delete;
            }
            else if (_parent is SqlUpdate){
                var d = _parent as SqlUpdate;
                Connection = d.Connection;
                ModelInfo = d.ModelInfo;
                WhereType = SqlWhereType.Update;
            }
            else if (_parent is SqlWhere){
                var d = _parent as SqlWhere;
                Connection = d.Connection;
                ModelInfo = d.ModelInfo;
                WhereType = d.WhereType;
            }
            else{
                throw new Exception("P only be SqlQuery, SqlDelete, SqlUpdate, SqlWhere.");
            }
        }

        public SqlWhere<TP, T> Add(Expression<Func<T, bool>> expression){
            AddWhere(expression);
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

        public SqlWhere<SqlWhere<TP, T>, T> WhereStart(SqlWhereJoinType joinType = SqlWhereJoinType.And){
            return WhereStart("", joinType);
        }

        public SqlWhere<SqlWhere<TP, T>, T> WhereStart(string level, SqlWhereJoinType joinType = SqlWhereJoinType.And){
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
}