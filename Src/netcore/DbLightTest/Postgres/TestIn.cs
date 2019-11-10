using System;
using System.Linq;
using DbLight;
using DbLight.Common;
using DbLight.Sql;
using NUnit.Framework;

namespace DbLightTest.Postgres
{
    public class TestIn: TestBase
    {
        [SetUp]
        public void Setup(){

        }

        [Test]
        public void InArray(){
            var db = new DbContext(GetConnection());

            //SELECT *
            //FROM [EFDemo]..[Posts] AS [a]
            //WHERE [a].[PostId] IN (1, 2, 3)
            //AND [a].[Title] IN (N'a', N'b', N'c')

            var userIds = new[]{1, 2, 3};
            var userNames = new[]{"a", "b", "c"};

            {
                var sql = db.Query<User>()
                    .Where(x => userIds.Contains(x.UserId) && userNames.Contains(x.UserName))
                    .ToString();

                Console.WriteLine(sql);
            }

            {
                var sql = db.Query<User>()
                    .WhereBegin()
                    .In(x => x.UserId, userIds)
                    .In(x => x.UserName, userNames)
                    .WhereEnded()
                    .ToString();

                Console.WriteLine(sql);
            }
        }

        public static void InQuery(DbConnection cn){
            var db = new DbContext(cn);

            //SELECT *
            // FROM [EFDemo]..[Posts] AS [a]
            //WHERE [a].[PostId] > 3
            //  AND [a].[BlogId] IN(
            //      SELECT [a].[BlogId] AS [BlogId]
            //       FROM [EFDemo]..[Blogs] AS [a]
            //      WHERE [a].[Rating] > 0
            // )

            {
                var sql = db.Query<Post>()
                    .Where(x => x.PostId > 3 &&
                                db.ChildQuery<Blog>()
                                    .Select(y => y.BlogId)
                                    .Where(y => y.Rating > 0).Contains(x.BlogId))
                    .ToString();

                Console.WriteLine(sql);
            }

            {
                var sql = db.Query<Post>()
                    .WhereBegin()
                    .Compare(x => x.PostId, SqlCompareType.Greater, 3)
                    .Add(x => db.ChildQuery<Blog>()
                        .Select(y => y.BlogId)
                        .WhereBegin(SqlWhereJoinType.And)
                        .Compare(y => y.Rating, SqlCompareType.Greater, 0)
                        .WhereEnded()
                        .Contains(x.BlogId))
                    .WhereEnded()
                    .ToString();

                Console.WriteLine(sql);
            }
        }
    }
}