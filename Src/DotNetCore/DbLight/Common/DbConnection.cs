using System;
using System.Collections.Generic;
using DbLight.Exceptions;

namespace DbLight.Common
{
    public class DbConnection
    {
        public DbDatabaseType DbType{ get; }
        public string ConnectionString{ get; set; }
        public string TestConnectionString{ get; set; }
        public bool TestConnection{ get; set; } = true;
        public SqlServerInfo SqlServer{ get; set; }
        public AccessInfo Access{ get; set; }
        public OracleInfo Oracle{ get; set; }
        public List<(string virtualName, string realName)> Groups{ get; set; } = new List<(string, string)>();

        public DbConnection(DbDatabaseType dbType){
            DbType = dbType;
        }

        public DbConnection(DbDatabaseType dbType, string connectionString){
            DbType = dbType;
            ConnectionString = connectionString;
        }

        public DbConnection(DbDatabaseType dbType, string connectionString, string testConnectionString){
            DbType = dbType;
            ConnectionString = connectionString;
            TestConnectionString = testConnectionString;
        }

        public class SqlServerInfo
        {
            public string ServerName{ get; set; } = "";
            public string UserName{ get; set; } = "";
            public string Password{ get; set; } = "";
            public string Database{ get; set; } = "";
            public bool IntegratedSecurity{ get; set; } = false;
        }

        public class AccessInfo
        {
            public string FileName{ get; set; } = "";
        }

        public class OracleInfo
        {
        }

        public string GetTableFullName(string database, string table){
            if (DbType == DbDatabaseType.SqlServer){
                var item = Groups.Find(x => x.virtualName.Equals(database, StringComparison.OrdinalIgnoreCase));
                if (item.virtualName == null){
                    return "[" + database + "]..[" + table + "]";
                }
                else{
                    return "[" + item.realName + "]..[" + table + "]";
                }
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }
    }
}