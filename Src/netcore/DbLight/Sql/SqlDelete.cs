using System;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DbLight.Common;

namespace DbLight.Sql
{
    public abstract class SqlDelete
    {
        public DbConnection Connection{ get; protected set; }
        public DbModelInfo ModelInfo{ get; protected set; }
    }

    public class SqlDelete<T> : SqlDelete where T : new()
    {
        private readonly DbContext _context;
        private DbTableModelInfo _from;

        private string _whereExpress;
        private SqlWhere<SqlDelete<T>, T> _where;

        private SqlDelete(){
            ModelInfo = DbModelHelper.GetModelInfo(typeof(T));
            From();
        }

        public SqlDelete(DbConnection connection) : this(){
            Connection = connection;
        }

        public SqlDelete(DbConnection connection, DbContext context) : this(){
            Connection = connection;
            _context = context;
        }

        public SqlDelete<T> Reset(){
            From();
            return this;
        }

        private void From(){
            _from = new DbTableModelInfo();

            if (ModelInfo.Kind == DbModelKind.Tuple){
                _from.Table = ModelInfo.Members[0].Model.TableName;
                _from.Schema = ModelInfo.Members[0].Model.SchemaName;
                _from.Database = ModelInfo.Members[0].Model.DatabaseName;
            }
            else{
                _from.Table = ModelInfo.TableName;
                _from.Schema = ModelInfo.SchemaName;
                _from.Database = ModelInfo.DatabaseName;
            }
        }

        public SqlDelete<T> Where(Expression<Func<T, bool>> expression){
            _whereExpress = DbExpressionHelper.ReadEditWhereExpression(Connection, ModelInfo, expression);
            return this;
        }

        public SqlWhere<SqlDelete<T>, T> WhereBegin(SqlWhereJoinType joinType = SqlWhereJoinType.And){
            return WhereBegin("", joinType);
        }

        public SqlWhere<SqlDelete<T>, T> WhereBegin(string level, SqlWhereJoinType joinType = SqlWhereJoinType.And){
            _where = new SqlWhere<SqlDelete<T>, T>(this, joinType);
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
            var sql = new StringBuilder();

            //START
            sql.Append("DELETE FROM ");

            //FROM
            sql.Append(DbSql.GetTableName(Connection, _from.Database, _from.Schema, _from.Table));

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