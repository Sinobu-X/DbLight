using System;
using System.Linq;
using DbLight;
using DbLight.Common;

namespace DbLightTest
{
    public class DemoMax
    {
        public static void MaxId(DbConnection cn){
            var db = new DbContext(cn);

            //SELECT (MAX([a].[PostId])) AS [PostId]
            //FROM [EFDemo]..[Posts] AS [a]

            {
                var sql = db.Query<Post>()
                    .Max(x => x.PostId)
                    .ToString();

                Console.WriteLine(sql);
            }


            {
                var maxId = db.Query<Post>()
                    .Max(x => x.PostId)
                    .ToFirst(x => x.PostId);

                Console.WriteLine($"MaxId = {maxId}");
            }
        }
    }
}