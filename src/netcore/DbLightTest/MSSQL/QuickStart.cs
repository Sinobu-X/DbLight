using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DbLight;
using DbLight.Common;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DbLightTest.MSSQL
{
    public class QuickStart
    {
        [Test]
        public async Task QueryAsync(){
            var cn = new DbConnection(DbDatabaseType.SqlServer,
                "server=127.0.0.1;uid=test;pwd=test;database=DbLight");
            var db = new DbContext(cn);
            var users = await db.Query<User>()
                .Where(x => x.UserId >= 4 && x.UserId < 10)
                .ToListAsync();

            Console.WriteLine(JsonConvert.SerializeObject(users));
        }
    }
}