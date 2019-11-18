using System;
using DbLight.Common;
using DbLight.Exceptions;

namespace DbLight.Provider.MSSQL
{
    internal class DbSqlInner : IDbSqlInner
    {
        public string GetTableName(DbConnection connection, string database, string schema, string table){
            if (string.IsNullOrEmpty(database)){
                return $"[{table}]";
            }
            else{
                var item = connection.Groups.Find(x =>
                    x.virtualName.Equals(database, StringComparison.OrdinalIgnoreCase));
                if (item.virtualName == null){
                    return $"[{database}]..[{table}]";
                }
                else{
                    return $"[{item.realName}]..[{table}]";
                }
            }
        }

        public string GetColumnName(string column){
            return "[" + column + "]";
        }

        public string ValueToWhereSql(object value){
            if (value == null){
                throw new Exception("Value is NULL");
            }

            return ValueToSql(value);
        }

        public string ValueToSetSql(object value){
            if (value == null){
                return "NULL";
            }

            return ValueToSql(value);
        }

        private string ValueToSql(object value){
            if (value is string vs){
                return 'N' + "'" + vs.Replace("'", "''") + "'";
            }
            else if (value is int){
                return value.ToString();
            }
            else if (value is bool bv){
                return bv ? "1" : "0";
            }
            else if (value is DateTime dv){
                return $"'{dv.ToString("yyyy-MM-dd HH:mm:ss")}'";
            }
            else if (value is decimal ||
                     value is double ||
                     value is float ||
                     value is long ||
                     value is Int16){
                return value.ToString();
            }
            else if (value is byte[] bytes){
                return "0x" + BitConverter.ToString(bytes).Replace("-", "");
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        public string ValueToLikeSql(DbWhereLikeType likeType, string value){
            switch (likeType){
                case DbWhereLikeType.Before:
                    return $"N'{value.Replace("'", "''")}%'";
                case DbWhereLikeType.After:
                    return $"N'%{value.Replace("'", "''")}'";
                case DbWhereLikeType.Middle:
                    return $"N'%{value.Replace("'", "''")}%'";
                case DbWhereLikeType.Equal:
                    return $"N'{value.Replace("'", "''")}'";
                default:
                    throw new DbUnknownException("Unexpected Like Type.\n" +
                                                 "Like Type: " + likeType);
            }
        }

        public string ToLikeSql(string express, DbWhereLikeType likeType, string value){
            var likeValue = ValueToLikeSql(likeType, value);
            return $"{express} LIKE {likeValue}";
        }

        public string TopSqlTop(int top){
            return " TOP " + top;
        }

        public string TopSqlWhere(int top){
            return null;
        }

        public string TopSqlLimit(int top){
            return null;
        }
    }
}