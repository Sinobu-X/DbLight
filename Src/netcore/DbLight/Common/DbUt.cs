using System;
using System.Data;
using System.Collections.Generic;
using System.Linq.Expressions;
using DbLight.Exceptions;

namespace DbLight.Common
{
    public class DbUt
    {
        public static string ValueToWhereSql(DbConnection connection, object value){
            if (value == null){
                throw new Exception("Value is NULL");
            }

//            var valueType = value.GetType();
//            if (valueType.IsPrimitive){
//                return ValueToSql(connection, value, valueType);
//            }
//            else if (valueType == typeof(decimal)){
//                return ValueToSql(connection, value, valueType);
//            }
//            else if (valueType == typeof(DateTime)){
//                return ValueToSql(connection, value, valueType);
//            }
//            else if (valueType == typeof(string)){
//                return ValueToSql(connection, value, valueType);
//            }
//            else{
//                throw new Exception("Unknown data type for create edit sql.\n" +
//                                    "Value = " + value + ", Type = " + valueType + "\n" +
//                                    "Only supports the following data types in where part:\n" +
//                                    "Primitive\n" +
//                                    "decimal\n" +
//                                    "DateTime\n" +
//                                    "string");
//            }
            return ValueToSql(connection, value);
        }

        public static string ValueToSetSql(DbConnection connection, object value){
            if (value == null){
                return "NULL";
            }
            
            return ValueToSql(connection, value);

//            var valueType = value.GetType();
//            if (valueType.IsPrimitive){
//                return ValueToSql(connection, value, valueType);
//            }
//            else if (valueType == typeof(decimal)){
//                return ValueToSql(connection, value, valueType);
//            }
//            else if (valueType == typeof(DateTime)){
//                return ValueToSql(connection, value, valueType);
//            }
//            else if (valueType == typeof(string)){
//                return ValueToSql(connection, value, valueType);
//            }
//            else if (valueType == typeof(byte[])){
//                return BytesToSetSql(connection, (byte[]) value);
//            }
//            else{
//                throw new Exception("Unknown data type for create edit sql.\n" +
//                                    "Value = " + value + ", Type = " + valueType + "\n" +
//                                    "Only supports the following data types:\n" +
//                                    "Primitive\n" +
//                                    "decimal\n" +
//                                    "DateTime\n" +
//                                    "string\n" +
//                                    "byte[]");
//            }
        }

        private static string ValueToSql(DbConnection connection, object value){
            if (value is string vs){
                if (connection.DbType == DbDatabaseType.SqlServer){
                    return 'N' + "'" + vs.Replace("'", "''") + "'";
                }
                else{
                    throw new DbUnexpectedDbTypeException();
                }
            }
            else if (value is int){
                return value.ToString();
            }
            else if (value is bool bv){
                return bv ? "1" : "0";
            }
            else if (value is DateTime dv){
                if (connection.DbType == DbDatabaseType.SqlServer){
                    return dv.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else{
                    throw new DbUnexpectedDbTypeException();
                }
            }
            else if (value is decimal ||
                     value is double ||
                     value is float ||
                     value is long ||
                     value is Int16){
                return value.ToString();
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

//        private static string ValueToSql(DbConnection connection, object value, Type valueType){
//            if (valueType.IsPrimitive){
//                //basic type
//                if (valueType == typeof(bool)){
//                    return ((bool) value) ? "1" : "0";
//                }
//                else{
//                    return value.ToString();
//                }
//            }
//            else if (valueType == typeof(decimal)){
//                return value.ToString();
//            }
//            else if (valueType == typeof(DateTime)){
//                if (connection.DbType == DbDatabaseType.SqlServer){
//                    return ((DateTime) value).ToString("'yyyy-MM-dd HH:mm:ss'");
//                }
//                else{
//                    throw new DbUnexpectedDbTypeException();
//                }
//            }
//            else if (valueType == typeof(string)){
//                var s = (string) value;
//                if (connection.DbType == DbDatabaseType.SqlServer){
//                    return 'N' + "'" + s.Replace("'", "''") + "'";
//                }
//                else{
//                    throw new DbUnexpectedDbTypeException();
//                }
//            }
//            else{
//                throw new DbCrashException("Failed to get here.");
//            }
//        }

        public static string BytesToSetSql(DbConnection connection, byte[] v){
            if (connection.DbType == DbDatabaseType.SqlServer){
                if (v != null && v.Length > 0){
                    return "0x" + BitConverter.ToString(v).Replace("-", "");
                }
                else{
                    return "NULL";
                }
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
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