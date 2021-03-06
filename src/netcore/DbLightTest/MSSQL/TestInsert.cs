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

namespace DbLightTest.MSSQL
{
    public class TestInsert
    {
        [Test]
        public async Task Normal(){
            var db = new DbContext(QuickStart.BuildConnection());

            var user = new User();
            user.UserId = 2;
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

            var insert = db.Insert(user)
                .Ignore(x => new{
                    x.SexId,
                    x.Remark
                });

            Console.WriteLine(insert.ToString());

            await insert.ExecuteAsync();
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


        [Test]
        public async Task InsertFromTable(){
            var db = new DbContext(QuickStart.BuildConnection());

//            INSERT INTO [User]([UserId], [Height], [UserName])
//            SELECT [a].[UserId] AS [UserId], [a].[UserName] AS [UserName], (N'') AS [UserName]
//            FROM [User] AS [a] WHERE [a].[UserId] > 100

            var insert = db.Insert<User>()
                .Include(x => new{
                    x.UserId,
                    x.Height,
                    x.UserName
                })
                .From(db.Query<User>()
                    .Select(x => new{
                        x.UserId,
                        x.Height,
                    })
                    .SelectValue(x => x.UserName, "")
                    .Where(x => x.UserId > 100));

            Console.WriteLine(insert.ToString());

            var count = await insert.ExecuteAsync();
            Console.WriteLine($"Count = {count}");
        }

        [Test]
        public async Task InsertBatch(){
            var db = new DbContext(QuickStart.BuildConnection());

            var maxUserId = await db.Query<User>()
                .Max(x => x.UserId)
                .ToFirstAsync(x => x.UserId);

            var sw = new Stopwatch();
            sw.Start();
            //-------

            var batchSqls = new List<string>();
            var random = new Random();

            for (var i = 0; i < 10000; i++){
                var user = new User();
                user.UserId = (i + 1) + maxUserId;
                user.UserName = "Name " + user.UserId;
                user.WeChatCode = "WeChat " + user.UserId;
                user.Phone = "130-" + user.UserId;
                user.Birthday = DateTime.Now.AddYears(-10);
                user.Height = 0.07m;
                user.Income = 0.14m;
                user.Married = true;
                user.Remark = "人的";
                user.RegisterTime = DateTime.Now;
                user.SexId = random.Next(1, 3);

                batchSqls.Add(db.Insert(user).ToString());
            }

            //-----
            Console.WriteLine($"Create SQL ms = {sw.ElapsedMilliseconds}");

            await db.ExecNoQueryAsync(batchSqls);
            //-----
            Console.WriteLine($"Execute SQL ms = {sw.ElapsedMilliseconds}");
            sw.Stop();
        }
    }
}