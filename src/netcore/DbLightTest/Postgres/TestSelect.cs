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
        public void WhereId(){
            var db = new DbContext(GetConnection());
            var query = db.Query<User>()
                .SelectWithIgnore(x => x, x => x.Photo);

            Console.WriteLine(query.ToString());
            Console.WriteLine(JsonConvert.SerializeObject(query.ToList()));
        }

        [Test]
        public void IgnoreColumn(){
            var db = new DbContext(GetConnection());
            var query = db.Query<User>()
                .SelectWithIgnore(x => x, x => new{
                    x.Photo,
                    x.Height
                });

            Console.WriteLine(query.ToString());
            Console.WriteLine(JsonConvert.SerializeObject(query.ToList()));
        }

        [Test]
        public void ExpressColumn(){
            var db = new DbContext(GetConnection());
            var query = db.Query<User>()
                .Select(x => x.UserId)
                .Select(x => x.Income, "{0} - 100.00::money", x => x.Income);

            Console.WriteLine(query.ToString());
            Console.WriteLine(JsonConvert.SerializeObject(query.ToList()));
        }

        [Test]
        public void ChildQueryColumn(){
            //SELECT "Item1"."user_id" AS "Item1.user_id", "Item1"."income" AS "Item1.income",
            //(SELECT (MAX("a"."role_id")) AS "role_id" FROM "public"."role" AS "a") AS "Item2.Item2"
            //FROM "public"."user" AS "Item1"
            var db = new DbContext(GetConnection());
            var query = db.Query<(User User, int MaxRoleId)>()
                .Select(x => new{
                    x.User.UserId,
                    x.User.Income
                })
                .Select(x => x.MaxRoleId, db.ChildQuery<Role>().Max(x => x.RoleId));

            Console.WriteLine(query.ToString());
            Console.WriteLine(
                JsonConvert.SerializeObject(query.ToList(x => (x.User.UserId,
                    x.User.Income, x.MaxRoleId))));
        }


    }
}