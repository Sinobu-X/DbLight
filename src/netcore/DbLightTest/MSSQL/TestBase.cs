using System;
using System.Diagnostics;
using DbLight;
using DbLight.Common;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DbLightTest.MSSQL
{
    public class TestBase
    {
        protected DbConnection GetConnection(){
            return new DbConnection(DbDatabaseType.SqlServer,
                "server=127.0.0.1;uid=test;pwd=test;database=DbLight");
        }
    }
}