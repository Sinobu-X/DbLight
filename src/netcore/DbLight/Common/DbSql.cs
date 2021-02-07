using DbLight.Exceptions;

namespace DbLight.Common
{
    public class DbSql
    {
        private static IDbSqlInner _mssqlInner;
        private static IDbSqlInner _postgresInner;

        private static IDbSqlInner GetInner(DbConnection connection){
            if (connection.DbType == DbDatabaseType.SqlServer){
                if (_mssqlInner == null){
                    _mssqlInner = new Provider.MSSQL.DbSqlInner();
                }

                return _mssqlInner;
            }

            if (connection.DbType == DbDatabaseType.Postgres){
                if (_postgresInner == null){
                    _postgresInner = new Provider.Postgres.DbSqlInner();
                }

                return _postgresInner;
            }

            throw new DbUnexpectedDbTypeException();
        }

        internal static string GetTableName(DbConnection connection, string database, string schema, string table){
            return GetInner(connection).GetTableName(connection, database, schema, table);
        }

        internal static string GetColumnName(DbConnection connection, string column){
            return GetInner(connection).GetColumnName(column);
        }

        internal static string ValueToWhereSql(DbConnection connection, object value){
            return GetInner(connection).ValueToWhereSql(value);
        }

        internal static string ValueToSetSql(DbConnection connection, object value){
            return GetInner(connection).ValueToSetSql(value);
        }

        internal static string ValueToLikeSql(DbConnection connection, DbWhereLikeType likeType, string value){
            return GetInner(connection).ValueToLikeSql(likeType, value);
        }

        internal static string ToLikeSql(DbConnection connection, string express, DbWhereLikeType likeType,
            string value){
            return GetInner(connection).ToLikeSql(express, likeType, value);
        }

        internal static string TopSqlTop(DbConnection connection, int top){
            return GetInner(connection).TopSqlTop(top);
        }

        internal static string TopSqlWhere(DbConnection connection, int top){
            return GetInner(connection).TopSqlWhere(top);
        }

        internal static string TopSqlLimit(DbConnection connection, int top){
            return GetInner(connection).TopSqlLimit(top);
        }
        
        internal static string OffsetSql(DbConnection connection, int offset){
            return GetInner(connection).OffsetSql(offset);
        }
    }
}