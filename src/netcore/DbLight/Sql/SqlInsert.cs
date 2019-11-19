using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DbLight.Common;

namespace DbLight.Sql
{
    public abstract class SqlInsert
    {
        public DbConnection Connection{ get; protected set; }
        public DbModelInfo ModelInfo{ get; protected set; }
    }

    public class SqlInsert<T> : SqlInsert
    {
        private readonly DbContext _context;
        private readonly DbTableModelInfo _from;
        private readonly T _item;
        private readonly List<DbColumnModelInfo> _ignoreColumns = new List<DbColumnModelInfo>();
        private bool _closeIdentify;

        private SqlInsert(){
            ModelInfo = DbModelHelper.GetModelInfo(typeof(T));
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

        public SqlInsert(DbConnection connection, T item) : this(){
            Connection = connection;
            _item = item;
        }

        public SqlInsert(DbConnection connection, DbContext context, T item) : this(){
            Connection = connection;
            _context = context;
            _item = item;
        }

        public SqlInsert<T> Ignore<T1>(Expression<Func<T, T1>> columns){
            var ignoreItems = DbExpressionHelper.ReadColumnExpression(columns, ModelInfo);

            foreach (var item in ignoreItems){
                _ignoreColumns.Add(item);
            }

            return this;
        }

        public SqlInsert<T> CloseIdentify(){
            _closeIdentify = true;
            return this;
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
                if (_ignoreColumns.Exists(y => y.Column.Equals(x.ColumnName, StringComparison.OrdinalIgnoreCase))){
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
            sql.Append("INSERT INTO ");

            //TABLE
            sql.Append(DbSql.GetTableName(Connection, _from.Database, _from.Schema, _from.Table));

            //COLUMNS BEGIN
            sql.Append("(");

            //COLUMNS
            {
                var isFirst = true;
                foreach (var column in members){
                    sql.Append(isFirst ? "" : ", ");
                    isFirst = false;
                    sql.Append(DbSql.GetColumnName(Connection, column.ColumnName));
                }
            }
            sql.Append("");

            //COLUMNS END & VALUES BEGIN
            sql.Append(") VALUES(");

            //VALUES
            {
                var isFirst = true;
                foreach (var column in members){
                    sql.Append(isFirst ? "" : ", ");
                    isFirst = false;
                    var value = column.PropertyInfo.GetValue(_item);
                    sql.Append(DbSql.ValueToSetSql(Connection, value));
                }
            }

            //VALUES END
            sql.Append(")");

            return sql.ToString();
        }
    }
}