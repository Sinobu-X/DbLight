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
    public class TestDb : TestBase
    {
        [Test]
        public void Connection(){
            var db = new DbContext(GetConnection());
            var list = db.ExecQueryToList<User>("SELECT * FROM \"user\"");
            Console.WriteLine(JsonConvert.SerializeObject(list));
        }

        [Test]
        public void Speed(){
            for (var i = 0; i < 10; i++){
                var sw = new Stopwatch();
                sw.Start();
                var db = new DbContext(GetConnection());
                var list = db.ExecQueryToList<User>("SELECT * FROM \"user\"");
                Console.WriteLine(JsonConvert.SerializeObject(list));
                Console.WriteLine(sw.ElapsedMilliseconds);
                sw.Stop();
            }
        }
    }
}