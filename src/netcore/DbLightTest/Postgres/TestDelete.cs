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
    public class TestDelete
    {
        [Test]
        public async Task ByInt1(){
            var db = new DbContext(QuickStart.BuildConnection());

            var del = db.Delete<User>().Where(x => x.UserId == 12);

            Console.WriteLine(del.ToString());

            var count = await del.ExecuteAsync();
            Console.WriteLine($"Count = {count}");
        }
        
        [Test]
        public async Task ByInt2(){
            var db = new DbContext(QuickStart.BuildConnection());

            var del = db.Delete<User>()
                .WhereBegin()
                .Compare(x => x.UserId, SqlCompareType.Equal, 12)
                .WhereEnded();

            Console.WriteLine(del.ToString());

            var count = await del.ExecuteAsync();
            Console.WriteLine($"Count = {count}");
        }

        [Test]
        public async Task ByString1(){
            var db = new DbContext(QuickStart.BuildConnection());

            var del = db.Delete<User>().Where(x => x.UserName == "abc");

            Console.WriteLine(del.ToString());

            var count = await del.ExecuteAsync();
            Console.WriteLine($"Count = {count}");
        }

        [Test]
        public async Task ByString2(){
            var db = new DbContext(QuickStart.BuildConnection());

            var del = db.Delete<User>()
                .WhereBegin()
                .Compare(x => x.UserName, SqlCompareType.Equal, "abc")
                .WhereEnded();

            Console.WriteLine(del.ToString());

            var count = await del.ExecuteAsync();
            Console.WriteLine($"Count = {count}");
        }


    }
}