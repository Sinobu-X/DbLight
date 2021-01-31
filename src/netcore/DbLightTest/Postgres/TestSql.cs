using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DbLight;
using DbLight.Common;
using DbLight.Sql;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DbLightTest.Postgres
{
    public class TestSql
    {
        [Test]
        public void QueryBool(){
            var db = new DbContext(QuickStart.BuildConnection());

           var exist = db.Query<(bool Item1, bool)>("select true as Item1").ToFirst<bool>(x => x.Item1);
           Console.WriteLine(exist);
        }
    }
}