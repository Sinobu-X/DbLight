using System;
using System.Collections.Generic;
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
    public class TestInsert
    {
        [Test]
        public async Task Insert(){
            var db = new DbContext(QuickStart.BuildConnection());

            var user = new User();
            user.UserId = 13;
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
        public async Task InsertBatch(){
            var db = new DbContext(QuickStart.BuildConnection());

            
            
            var sqls = new List<string>();

            {
                sqls.Add(db.Delete<User>()
                    .WhereBegin()
                    .Compare(x=> x.UserId, SqlCompareType.GreaterOrEqual, 100)
                    .WhereEnded()
                    .ToString());
            }
            
            for (var i = 0; i < 100; i++) {
                var user = new User();
                user.UserId = 100 + i;
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
                
                sqls.Add(db.Insert(user).ToString());
            }

            await db.ExecNoQueryAsync(sqls);
        }

        [Test]
        public async Task InsertFile(){
            var db = new DbContext(QuickStart.BuildConnection());

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