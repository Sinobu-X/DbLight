using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DbLight.Common;
using DbLight.Exceptions;

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
        private readonly List<DbColumnModelInfo> _includeColumns = new List<DbColumnModelInfo>();
        private readonly List<DbColumnModelInfo> _excludeColumns = new List<DbColumnModelInfo>();
        private bool _closeIdentify;
        private string _fromSql;

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

        public SqlInsert(DbConnection connection, DbContext context) : this(){
            Connection = connection;
            _context = context;
        }

        public SqlInsert<T> Include<T1>(Expression<Func<T, T1>> columns){
            var items = DbExpressionHelper.ReadColumnExpression(columns, ModelInfo);

            foreach (var item in items){
                _includeColumns.Add(item);
            }

            return this;
        }

        [Obsolete("Select is deprecated, please use Exclude instead.")]
        public SqlInsert<T> Ignore<T1>(Expression<Func<T, T1>> columns){
            return Exclude(columns);
        }
        
        public SqlInsert<T> Exclude<T1>(Expression<Func<T, T1>> columns){
            var items = DbExpressionHelper.ReadColumnExpression(columns, ModelInfo);

            foreach (var item in items){
                _excludeColumns.Add(item);
            }

            return this;
        }

        public SqlInsert<T> CloseIdentify(){
            _closeIdentify = true;
            return this;
        }

        public SqlInsert<T> From<TX>(SqlQuery<TX> query) where TX : new(){
            _fromSql = query.ToString();
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

            if (_includeColumns.Count > 0){
                var temps = new List<DbMemberInfo>();

                var members1 = members;
                _includeColumns.ForEach(x => {
                    var member = members1.Find(y => y.ColumnName.Equals(x.Column, StringComparison.OrdinalIgnoreCase));
                    if (member == null){
                        throw new DbArgumentException($"Column [{x.Column}] not found in the table struct.");
                    }
                    temps.Add(member);
                });

                members = temps;
            }
            else{
                members = members.FindAll(x => {
                    if (_excludeColumns.Exists(y => y.Column.Equals(x.ColumnName, StringComparison.OrdinalIgnoreCase))){
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
            }

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
            sql.Append(") ");

            if (_fromSql == null){
                //COLUMNS END & VALUES BEGIN
                sql.Append("VALUES(");

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
            }
            else{
                sql.Append(_fromSql);
            }

            return sql.ToString();
        }
    }
}