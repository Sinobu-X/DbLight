using System;
using System.Diagnostics;
using DbLight;
using DbLight.Common;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DbLightTest.Postgres
{
    public class TestBase
    {
        protected DbConnection GetConnection(){
            return new DbConnection(DbDatabaseType.Postgres,
                "Host=127.0.0.1;Username=test;Password=test;Database=dblight");
        }


    }
}