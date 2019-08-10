using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DbLight.Common;

namespace DbLight.Sql
{
    public abstract class SqlUpdate
    {
        public DbConnection Connection{ get; protected set; }
        public DbModelInfo ModelInfo{ get; protected set; }
    }

    public class SqlUpdate<T> : SqlUpdate
    {
        private readonly DbContext _context;
        private readonly DbTableModelInfo _from;
        private T _item;
        private readonly List<DbColumnModelInfo> _columns = new List<DbColumnModelInfo>();
        private bool _closeIdentify;
        private string _whereExpress;
        private SqlWhere<SqlUpdate<T>, T> _where;

        private SqlUpdate(){
            ModelInfo = DbModelHelper.GetModelInfo(typeof(T));
            _from = new DbTableModelInfo();

            if (ModelInfo.Kind == DbModelKind.Tuple){
                _from.Table = ModelInfo.Members[0].Model.TableName;
                _from.Database = ModelInfo.Members[0].Model.DatabaseName;
            }
            else{
                _from.Table = ModelInfo.TableName;
                _from.Database = ModelInfo.DatabaseName;
            }
        }

        public SqlUpdate(DbConnection connection, T item) : this(){
            Connection = connection;
            _item = item;
        }

        public SqlUpdate(DbConnection connection, DbContext context, T item) : this(){
            Connection = connection;
            _context = context;
            _item = item;
        }

        public SqlUpdate<T> Select<T1>(Expression<Func<T, T1>> columns){
            return Select<T1, T1>(columns, null);
        }

        public SqlUpdate<T> Select<T1, T2>(Expression<Func<T, T1>> columns,
            Expression<Func<T, T2>> ignoreColumns){
            var selectItems = DbExpressionHelper.ReadColumnExpression(columns, ModelInfo);
            var ignoreItems = ignoreColumns != null
                ? DbExpressionHelper.ReadColumnExpression(ignoreColumns, ModelInfo)
                : new List<DbColumnModelInfo>();

            for (var i = selectItems.Count - 1; i >= 0; i--){
                var item = selectItems[i];
                if (ignoreItems.Exists(x => x.Column == item.Column)){
                    selectItems.RemoveAt(i);
                }
            }
            
            foreach (var item in selectItems){
                _columns.Add(item);
            }

            return this;
        }

        public SqlUpdate<T> CloseIdentify(){
            _closeIdentify = true;
            return this;
        }
        
        
        public SqlUpdate<T> SetData(T item){
            _item = item;
            return this;
        }


        public SqlUpdate<T> Where(Expression<Func<T, bool>> expression){
            _whereExpress = DbExpressionHelper.ReadEditWhereExpression(Connection, ModelInfo, expression);
            return this;
        }

        public SqlWhere<SqlUpdate<T>, T> WhereBegin(SqlWhereJoinType joinType = SqlWhereJoinType.And){
            return WhereBegin("", joinType);
        }

        public SqlWhere<SqlUpdate<T>, T> WhereBegin(string level, SqlWhereJoinType joinType = SqlWhereJoinType.And){
            _where = new SqlWhere<SqlUpdate<T>, T>(this, joinType);
            return _where;
        }

        public Task<int> ExecuteAsync(){
            return _context.ExecNoQueryAsync(ToString());
        }
        
        public int Execute(){
            return _context.ExecNoQuery(ToString());
        }

        public override string ToString(){
            return ToSql();
        }

        private string ToSql(){
            List<DbMemberInfo> members;
            if (ModelInfo.Kind == DbModelKind.Tuple){
                members = ModelInfo.Members[0].Model.Members;
            }
            else{
                members = ModelInfo.Members;
            }

            members = members.FindAll(x => {
                if (!_columns.Exists(y => y.Column.ToUpper() == x.ColumnName.ToUpper())){
                    return false;
                }

                if (x.NotMapped){
                    return false;
                }

                if (x.Model.Kind != DbModelKind.Value){
                    return false;
                }

                if (x.Identity && _closeIdentify == false){
                    return false;
                }

                return true;
            });

            var sql = new StringBuilder();

            //START
            sql.Append("UPDATE ");

            //TABLE
            sql.Append(Connection.GetTableFullName(_from.Database, _from.Table));

            //SET
            sql.Append(" SET ");

            {
                bool isFirst = true;
                foreach (var column in members){
                    sql.Append(isFirst ? "" : ", ");
                    isFirst = false;
                    var value = column.PropertyInfo.GetValue(_item);
                    sql.Append(string.Format("[{0}] = {1}",
                        column.ColumnName,
                        DbUt.ValueToSetSql(Connection, value)));
                }
            }

            //WHERE
            if (_where != null){
                var s = _where.ToString();
                if (!string.IsNullOrEmpty(s)){
                    sql.Append(" WHERE ");
                    sql.Append(s);
                }
            }
            else if (!string.IsNullOrEmpty(_whereExpress)){
                sql.Append(" WHERE ");
                sql.Append(_whereExpress);
            }

            return sql.ToString();
        }
    }
}