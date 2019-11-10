using System;
using System.Linq;
using DbLight;
using DbLight.Common;
using DbLight.Sql;

namespace DbLightTest
{
    public class DemoJoin
    {
        public static void OutMultipleTable(DbConnection cn){
            var db = new DbContext(cn);

//            SELECT [Item1].[PostId] AS [Item1.PostId]
//                 , [Item1].[Title] AS [Item1.Title]
//                 , [Item1].[BlogId] AS [Item1.BlogId]
//                 , [Item1].[AuthorId] AS [Item1.AuthorId]
//                 , [Item1].[Price] AS [Item1.Price]
//                 , [Item2].[Rating] AS [Item2.Rating]
//                 , [Item2].[Remark] AS [Item2.Remark]
//              FROM [EFDemo]..[Posts] AS [Item1]
//         LEFT JOIN [EFDemo]..[Blogs] AS [Item2] ON [Item2].[BlogId] = [Item1].[BlogId]
//             WHERE [Item1].[PostId] = 9
//


            {
                var sql = db.Query<(Post Post, Blog Blog)>()
                    .Select(x => new{
                        x.Post,
                        x.Blog.Rating,
                        x.Blog.Remark
                    })
                    .LeftJoin(x => x.Blog, x => x.Blog.BlogId == x.Post.BlogId)
                    .WhereBegin()
                    .Compare(x => x.Post.PostId, SqlCompareType.Equal, 9)
                    .WhereEnded()
                    .ToString();

                Console.WriteLine(sql);
            }
        }
    }
}