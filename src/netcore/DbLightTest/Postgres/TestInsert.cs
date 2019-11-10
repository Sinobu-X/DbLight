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
    public class TestInsert : TestBase
    {
        [Test]
        public async Task Normal(){
            var db = new DbContext(GetConnection());

            var user = new User();
            user.UserId = 12;
            user.UserName = "Name " + user.UserId;
            user.WeChatCode = "WeChat " + user.UserId;
            user.Phone = "130-" + user.UserId;
            user.Birthday = DateTime.Now.AddYears(-10);
            user.Height = 0.07m;
            user.Income = 0.14m;
            user.Married = true;
            user.Remark = "人的";
            user.RegisterTime = DateTime.Now;
            user.SexId = 2;

            Console.WriteLine(db.Insert(user).ToString());

            await db.Insert(user).ExecuteAsync();
        }

        [Test]
        public async Task InsertFile(){
            var db = new DbContext(GetConnection());

            var user = new User();
            user.UserId = 13;
            user.UserName = "Name " + user.UserId;
            user.WeChatCode = "WeChat " + user.UserId;
            user.Phone = "130-" + user.UserId;
            user.Birthday = DateTime.Now.AddYears(-10);
            user.Height = 0.07m;
            user.Income = 0.14m;
            user.Married = true;
            user.Photo = System.IO.File.ReadAllBytes("/Users/sinobu/Downloads/IMG_20191106_195230.jpg");
            user.Remark = "人的";
            user.RegisterTime = DateTime.Now;
            user.SexId = 2;

            Console.WriteLine(db.Insert(user).ToString());

            await db.Insert(user).ExecuteAsync();
        }
    }
}