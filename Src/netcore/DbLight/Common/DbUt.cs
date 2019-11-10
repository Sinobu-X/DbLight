using System;
using System.Data;
using System.Collections.Generic;
using System.Linq.Expressions;
using DbLight.Exceptions;

namespace DbLight.Common
{
    public class DbUt
    {
        internal static string GetTableName(DbConnection connection, string database, string schema, string table){
            if (connection.DbType == DbDatabaseType.SqlServer){
                var item = connection.Groups.Find(x =>
                    x.virtualName.Equals(database, StringComparison.OrdinalIgnoreCase));
                if (item.virtualName == null){
                    return "[" + database + "]..[" + table + "]";
                }

                else{
                    return "[" + item.realName + "]..[" + table + "]";
                }
            }

            if (connection.DbType == DbDatabaseType.Postgres){
                var item = connection.Groups.Find(x =>
                    x.virtualName.Equals(schema, StringComparison.OrdinalIgnoreCase));
                if (item.virtualName == null){
                    return "\"" + (string.IsNullOrEmpty(schema) ? "public" : schema) + "\".\"" + table + "\"";
                }
                else{
                    return "\"" + item.realName + "\".\"" + table + "\"";
                }
            }

            throw new DbUnexpectedDbTypeException();
        }

        internal static string GetColumnName(DbConnection connection, string column){
            if (connection.DbType == DbDatabaseType.SqlServer){
                return "[" + column + "]";
            }

            if (connection.DbType == DbDatabaseType.Postgres){
                return "\"" + column + "\"";
            }

            throw new DbUnexpectedDbTypeException();
        }

        internal static string ValueToWhereSql(DbConnection connection, object value){
            if (value == null){
                throw new Exception("Value is NULL");
            }

            return ValueToSql(connection, value);
        }

        internal static string ValueToSetSql(DbConnection connection, object value){
            if (value == null){
                return "NULL";
            }

            return ValueToSql(connection, value);
        }

        private static string ValueToSql(DbConnection connection, object value){
            if (value is string vs){
                if (connection.DbType == DbDatabaseType.SqlServer){
                    return 'N' + "'" + vs.Replace("'", "''") + "'";
                }

                if (connection.DbType == DbDatabaseType.Postgres){
                    return "'" + vs.Replace("'", "''") + "'";
                }

                throw new DbUnexpectedDbTypeException();
            }
            else if (value is int){
                return value.ToString();
            }
            else if (value is bool bv){
                if (connection.DbType == DbDatabaseType.SqlServer){
                    return bv ? "1" : "0";
                }

                if (connection.DbType == DbDatabaseType.Postgres){
                    return bv ? "true" : "false";
                }

                throw new DbUnexpectedDbTypeException();
            }
            else if (value is DateTime dv){
                if (connection.DbType == DbDatabaseType.SqlServer){
                    return $"'{dv.ToString("yyyy-MM-dd HH:mm:ss")}'";
                }

                if (connection.DbType == DbDatabaseType.Postgres){
                    return $"'{dv.ToString("yyyy-MM-dd HH:mm:ss")}'";
                }

                throw new DbUnexpectedDbTypeException();
            }
            else if (value is decimal ||
                     value is double ||
                     value is float ||
                     value is long ||
                     value is Int16){
                return value.ToString();
            }
            else if (value is byte[] bytes){
                if (connection.DbType == DbDatabaseType.SqlServer){
                    return "0x" + BitConverter.ToString(bytes).Replace("-", "");
                }

                if (connection.DbType == DbDatabaseType.Postgres){
                    return "decode('" + BitConverter.ToString(bytes).Replace("-", "") + "', 'hex')";
                }

                throw new DbUnexpectedDbTypeException();
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        internal static string ValueToLikeSql(DbConnection connection, DbWhereLikeType likeType, string value){
            if (connection.DbType == DbDatabaseType.SqlServer){
                switch (likeType){
                    case DbWhereLikeType.Before:
                        return $"N'{value.Replace("'", "''")}%'";
                    case DbWhereLikeType.After:
                        return $"N'%{value.Replace("'", "''")}'";
                    case DbWhereLikeType.Middle:
                        return $"N'%{value.Replace("'", "''")}%'";
                    default:
                        throw new DbUnknownException("Unexpected Like Type.\n" +
                                                     "Like Type: " + likeType);
                }
            }

            if (connection.DbType == DbDatabaseType.Postgres){
                switch (likeType){
                    case DbWhereLikeType.Before:
                        return $"'{value.Replace("'", "''")}%'";
                    case DbWhereLikeType.After:
                        return $"'%{value.Replace("'", "''")}'";
                    case DbWhereLikeType.Middle:
                        return $"'%{value.Replace("'", "''")}%'";
                    default:
                        throw new DbUnknownException("Unexpected Like Type.\n" +
                                                     "Like Type: " + likeType);
                }
            }

            throw new DbUnexpectedDbTypeException();
        }

        internal static string ToLikeSql(DbConnection connection, string express, DbWhereLikeType likeType, string value){
            var likeValue = ValueToLikeSql(connection, likeType, value);
            return $"{express} LIKE {likeValue}";
        }

        public static string IdentifyInsertOnSql(DbConnection connection, string table){
            if (connection.DbType == DbDatabaseType.SqlServer){
                return string.Format("SET IDENTITY_INSERT {0} ON", table);
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        public static string IdentifyInsertOffSql(DbConnection connection, string table){
            if (connection.DbType == DbDatabaseType.SqlServer){
                return string.Format("SET IDENTITY_INSERT {0} OFF", table);
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }
    }
}