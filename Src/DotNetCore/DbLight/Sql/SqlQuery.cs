using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DbLight.Common;


namespace DbLight.Sql
{
    public class SqlQuery
    {
        public DbConnection Connection{ get; protected set; }
        public bool IsChildQuery{ get; set; }
        public DbModelInfo ModelInfo{ get; protected set; }

        protected DbContext _context;
        protected string _sql;

        public Task<DataTable> ToDataTableAsync(){
            return _context.ExecQueryToDataTableAsync(ToString());
        }

        protected SqlQuery(){
        }

        public SqlQuery(string sql){
            _sql = sql;
        }
       
        public SqlQuery(string sql, DbContext context) : this(){
            _sql = sql;
            _context = context;
        }

        
        public T1 To<T1>(){
            return default(T1);
        }

        public bool In<T1>(T1 value){
            return true;
        }

        public override string ToString(){
            return _sql;
        }
    }

    public class SqlQuery<T> : SqlQuery where T : new()
    {
        private bool _distinct;
        private int _top;

        private readonly List<(DbColumnModelInfo Column, string Expression)> _columns =
            new List<(DbColumnModelInfo Column, string Expression)>();

        private DbTableModelInfo _from;
        private string _fromExpression;
        private bool _withNoLock;

        private readonly List<(JoinType joinType, DbTableModelInfo table, string expression, string on)> _joins =
            new List<(JoinType joinType, DbTableModelInfo table, string expression, string on)>();

        private string _whereExpress;
        private SqlWhere<SqlQuery<T>, T> _where;

        private readonly List<DbColumnModelInfo> _groupBys = new List<DbColumnModelInfo>();

        private string _havingExpress;

        private readonly List<(SqlOrderByType OrderByType, DbColumnModelInfo Column)> _orderBys =
            new List<(SqlOrderByType OrderByType, DbColumnModelInfo Column)>();

        private SqlQuery(){
            ModelInfo = DbModelHelper.GetModelInfo(typeof(T));
            From();
        }

        public SqlQuery(DbConnection connection) : this(){
            Connection = connection;
        }

        public SqlQuery(DbConnection connection, DbContext context) : this(){
            Connection = connection;
            _context = context;
        }
        
        public SqlQuery(string sql){
            _sql = sql;
        }
       
        public SqlQuery(string sql, DbContext context) : this(){
            _sql = sql;
            _context = context;
        }

        public SqlQuery<T> Reset(){
            _distinct = false;
            _top = 0;
            _columns.Clear();
            From();
            _withNoLock = false;
            _joins.Clear();
            _whereExpress = null;
            _where = null;
            _groupBys.Clear();
            _havingExpress = null;
            _orderBys.Clear();
            return this;
        }


        public SqlQuery<T> Distinct(){
            _distinct = true;
            return this;
        }

        public SqlQuery<T> Top(int count){
            _top = count;
            return this;
        }

        private void AddColumn(DbColumnModelInfo column, string expression){
            _columns.Add((column, expression));
        }

        public SqlQuery<T> Select<T1>(Expression<Func<T, T1>> column, SqlQuery childQuery){
            return Select(column, childQuery.ToString());
        }

        public SqlQuery<T> Select<T1, T2>(Expression<Func<T, T1>> column, 
            string expression, Expression<Func<T, T2>> parameters){
            return Select(column, DbExpressionHelper.ReadQueryAnyExpression(Connection, ModelInfo, expression, parameters));
        }

        public SqlQuery<T> Select<T1>(Expression<Func<T, T1>> column, string expression){
            var items = DbExpressionHelper.ReadColumnExpression(column, ModelInfo);
            if (items.Count != 1){
                throw new Exception("Only support one property for custom express.\n" +
                                    "Error Expression: " + expression);
            }

            AddColumn(items[0], expression);
            return this;
        }

        public SqlQuery<T> Select<T1>(Expression<Func<T, T1>> columns){
            var items = DbExpressionHelper.ReadColumnExpression(columns, ModelInfo);
            foreach (var item in items){
                AddColumn(item, null);
            }

            return this;
        }

        public SqlQuery<T> SelectWithIgnore<T1, T2>(Expression<Func<T, T1>> columns,
            Expression<Func<T, T2>> ignoreColumns){
            var selectItems = DbExpressionHelper.ReadColumnExpression(columns, ModelInfo);
            var ignoreItems = DbExpressionHelper.ReadColumnExpression(ignoreColumns, ModelInfo);

            for (var i = selectItems.Count - 1; i >= 0; i--){
                var item = selectItems[i];
                if (ignoreItems.Find(x => x.Member == item.Member &&
                                          x.Column == item.Column) != null){
                    selectItems.RemoveAt(i);
                }
            }

            foreach (var item in selectItems){
                AddColumn(item, null);
            }

            return this;
        }

        public SqlQuery<T> Sum<T1>(Expression<Func<T, T1>> column){
            return Select(column, "SUM({0})", column);
        }

        public SqlQuery<T> Sum<T1, T2>(Expression<Func<T, T1>> column, Expression<Func<T, T2>> from){
            return Select(column, "SUM({0})", from);
        }

        public SqlQuery<T> Count<T1>(Expression<Func<T, T1>> column){
            return Select(column, "COUNT(*)", column);
        }

        public SqlQuery<T> Min<T1>(Expression<Func<T, T1>> column){
            return Select(column, "MIN({0})", column);
        }

        public SqlQuery<T> Min<T1, T2>(Expression<Func<T, T1>> column, Expression<Func<T, T2>> from){
            return Select(column, "MIN({0})", from);
        }

        public SqlQuery<T> Max<T1>(Expression<Func<T, T1>> column){
            return Select(column, "MAX({0})", column);
        }

        public SqlQuery<T> Max<T1, T2>(Expression<Func<T, T1>> column, Expression<Func<T, T2>> from){
            return Select(column, "MAX({0})", from);
        }

        public SqlQuery<T> From(){
            _from = new DbTableModelInfo();

            if (ModelInfo.Kind == DbModelKind.Tuple){
                _from.Member = ModelInfo.Members[0].ColumnName;
                _from.Table = ModelInfo.Members[0].Model.TableName;
                _from.Database = ModelInfo.Members[0].Model.DatabaseName;
            }
            else{
                _from.Member = "a";
                _from.Table = ModelInfo.TableName;
                _from.Database = ModelInfo.DatabaseName;
            }

            _fromExpression = null;
            return this;
        }

        public SqlQuery<T> From<T1>(Expression<Func<T, T1>> table){
            _from = DbExpressionHelper.ReadTableExpression(table, ModelInfo);
            _fromExpression = null;
            return this;
        }

        public SqlQuery<T> From<T1>(Expression<Func<T, T1>> table, string expression){
            _from = DbExpressionHelper.ReadTableExpression(table, ModelInfo);
            _fromExpression = expression;
            return this;
        }

        public SqlQuery<T> From<T1>(Expression<Func<T, T1>> table, SqlQuery childQuery){
            return From(table, childQuery.ToString());
        }

        private void AddJoin(JoinType joinType, DbTableModelInfo table, string expression, string on){
            _joins.Add((joinType, table, expression, on));
        }

        public SqlQuery<T> LeftJoin<T1>(Expression<Func<T, T1>> table, Expression<Func<T, bool>> on){
            var tableName = DbExpressionHelper.ReadTableExpression(table, ModelInfo);
            var onSql = DbExpressionHelper.ReadQueryWhereExpression(Connection, ModelInfo, on);
            AddJoin(JoinType.LeftJoin, tableName, null, onSql);
            return this;
        }

        public SqlQuery<T> LeftJoin<T1>(Expression<Func<T, T1>> table, string expression,
            Expression<Func<T, bool>> on){
            var tableName = DbExpressionHelper.ReadTableExpression(table, ModelInfo);
            var onSql = DbExpressionHelper.ReadQueryWhereExpression(Connection, ModelInfo, on);
            AddJoin(JoinType.LeftJoin, tableName, expression, onSql);
            return this;
        }

        public SqlQuery<T> LeftJoin<T1>(Expression<Func<T, T1>> table, SqlQuery childQuery,
            Expression<Func<T, bool>> on){
            return LeftJoin(table, childQuery.ToString(), on);
        }

        public SqlQuery<T> WithNoLock(){
            _withNoLock = true;
            return this;
        }

        public SqlQuery<T> Where(Expression<Func<T, bool>> exp){
            _whereExpress = DbExpressionHelper.ReadQueryWhereExpression(Connection, ModelInfo, exp);
            return this;
        }

        public SqlQuery<T> Where(string expression){
            _whereExpress = expression;
            return this;
        }

        public SqlWhere<SqlQuery<T>, T> WhereBegin(SqlWhereJoinType joinType = SqlWhereJoinType.And){
            _where = new SqlWhere<SqlQuery<T>, T>(this, joinType);
            return _where;
        }

        public SqlQuery<T> GroupBy<T1>(Expression<Func<T, T1>> columns){
            var items = DbExpressionHelper.ReadColumnExpression(columns, ModelInfo);
            foreach (var item in items){
                _groupBys.Add(item);
            }

            return this;
        }

        public SqlQuery<T> Having(Expression<Func<T, bool>> exp){
            _havingExpress = DbExpressionHelper.ReadQueryWhereExpression(Connection, ModelInfo, exp);
            return this;
        }

        public SqlQuery<T> OrderBy<T1>(Expression<Func<T, T1>> columns){
            return OrderBy(columns, SqlOrderByType.Asc);
        }

        public SqlQuery<T> OrderBy<T1>(Expression<Func<T, T1>> columns, SqlOrderByType orderByType){
            var items = DbExpressionHelper.ReadColumnExpression(columns, ModelInfo);
            foreach (var item in items){
                _orderBys.Add((orderByType, item));
            }

            return this;
        }
        
        public DataTable ToDataTable(){
            return _context.ExecQueryToDataTable(ToString());
        }
        
        public new Task<DataTable> ToDataTableAsync(){
            return _context.ExecQueryToDataTableAsync(ToString());
        }

        public Task<List<T>> ToListAsync(){
            return _context.ExecQueryToListAsync<T>(ToString());
        }

        public Task<List<TResult>> ToListAsync<TResult>(Func<T, TResult> converter){
            return _context.ExecQueryToListAsync(ToString(), converter);
        }

        public List<T> ToList(){
            return _context.ExecQueryToList<T>(ToString());
        }

        public List<TResult> ToList<TResult>(Func<T, TResult> converter){
            return _context.ExecQueryToList(ToString(), converter);
        }

        public new T1 To<T1>(){
            return default(T1);
        }

        public new bool In<T1>(T1 value){
            return true;
        }

        public override string ToString(){
            return ToSql();
        }

        private string ToSql(){
            if (_sql != null){
                return _sql;
            }

            var sql = new StringBuilder();

            //SELECT
            sql.Append("SELECT");

            if (_distinct){
                sql.Append(" DISTINCT");
            }

            if (_top > 0){
                sql.Append(" TOP " + _top);
            }

            //COLUMNS
            if (_columns.Count == 0){
                sql.Append(" *");
            }
            else{
                var isFirstAdd = true;
                foreach (var column in _columns){
                    string subSql;

                    //a.PostId
                    //a.Item
                    string displayName;
                    if (IsChildQuery){
                        displayName = column.Column.Column;
                    }
                    else{
                        if (column.Column.Member == "a"){
                            displayName = column.Column.Column;
                        }
                        else{
                            displayName = column.Column.Member + "." + column.Column.Column;
                        }
                    }

                    if (column.Expression == null){
                        subSql = string.Format("[{0}].[{1}] AS [{2}]", column.Column.Member, column.Column.Column,
                            displayName);
                    }
                    else{
                        subSql = string.Format("({0}) AS [{1}]", column.Expression, displayName);
                    }

                    sql.Append(isFirstAdd ? " " : ", ");
                    sql.Append(subSql);
                    isFirstAdd = false;
                }
            }

            //FROM
            sql.Append(" FROM");

            {
                if (_fromExpression == null){
                    sql.Append(string.Format(" {0} AS [{1}]",
                        Connection.GetTableFullName(_from.Database, _from.Table),
                        _from.Member));
                }
                else{
                    sql.Append(string.Format(" ({0}) AS [{1}]",
                        _fromExpression,
                        _from.Member));
                }
            }

            //LOCK
            if (_withNoLock){
                sql.Append(" WITH(NOLOCK)");
            }

            //JOIN
            _joins.ForEach(join => {
                string joinType = "";
                switch (join.joinType){
                    case JoinType.InnerJoin:
                        joinType = "INNER JOIN";
                        break;
                    case JoinType.LeftJoin:
                        joinType = "LEFT JOIN";
                        break;
                }

                string joinExpression;
                if (join.expression == null){
                    joinExpression = string.Format("{0} AS [{1}]",
                        Connection.GetTableFullName(join.table.Database, join.table.Table),
                        join.table.Member);
                }
                else{
                    joinExpression = string.Format("({0}) AS [{1}]", join.expression, join.table.Member);
                }

                sql.Append(" ");
                sql.Append(joinType);
                sql.Append(" ");
                sql.Append(joinExpression);
                sql.Append(" ON ");
                sql.Append(join.on);
            });

            //WHERE
            if (_where != null){
                var s = _where.ToString();
                if (s != ""){
                    sql.Append(" WHERE ");
                    sql.Append(s);
                }
            }
            else if (!string.IsNullOrEmpty(_whereExpress)){
                sql.Append(" WHERE ");
                sql.Append(_whereExpress);
            }

            //GROUP BY
            if (_groupBys.Count > 0){
                sql.Append(" GROUP BY");
                var isFirstAdd = true;
                foreach (var column in _groupBys){
                    string subSql = string.Format("[{0}].[{1}]", column.Member, column.Column);
                    sql.Append(isFirstAdd ? " " : ", ");
                    sql.Append(subSql);
                    isFirstAdd = false;
                }
            }

            //HAVING
            if (_havingExpress != null){
                sql.Append(" HAVING ");
                sql.Append(_havingExpress);
            }

            //ORDER BY
            if (_orderBys.Count > 0 && IsChildQuery == false){
                sql.Append(" ORDER BY");
                var isFirstAdd = true;
                foreach (var item in _orderBys){
                    string displayName;
                    if (item.Column.Member == "a"){
                        displayName = item.Column.Column;
                    }
                    else{
                        displayName = item.Column.Member + "." + item.Column.Column;
                    }

                    string subSql = string.Format("[{0}] {1}", displayName,
                        item.OrderByType == SqlOrderByType.Asc ? "ASC" : "DESC");
                    sql.Append(isFirstAdd ? " " : ", ");
                    sql.Append(subSql);
                    isFirstAdd = false;
                }
            }

            return sql.ToString();
        }

        private enum JoinType
        {
            LeftJoin,
            InnerJoin,
        }
    }

    public enum SqlOrderByType
    {
        Asc,
        Desc
    }
}