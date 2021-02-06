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
    public class TestSql
    {
        [Test]
        public void QueryBool(){
            var db = new DbContext(QuickStart.BuildConnection());

           var exist = db.Query<(bool Item1, bool)>("select true as Item1").ToFirst<bool>(x => x.Item1);
           Console.WriteLine(exist);
        }
        
        [Test]
        public async Task ExecuteBatch(){
            var db = new DbContext(QuickStart.BuildConnection());

            var batchSqlList = new List<string>();
            batchSqlList.Add(db.Delete<User>()
                .WhereBegin()
                .Compare(x => x.UserId, SqlCompareType.Equal, 12)
                .WhereEnded()
                .ToString());
            batchSqlList.Add(db.Delete<User>()
                .WhereBegin()
                .Compare(x => x.UserId, SqlCompareType.Equal, 12)
                .WhereEnded()
                .ToString());
            
            await db.ExecNoQueryAsync(batchSqlList);
        }
    }
}