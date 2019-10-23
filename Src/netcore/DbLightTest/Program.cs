using System;
using System.Threading.Tasks;
using DbLight;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using DbLight.Sql;
using DbLight.Common;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using log4net.Config;

namespace DbLightTest
{
    class Program
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args){
            _log.Info("App Running...");

            var connection = new DbConnection(DbDatabaseType.SqlServer,
                "server=127.0.0.1;uid=test;pwd=test;database=EFDemo;Connect Timeout=900");
            connection.Groups.Add(("Demo", "EFDemo"));

            DemoJoin.OutMultipleTable(connection);
//            DemoUpdate.Express(connection);
//            DemoIn.InArray(connection);
//            DemoIn.InQuery(connection);
//            DemoMax.MaxId(connection);


//            var need = true;
//
//            while (true){
//                Task.Run(async () => {
//                    try{
//                        await Test_MaxId(connection);
//                    }
//                    catch (Exception e){
//                        Console.WriteLine(e);
//                        throw;
//                    }
//                });
//
//
//                //TestSyncSpeed(dbcn);
////                if (need) {
////                   for(var i = 0; i < 100; i++) {
////                        TestSqlSync(dbcn);
////                    }
////                }
//
////                var w = new Stopwatch();
////                w.Start();
////                Test_SqlQuery_LeftJoin_Sync(dbcn);
////                w.Stop();
////                Console.WriteLine("T = " + w.ElapsedMilliseconds);
//
//                //Test_QueryForSingleTable(connection).Wait();
//
//                //TestQueryBySql(dbcn).Wait();
//                //TestReflection();
//
//                //TestQueryLeftJoin(dbcn).Wait();
//                //TestObjectQueryBySql(dbcn).Wait();
//
//                //TestExpressionHelper();
//
//                //Test_WhereEqual(connection).Wait();
//
//                var command = Console.ReadLine().ToUpper();
//                if (command == "R"){
//                    need = true;
//                    continue;
//                }
//                else if (command == "CLEAR"){
//                    Console.Clear();
//                }
//                else if (command == "GC"){
//                    GC.Collect();
//                }
//                else{
//                    break;
//                }
//            }

            //Console.Read();
        }

        class Abc
        {
            public int MaxId{ get; set; }
        }

        static async Task Test_MaxId(DbConnection connection){
            using (var db = new DbContext(connection)){
                var sql = db.Query<Abc>()
                    .Select(x => x.MaxId, "MAX(TxnId)")
                    .From(x => x, "ddd")
                    .WhereBegin()
                    .Add("ABC = 4")
                    .WhereEnded()
                    .ToString();

//                var sql = db.Query<(Post Post, Blog Blog)>()
//                    .LeftJoin(x => x.Blog, x => x.Post.BlogId == x.Blog.BlogId)
//                    .LeftJoin(x => x.Blog, x => x.Post.BlogId == x.Blog.BlogId)
//                    .ToString();
//

                Console.WriteLine(sql);
            }
        }

        static async Task Test_WhereEqual(DbConnection connection){
            var db = new DbContext(connection);

            var times = 100000;

            var item = new Post();
            item.PostId = 24;
            item.BlogId = 12;

            var sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            for (var i = 0; i < times; i++){
                var s = db.Query<Post>()
                    .Select(x => new{
                        x.PostId,
                        x.BlogId
                    })
                    .WhereBegin()
                    .Compare(x => x.PostId, SqlCompareType.GreaterOrEqual, item.PostId)
                    .Compare(x => x.BlogId, SqlCompareType.Equal, item.BlogId)
                    .Compare(x => x.PostId, SqlCompareType.GreaterOrEqual, item.PostId)
                    .Compare(x => x.BlogId, SqlCompareType.Equal, item.BlogId)
                    .Compare(x => x.PostId, SqlCompareType.GreaterOrEqual, item.PostId)
                    .Compare(x => x.BlogId, SqlCompareType.Equal, item.BlogId)
                    .WhereEnded()
                    .ToString();
            }

            sw.Stop();
            Console.WriteLine($"Time = {sw.ElapsedMilliseconds}");

            sw.Reset();
            sw.Start();
            for (var i = 0; i < times; i++){
                var s = db.Update(item)
                    .Select(x => new{
                        x.PostId,
                        x.Title
                    })
                    .WhereBegin()
                    .Compare(x => x.PostId, SqlCompareType.GreaterOrEqual, item.PostId)
                    .Compare(x => x.BlogId, SqlCompareType.Equal, item.BlogId)
                    .Compare(x => x.PostId, SqlCompareType.GreaterOrEqual, item.PostId)
                    .Compare(x => x.BlogId, SqlCompareType.Equal, item.BlogId)
                    .Compare(x => x.PostId, SqlCompareType.GreaterOrEqual, item.PostId)
                    .Compare(x => x.BlogId, SqlCompareType.Equal, item.BlogId)
                    .WhereEnded()
                    .ToString();
            }

            sw.Stop();
            Console.WriteLine($"Time = {sw.ElapsedMilliseconds}");

            sw.Reset();
            sw.Start();
            for (var i = 0; i < times; i++){
                var s = db.Delete<Post>()
                    .WhereBegin()
                    .Compare(x => x.PostId, SqlCompareType.GreaterOrEqual, item.PostId)
                    .Compare(x => x.BlogId, SqlCompareType.Equal, item.BlogId)
                    .Compare(x => x.PostId, SqlCompareType.GreaterOrEqual, item.PostId)
                    .Compare(x => x.BlogId, SqlCompareType.Equal, item.BlogId)
                    .Compare(x => x.PostId, SqlCompareType.GreaterOrEqual, item.PostId)
                    .Compare(x => x.BlogId, SqlCompareType.Equal, item.BlogId)
                    .WhereEnded()
                    .ToString();
            }

            sw.Stop();
            Console.WriteLine($"Time = {sw.ElapsedMilliseconds}");


//            sw.Reset();
//            sw.Start();
//            for (var i = 0; i < times; i++){
//                var s = db.Query<Post>()
////                    .Select(x => new{
////                        x.PostId,
////                        x.BlogId
////                    })
//                    .Where(x => x.PostId >= item.PostId && x.BlogId == item.BlogId &&
//                                x.PostId >= item.PostId && x.BlogId == item.BlogId &&
//                                x.PostId >= item.PostId && x.BlogId == item.BlogId)
//                    .ToString();
//            }
//
//            sw.Stop();
//            Console.WriteLine($"Time = {sw.ElapsedMilliseconds}");


            sw.Reset();
            sw.Start();
            for (var i = 0; i < times; i++){
                var BLOG_ID = "BlogId";
                var POST_ID = "PostId";
                var POST = "[].[Post]";

                var s =
                    $"SELECT {BLOG_ID},{POST_ID} {POST} WHERE {POST_ID} >= {item.PostId} AND {BLOG_ID} = {item.BlogId}";
            }

            sw.Stop();
            Console.WriteLine($"Time = {sw.ElapsedMilliseconds}");
        }

        static async Task Test_QueryForSingleTable(DbConnection connection){
            using (var db = new DbContext(connection)){
                var sql = db.Query<(Post Post, Blog Blog)>()
                    .LeftJoin(x => x.Blog, x => x.Post.BlogId == x.Blog.BlogId)
                    .LeftJoin(x => x.Blog, x => x.Post.BlogId == x.Blog.BlogId)
                    .ToString();


                Console.WriteLine(sql);
            }
        }

        static async Task Test_MethodInWhere(DbConnection connection){
            using (var db = new DbContext(connection)){
                string sql = db.Query<Post>()
                    //.Where(x => x.BlogId == db.ChildQuery<Blog>().Max(y => y.BlogId).To<int>())
                    //.Where(x => x.BlogId == db.Exp("SELECT MAX(xx)").To<int>())
                    //.Where(x => db.ChildQuery("SELECT XXX").In(x.BlogId))
                    .Where(x => db.Exp("SELECT MAX(xx)").Contains(x.BlogId) &&
                                x.Title.Contains("xx") &&
                                x.Title.StartsWith("s") &&
                                x.Title.EndsWith("f") &&
                                new string[]{"1", "2"}.Contains(x.Title) &&
                                x.BlogId != db.Exp("BIG").To<int>())
                    .WhereBegin()
                    .Add("{0} > {1}", x => new{
                        x.BlogId, x.Price
                    })
                    .WhereEnded()
                    .ToString();
                Console.WriteLine(sql);
            }
        }

        static async Task Test_SqlUpdate(DbConnection connection){
            using (var db = new DbContext(connection)){
                var w = new Stopwatch();
                w.Start();

                var id = 200;
                var item = new PostView(){
                    PostId = 1001,
                    AuthorName = "xx",
                    Title = "Test中在",
                    Price = 23.45m,
                    AuthorId = 2,
                    BlogId = 1
                };

                var sql = db.Update(item)
                    .Select(x => new{
                        x.Title,
                        x.Price
                    })
                    .WhereBegin()
                    .Add(x => x.AuthorName.CompareTo(item.AuthorName) >= 0)
                    .Add(x => x.AuthorName.CompareTo(item.AuthorName) <= 0)
                    .Add(x => x.AuthorName.CompareTo(item.AuthorName) == 0)
                    .Add(x => x.Price.CompareTo(item.Price) >= 0)
                    .Add(x => x.Price.CompareTo(item.Price) <= 0)
                    .Add(x => x.Price.CompareTo(item.Price) == 0)
                    .Add(x => x.AuthorName == null)
                    .WhereEnded()
                    .ToString();


                w.Stop();
                Console.WriteLine("T:" + w.ElapsedMilliseconds);
                Console.WriteLine(sql);

//                Console.WriteLine(JsonConvert.SerializeObject(new{
//                    Count = count
//                }, Formatting.Indented));
            }
        }

        static async Task Test_SqlUpdateBatch(DbConnection connection){
            using (var db = new DbContext(connection)){
                var items = await db.Query<Post>().ToListAsync();

                var w = new Stopwatch();
                w.Start();

                var batchSql = new List<string>();
                var updater = db.Update<Post>(null).Select(y => new{
                    y.Price,
                    y.Title
                });
                items.ForEach(x => { batchSql.Add(updater.SetData(x).Where(y => y.PostId == x.PostId).ToString()); });

                w.Stop();
                Console.WriteLine("T:" + w.ElapsedMilliseconds);

                //b
                //Console.WriteLine(JsonConvert.SerializeObject(batchSql, Formatting.Indented));
            }
        }

        static async Task Test_SqlInsert(DbConnection connection){
            using (var db = new DbContext(connection)){
                var w = new Stopwatch();
                w.Start();

                var item = new PostView(){
                    PostId = 1001,
                    AuthorName = "xx",
                    Title = "Test中在",
                    Price = 23.45m,
                    AuthorId = 2,
                    BlogId = 1
                };
                await db.Insert<Post>(item).ExecuteAsync();

                w.Stop();
                Console.WriteLine("T:" + w.ElapsedMilliseconds);

                Console.WriteLine(JsonConvert.SerializeObject(new{
                    Count = 0
                }, Formatting.Indented));
            }
        }

        static async Task Test_SqlDelete(DbConnection connection){
            using (var db = new DbContext(connection)){
                var w = new Stopwatch();
                w.Start();

                var d = new{
                    ids = new string[]{"3", "3"}
                };


                var name = "xxxx";

                var count = await db.Delete<Blog>()
                    //.Where(x => x.AuthorId == null && x.AuthorName == null && x.AuthorName.Contains("x") )
//                    .Where(x => x.Enable == null || 
//                                x.OpenTime == null || 
//                                d.ids.Contains(x.Remark) || 
//                                x.Remark.StartsWith(name) || 
//                                x.Remark.EndsWith(name) || 
//                                x.Remark.Contains(name))
                    //.Where(x => x.Remark.Contains("ff"))
//                    .Where(x => db.ChildQuery<Post>()
//                                    .Max(y => y.BlogId)
//                                    .Where(y => y.BlogId != 30)
//                                    .In(x.BlogId)
//                                && new int[]{1, 2, 3}.Contains(x.BlogId)
//                                && db.ChildQuery("SELECT BlogId FROM Blogs").In(x.BlogId)
//                                && x.Enable == true)
//                    .Where(x => x.BlogId >= db.ChildQuery<Post>().Max(y => y.BlogId).ToValue<int>())
                    .Where(x => x.BlogId >= db.ChildQuery("SELECT BlogId FROM Blogs").To<int>())
                    .Where(x => x.BlogId >= db.Exp("SELECT BlogId FROM Blogs").To<int>())
                    .Where(x => db.Exp("SELECT BlogId FROM Blogs").Contains(x.BlogId))
//                    .WhereBegin(SqlWhereJoinType.Or)
//                    .Add(x => x.BlogId > 4)
//                    .Add(x => x.Enable == true)
//                    .WhereStart("1")
//                    .Add(x => x.Rating != 5)
//                    .Add(x => x.Remark != "d")
//                    .WhereEnded("1")
//                    .WhereStart("2")
//                    .Add(x => x.Rating != 4)
//                    .Add(x => x.Remark != "x")
//                    .WhereEnded("2")
//                    .WhereEnded()
                    .ExecuteAsync();

                w.Stop();
                Console.WriteLine("T:" + w.ElapsedMilliseconds);

                Console.WriteLine(JsonConvert.SerializeObject(new{
                    Result = count
                }, Formatting.Indented));
            }
        }

        static void Test_SqlQuery_LeftJoin_Sync(DbConnection dbcn){
            var blogId = 5;
            var minPrice = 1m;


            using (var db = new DbContext(dbcn)){
                var w = new Stopwatch();
                w.Start();
                var query = db.Query<(Post Post, int Count)>();
                query.Reset()
                    .Select(x => x.Post.BlogId)
                    .Sum(x => x.Post.Price)
                    .Count(x => x.Count)
                    .Where(x => x.Post.BlogId != 9)
                    .GroupBy(x => x.Post.BlogId)
                    .Having(x => x.Post.BlogId > 0)
                    .OrderBy(x => x.Post.Price);
                for (int i = 0; i < 1000; i++){
                    var items = query.ToString();
                }


                w.Stop();
                Console.WriteLine("T:" + w.ElapsedTicks);

                //Console.WriteLine(JsonConvert.SerializeObject(items, Formatting.Indented));
            }
        }


        async static Task Test_SqlQuery_LeftJoin(DbConnection dbcn){
            var blogId = 120;
            var minPrice = 200m;
            using (var db = new DbContext(dbcn)){
                var items = await db.Query<Post>()
                    .Where(x => x.BlogId > blogId && x.Price >= minPrice)
                    .ToListAsync();

                Console.WriteLine(JsonConvert.SerializeObject(items, Formatting.Indented));
            }
        }

        async static Task Test_SqlQuery_SelectWithIgnore(DbConnection dbcn){
            using (var db = new DbContext(dbcn)){
                var sql = db.Query<(Post Post, Blog Blog)>()
                    //.Max(x => x.BlogId)
                    //.SelectWithIgnore(x => x, x => x.Title)
                    //.SelectWithIgnore(x => x, x => x.Rating)
                    //.Select(x => x.Rating, x => "{0} + 1000", x=> x.Rating)
                    //.From(x => db.Query<Post>().Max(y => y.BlogId).Min(y => y.Title))
                    //.WithNoLock()
                    .Select(x => new{
                        x.Post,
                        x.Blog
                    })
                    .LeftJoin(x => x.Blog, x => x.Blog.BlogId == x.Post.BlogId)
                    //.Where(x => x.PostId > 1 && (x.Title == "xx" || x.Title == "ff") && x.AuthorId == 9)
                    //.Where(x => x.Url == subQuery)
                    //.Where(x => x.Url == db.Query<Post>().SelectWithIgnore(y => y, y=> y.BlogId).ToSql(true))
                    //.Where(x => x.BlogId >= 4 || x.Url == "Abc" || x.Rating != 23)
                    .ToString();
                Console.WriteLine(sql);
                var items = await db.ExecQueryToListAsync<Post>(sql);


//                var items = await db.Query<(Post Post, (int MaxRating, int Count) Extra)>()
//                    .Select(x => new{
//                        //x.Post.PostId,
//                        x.Post.Title,
//                        x.Post.AuthorId
//                    })
//                    .Select(x => x.Extra.MaxRating, x => db.ChildQuery<Blog>().Max(y => y.Rating))
//                    .Select(x => x.Extra.Count, x => db.ChildQuery<Blog>().Count(y => y.BlogId))
//                    .Select(x => x.Post.PostId, x => "CASE WHEN {0} > 4 THEN {0} + 1 ELSE {1} END", x=> new{
//                        x.Post.BlogId,
//                        x.Post.AuthorId
//                    })
//                    .ToListAsync();

                Console.WriteLine(JsonConvert.SerializeObject(items, Formatting.Indented));
            }
        }

        async static Task TestQueryLeftJoin(DbConnection dbcn){
            using (var db = new DbContext(dbcn)){
//                var items = await db.Query<(Post Post, Blog Blog, Author Author, int MaxRating)>()
//                    .Select(x => new{
//                        x.Post.PostId,
//                        x.Post.Title,
//                        x.Post.AuthorId
//                    }).ToListAsync<Post>(x => x.Post);

                var sql = db.Query<(Blog, Post)>()
                    .SelectWithIgnore(x => x.Item1, x => new{
                        x.Item1.BlogId,
                        x.Item1.Url
                    })
                    .WithNoLock()
                    .ToString();

                Console.WriteLine(sql);

                //Console.WriteLine(JsonConvert.SerializeObject(items, Formatting.Indented));
            }
        }

        static void TestReflection(){
            //{
            //    var info = DbLight.MappingUt.GetTypeInfo(typeof((Blog Blog, Post Post, int MaxCount)));
            //    Console.WriteLine(JsonConvert.SerializeObject(info, Formatting.Indented));
            //}
            //{
            //    var info = DbLight.MappingUt.GetTypeInfo(typeof(Blog));
            //    Console.WriteLine(JsonConvert.SerializeObject(info, Formatting.Indented));
            //}
            //TestReflectionSub<(Blog Blog, Post Post, int MaxCount)>();
            //var item = DbUt.TestReflectionSub<(Blog Blog, Post Post, int MaxCount)>();
            //(Blog Blog, Post Post, int MaxCount) a = (null,null,9);
            //a.MaxCount = 10;
        }

        static void TestReflectionSub<T>() where T : new(){
            var t = typeof(T);

            var item = Activator.CreateInstance(t);
            var firstField = item.GetType().GetFields()[0];
            firstField.SetValue(item, Activator.CreateInstance(firstField.FieldType));

            //Console.WriteLine();
            //Console.WriteLine(t.ToString());
        }


        async static Task TestQueryTrupleMerge(DbConnection dbcn){
            // (Blogs Blog, Posts Post) item = (new Blogs(), new Posts());

            // await TestTruple<(Blog Blog, Post Post)>((new Blog(), new Post()));

            //using (var db = new DbContext(dbcn)) {

            //    var items = await db.Query<(Blogs Blogs, Posts Post)>(@"Select * from authors").ToListAsync();

            //    Console.WriteLine(JsonConvert.SerializeObject(items, Formatting.Indented));
            //}
        }

        //async static Task TestTruple<T>(T item)
        //{
        //    if(typeof(T) is System.ValueTuple) {

        //    }
        //    foreach (var p in typeof(T).GetFields()) {
        //        Console.WriteLine(JsonConvert.SerializeObject(p, Formatting.Indented));
        //    }
        //}

        async static Task TestObjectSqlAsync(DbConnection dbcn){
            using (var db = new DbContext(dbcn)){
                var sql = "SELECT" +
                          "  a.*" +
                          " ,b.Url AS [b.Url]" +
                          " ,b.Rating AS [b.Rating]" +
                          " ,b.Active AS [b.Active]" +
                          " ,c.AuthorName AS [c.AuthorName]" +
                          " ,(SELECT MAX(Price) FROM Posts) AS [d.MaxPrice]" +
                          " FROM Posts AS a" +
                          " LEFT JOIN Blogs AS b ON b.BlogId = a.BlogId" +
                          " LEFT JOIN Authors AS c ON c.AuthorId = a.AuthorId";

                //{
                //    Stopwatch sw = new Stopwatch();
                //    sw.Start();
                //    var dt = await db.ExecQueryToDataTableAsync(sql);
                //    sw.Stop();
                //    Console.WriteLine("DataTable = " + sw.ElapsedMilliseconds);
                //}
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var items = await db.Query<(Post Post, Blog Blog, Author Author, decimal MaxPrice)>(sql)
                        .ToListAsync<Post>(x => {
                            x.Post.PostBlog = x.Blog;
                            x.Post.PostAuthor = x.Author;
                            return x.Post;
                        });
                    //var items = await db.Query<(Post Post, Blog Blog, Author Author, decimal MaxPrice)>(sql).ToListAsync();
                    sw.Stop();
                    Console.WriteLine("List = " + sw.ElapsedMilliseconds);
                    //Console.WriteLine(JsonConvert.SerializeObject(items, Formatting.Indented));
                }
            }
        }

        async static Task TestTupleSqlAsync(DbConnection dbcn){
            using (var db = new DbContext(dbcn)){
                var sql = "SELECT" +
                          "  a.*" +
                          " ,b.Url AS [b.Url]" +
                          " ,b.Rating AS [b.Rating]" +
                          " ,b.Active AS [b.Active]" +
                          " ,c.AuthorName AS [c.AuthorName]" +
                          " ,(SELECT MAX(Price) FROM Posts) AS [d.MaxPrice]" +
                          " FROM Posts AS a" +
                          " LEFT JOIN Blogs AS b ON b.BlogId = a.BlogId" +
                          " LEFT JOIN Authors AS c ON c.AuthorId = a.AuthorId";

                //{
                //    Stopwatch sw = new Stopwatch();
                //    sw.Start();
                //    var dt = await db.ExecQueryToDataTableAsync(sql);
                //    sw.Stop();
                //    Console.WriteLine("DataTable = " + sw.ElapsedMilliseconds);
                //}
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var items = await db.Query<(Post Post, Blog Blog, Author Author, decimal MaxPrice)>(sql)
                        .ToListAsync<Post>(x => {
                            x.Post.PostBlog = x.Blog;
                            x.Post.PostAuthor = x.Author;
                            return x.Post;
                        });
                    //var items = await db.Query<(Post Post, Blog Blog, Author Author, decimal MaxPrice)>(sql).ToListAsync();
                    sw.Stop();
                    Console.WriteLine("List = " + sw.ElapsedMilliseconds);
                    //Console.WriteLine(JsonConvert.SerializeObject(items, Formatting.Indented));
                }
            }
        }

        static void TestSqlSync(DbConnection dbcn){
            using (var db = new DbContext(dbcn)){
                var sql = "SELECT" +
                          "  a.*" +
                          " ,b.Url AS [b.Url]" +
                          " ,b.Rating AS [b.Rating]" +
                          " ,b.Active AS [b.Active]" +
                          " ,c.AuthorName AS [c.AuthorName]" +
                          " ,(SELECT MAX(Price) FROM Posts) AS [d.MaxPrice]" +
                          " FROM Posts AS a" +
                          " LEFT JOIN Blogs AS b ON b.BlogId = a.BlogId" +
                          " LEFT JOIN Authors AS c ON c.AuthorId = a.AuthorId";

                //{
                //    Stopwatch sw = new Stopwatch();
                //    sw.Start();
                //    var dt = await db.ExecQueryToDataTableAsync(sql);
                //    sw.Stop();
                //    Console.WriteLine("DataTable = " + sw.ElapsedMilliseconds);
                //}
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var items = db.Query<(Post Post, Blog Blog, Author Author, decimal MaxPrice)>(sql).ToList<Post>(
                        x => {
                            x.Post.PostBlog = x.Blog;
                            x.Post.PostAuthor = x.Author;
                            return x.Post;
                        });
                    //var items = await db.Query<(Post Post, Blog Blog, Author Author, decimal MaxPrice)>(sql).ToListAsync();
                    sw.Stop();
                    Console.WriteLine("List = " + sw.ElapsedMilliseconds);
                    //Console.WriteLine(JsonConvert.SerializeObject(items, Formatting.Indented));
                }
            }
        }


        async static Task TestObjectQueryBySql(DbConnection dbcn){
            using (var db = new DbContext(dbcn)){
                var sql = @"SELECT
   Post.*
  ,Blog.Url AS [Blog.Url]
  ,Blog.Rating AS [Blog.Rating]
  ,Author.AuthorName AS [Author.AuthorName]
FROM Posts AS Post
LEFT JOIN Blogs AS Blog ON Blog.BlogId = Post.BlogId 
LEFT JOIN Authors AS Author ON Author.AuthorId = Post.AuthorId";

                var items = await db
                    .Query<Post>(sql).ToListAsync();

                Console.WriteLine(JsonConvert.SerializeObject(items, Formatting.Indented));
            }
        }

        async static Task TestTupleQueryBySql(DbConnection dbcn){
            using (var db = new DbContext(dbcn)){
                var sql = @"SELECT 
                 a.*
                ,(SELECT MAX(a.Rating) FROM Blogs AS a) AS [b.Item]
                ,b.Total AS [c.Item1]
                ,b.MaxPrice AS [c.Item2]
            FROM Blogs AS a
            LEFT JOIN (SELECT 
                 a.BlogId AS BlogId
                ,SUM(a.Price) AS Total
                ,MAX(a.Price) AS MaxPrice 
            FROM Posts AS a GROUP BY a.BlogId) 
            AS b ON b.BlogId = a.BlogId";

                var items = await db
                    .Query<(Blog Blog, int MaxRating, (decimal Total, decimal MaxPrice) Summary)>(sql).ToListAsync();

                Console.WriteLine(JsonConvert.SerializeObject(items, Formatting.Indented));
            }
        }

        async static Task TestDataTableMapping(DbConnection dbcn){
            //{
            //    var mapping = new DataTableMapping<Blog>(null);
            //    mapping.AddRow(null);
            //    mapping.ToList();
            //}

            //{
            //    var mapping = new DataTableMapping<(Blog Blog, Post Post, int Count), Post>(null,
            //    (x) => {
            //        x.Post.Title = "";
            //        return x.Post;
            //    });
            //    mapping.AddRow(null);
            //    mapping.ToList();
            //}
        }

        static void TestExpressionHelper(){
//            var a = new MyClass<(Post Post, Blog Blog, (int MaxId, int Count), decimal Total)>();
//            a.Test(x => new{
//                //x,
//                x.Post,
//                x.Blog,
//                x.Total,
//                x.Post.BlogId,
//                //x.Post.PostBlog,
//                //x.Post.PostBlog.Enable,
//                x.Item3.Count
//            });

            var a = new MyClass<Post>();
            a.Test(x => new{
                x,
                x.BlogId,
                x.PostAuthor.AuthorId,
                x.PostBlog,
                x.PostBlog.Enable
            });
        }

        class MyClass<T>
        {
            public void Test<TResult>(Expression<Func<T, TResult>> expression){
                var items = DbExpressionHelper.ExpandModelExpression(expression);
                items.ForEach(x => {
                    var cells = new List<string>();
                    x.ForEach(y => cells.Add(y.ToString()));
                    Console.WriteLine(string.Join(", ", cells));
                });

                var modelInfo = DbModelHelper.GetModelInfo(typeof(T));
                var columns = DbExpressionHelper.ReadColumnExpression(expression, modelInfo);

                columns.ForEach(x => Console.WriteLine(x));
            }
        }

        static void TestSyncSpeed(DbConnection dbcn){
            using (var db = new DbContext(dbcn)){
                Console.WriteLine("Start by Sync");
                DataTable dt;
                try{
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    dt = db.ExecQueryToDataTable("SELECT * FROM Blogs");
                    sw.Stop();
                    Console.WriteLine("END, Time = " + sw.ElapsedMilliseconds);
                    //Console.WriteLine(JsonConvert.SerializeObject(dt, Formatting.Indented));
                }
                catch (Exception ex){
                    Console.WriteLine("END");
                    if (ex.InnerException != null){
                        Console.WriteLine(ex.InnerException.Message);
                        Console.WriteLine(ex.InnerException.StackTrace);
                    }

                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        async static Task TestAsyncSpeed(DbConnection dbcn){
            using (var db = new DbContext(dbcn)){
                Console.WriteLine("Start by Async");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var dt = await db.ExecQueryToDataTableAsync("SELECT * FROM Blogs");
                sw.Stop();
                Console.WriteLine("END, Time = " + sw.ElapsedMilliseconds);
                //Console.WriteLine(JsonConvert.SerializeObject(dt, Formatting.Indented));
            }
        }

        async static Task TestConnection(){
            var dbcn = new DbConnection(DbDatabaseType.SqlServer,
                "server=127.0.0.1;uid=sa;pwd=-101868;database=EFDemo;Connect Timeout=900");

            using (var db = new DbContext(dbcn)){
                var dt = await db.ExecQueryToDataTableAsync("SELECT * FROM Blogs");
                Console.WriteLine(JsonConvert.SerializeObject(dt, Formatting.Indented));
            }
        }
    }
}