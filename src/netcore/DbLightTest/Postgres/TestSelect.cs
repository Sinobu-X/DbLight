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
    public class TestSelect : TestBase
    {
        [Test]
        public void ById(){
            var db = new DbContext(GetConnection());
            var query = db.Query<User>()
                .SelectWithIgnore(x => x, x => x.Photo);

            Console.WriteLine(query.ToString());
            Console.WriteLine(JsonConvert.SerializeObject(query.ToList()));
        }
    }
}