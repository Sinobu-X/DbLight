using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DbLight;
using DbLight.Common;
using DbLight.Sql;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DbLightTest.MSSQL
{
    public class TestUpdate
    {
        [Test]
        public async Task ByInt1(){
            var db = new DbContext(QuickStart.BuildConnection());

            var user = new User();
            user.UserId = 12;
            user.UserName = "Name '" + user.UserId;
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

            var upd = db.Update(user)
                .Exclude(x => x.Photo)
                .Where(x => x.UserId == 12);

            Console.WriteLine(upd.ToString());

            var count = await upd.ExecuteAsync();
            Console.WriteLine($"Count = {count}");
        }
        
        [Test]
        public async Task ByInt2(){
            var db = new DbContext(QuickStart.BuildConnection());

            var user = new User();
            user.UserId = 12;
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

            var del = db.Update(user)
                .WhereBegin()
                .Compare(x => x.UserId, SqlCompareType.Equal, 12)
                .WhereEnded();

            Console.WriteLine(del.ToString());

            var count = await del.ExecuteAsync();
            Console.WriteLine($"Count = {count}");
        }

        [Test]
        public async Task IgnoreColumn(){
            var db = new DbContext(QuickStart.BuildConnection());

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

            var del = db.Update(user)
                .Exclude(x => new{
                    x.UserId,
                    x.Photo
                })
                .Where(x => x.UserId == 12);

            Console.WriteLine(del.ToString());

            var count = await del.ExecuteAsync();
            Console.WriteLine($"Count = {count}");
        }

        [Test]
        public async Task UpdateExpress(){
            var db = new DbContext(QuickStart.BuildConnection());

            var user = new User();
            user.UserId = 12;
            user.UserName = "Name " + user.UserId;
            user.SexId = 2;

            var del = db.Update(user)
                .Include(x => new {
                    x.UserName,
                    x.Height,
                    x.SexId
                })
                .Include("{0} = {0} + 100.00", x => x.Income)
                .Where(x => x.UserId == 12);

            Console.WriteLine(del.ToString());

            var count = await del.ExecuteAsync();
            Console.WriteLine($"Count = {count}");
        }
    }
}