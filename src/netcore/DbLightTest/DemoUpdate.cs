using System;
using DbLight;
using DbLight.Common;

namespace DbLightTest
{
    public class DemoUpdate
    {
        public static void Express(DbConnection cn){
            var db = new DbContext(cn);

            //UPDATE [EFDemo]..[Blogs]
            //   SET [Url] = N'abc/d''ef'
            //     , [Active] = 1
            //     , [Rating] = [Rating] + 1
            // WHERE [BlogId] = 5


            {
                var blog = new Blog(){
                    Enable = true,
                    Url = "abc/d'ef",
                };
                
                var sql = db.Update<Blog>(blog)
                    .Select(x => new{
                        x.Enable,
                        x.Url
                    })
                    .Select("{0} = {0} + 1", x => x.Rating)
                    .Where(x => x.BlogId == 5)
                    .ToString();

                Console.WriteLine(sql);
            }
        }
    }
}