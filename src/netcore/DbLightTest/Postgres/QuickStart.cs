using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DbLight;
using DbLight.Common;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DbLightTest.Postgres
{
    public class QuickStart
    {
        public static DbConnection BuildConnection(){
            //Host=127.0.0.1;Username=cqa_sa;Password=cqa_sa;Database=cqa;Connect Timeout=900
            return new DbConnection(DbDatabaseType.Postgres,
                "Host=127.0.0.1;Username=test;Password=test;Database=dblight");
        }

        [Test]
        public void TestConnection() {
            var cn = new DbConnection(DbDatabaseType.Postgres) {
                Postgres = new DbConnection.PostgresInfo() {
                    Host = "127.0.0.1",
                    User = "test",
                    Password = "test",
                    Database = "dblight"
                },
            };

            var db = new DbContext(cn);
            var users = db.Query<User>()
                .Where(x => x.UserId >= 1 && x.UserId < 10)
                .ToList();

            Console.WriteLine(JsonConvert.SerializeObject(users));
        }
        
        [Test]
        public void Query(){
            var db = new DbContext(BuildConnection());
            var users = db.Query<User>()
                .Where(x => x.UserId >= 1 && x.UserId < 10)
                .ToList();

            Console.WriteLine(JsonConvert.SerializeObject(users));
        }

        [Test]
        public async Task QueryAsync(){
            var db = new DbContext(BuildConnection());
            var users = await db.Query<User>()
                .Where(x => x.UserId >= 1 && x.UserId < 10)
                .ToListAsync();

            Console.WriteLine(JsonConvert.SerializeObject(users));
        }

        [Test]
        public async Task QueryMultiTableAsync(){
            var db = new DbContext(BuildConnection());

            var users = await db.Query<(User User, Sex Sex)>()
                .Select(x => new{
                    x.User,
                    x.Sex.SexName
                })
                .LeftJoin(x => x.Sex, x => x.Sex.SexId == x.User.SexId)
                .Where(x => x.User.UserId >= 1 && x.User.UserId < 10)
                .ToListAsync();

            Console.WriteLine(JsonConvert.SerializeObject(users));
        }

        [Test]
        public async Task InsertAsync(){
            var db = new DbContext(BuildConnection());

            var user = new User();
            user.UserId = await db.Query<User>().Max(x => x.UserId).ToFirstAsync(x => x.UserId) + 1;
            user.UserName = "Name " + user.UserId;
            user.WeChatCode = "WeChat " + user.UserId;
            user.Phone = "130-" + user.UserId;
            user.Birthday = DateTime.Now.AddYears(-10);
            user.Height = 0.07m;
            user.Income = 0.14m;
            user.Married = true;
            user.Remark = "中文";
            user.SexId = 2;
//            user.Photo = File.ReadAllBytes(@"your/path/image.png");
            user.RegisterTime = DateTime.Now;


            await db.Insert(user).ExecuteAsync();
        }

        [Test]
        public async Task UpdateAsync(){
            var db = new DbContext(BuildConnection());

            var user = new User();
            user.UserId = 1;
            user.UserName = "Name '" + user.UserId;
            user.WeChatCode = "WeChat " + user.UserId;
            user.Phone = "130-" + user.UserId;
            user.Birthday = DateTime.Now.AddYears(-20);
            user.Height = 1.07m;
            user.Income = 1.14m;
            user.Married = false;
            user.Remark = "中文";
            user.SexId = 2;
//            user.Photo = File.ReadAllBytes(@"your/path/image.png");
            user.RegisterTime = DateTime.Now;

            var updCount = await db.Update(user)
                .Where(x => x.UserId == user.UserId)
                .ExecuteAsync();

            Console.WriteLine($"Affect Count = {updCount}");
        }

        [Test]
        public async Task UpdatePartAsync(){
            var db = new DbContext(BuildConnection());

            var user = new User();
            user.UserId = 1;
            user.UserName = "Name '" + user.UserId;
            user.SexId = 1;

            var updCount = await db.Update(user)
                .Select(x => new{
                    x.UserName,
                    x.SexId
                })
                .Where(x => x.UserId == user.UserId)
                .ExecuteAsync();

            Console.WriteLine($"Affect Count = {updCount}");
        }

        [Test]
        public async Task DeleteAsync(){
            var db = new DbContext(BuildConnection());

            var delCount = await db.Delete<User>()
                .Where(x => x.UserId == 2)
                .ExecuteAsync();

            Console.WriteLine($"Affect Count = {delCount}");
        }

        [Test]
        public async Task Transaction(){
            using (var db = new DbContext(BuildConnection())){
                await db.BeginTransactionAsync();

                //del
                await db.Delete<Sex>()
                    .Where(x => x.SexId == 1)
                    .ExecuteAsync();

                //insert
                await db.Insert(new Sex(){
                    SexId = 1,
                    SexName = "Man"
                }).ExecuteAsync();

                //del
                await db.Delete<Sex>()
                    .Where(x => x.SexId == 2)
                    .ExecuteAsync();

                //insert
                await db.Insert(new Sex(){
                    SexId = 2,
                    SexName = "Woman"
                }).ExecuteAsync();

                db.Commit();
            }
        }

        [Test]
        public async Task Batch(){
            var db = new DbContext(BuildConnection());

            var batchSqls = new List<string>();

            batchSqls.Add(db.Delete<Sex>()
                .Where(x => x.SexId == 1)
                .ToString());

            batchSqls.Add(db.Insert(new Sex(){
                SexId = 1,
                SexName = "Man"
            }).ToString());

            batchSqls.Add(db.Delete<Sex>()
                .Where(x => x.SexId == 2)
                .ToString());

            batchSqls.Add(db.Insert(new Sex(){
                SexId = 2,
                SexName = "Woman"
            }).ToString());

            await db.ExecNoQueryAsync(batchSqls);
        }


    }
}