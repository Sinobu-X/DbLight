# DbLight
ORM for .NET Core

Supports MSSQL, Postgres

## Nuget
```csharp
Install-Package DbLightCore
```


## Quick Start

### Table & Model for MSSQL
```sql
CREATE TABLE [dbo].[User](
    [UserId] [INT] NOT NULL,
    [UserName] [NVARCHAR](128) NULL,
    [WeChatCode] [NVARCHAR](64) NULL,
    [Phone] [NVARCHAR](16) NULL,
    [Birthday] [DATETIME] NULL,
    [Income] [MONEY] NULL,
    [Height] [FLOAT] NULL,
    [SexId] [INT] NULL,
    [Married] [BIT] NULL,
    [Remark] [NVARCHAR](MAX) NULL,
    [Photo] [IMAGE] NULL,
    [RegisterTime] [DATETIME] NULL,
CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED(
    [UserId] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]) ON [PRIMARY]
```

```csharp
public class User
{
    public int UserId{ get; set; }
    public string UserName{ get; set; } = "";
    public string WeChatCode{ get; set; } = "";
    public string Phone{ get; set; } = "";
    public DateTime? Birthday{ get; set; }
    public decimal Income{ get; set; }
    public decimal Height{ get; set; }
    public int SexId{ get; set; }
    public bool Married{ get; set; }
    public string Remark{ get; set; } = "";
    public byte[] Photo{ get; set; }
    public DateTime RegisterTime{ get; set; }
}
```

### Table & Model for Postgres
```sql
create table "user"
(
	user_id integer not null
		constraint user_pk
			primary key,
	user_name varchar(64),
	we_chat_code varchar(64),
	phone varchar(16),
	birthday date,
	income money,
	height numeric,
	sex_id integer,
	married boolean,
	remark text,
	photo bytea,
	register_time time
);
```

```csharp
    [Table("user")]
    public class User
    {
        [Column("user_id")]
        public int UserId{ get; set; }

        [Column("user_name")] public string UserName{ get; set; } = "";

        [Column("we_chat_code")]
        public string WeChatCode{ get; set; } = "";

        [Column("phone")]
        public string Phone{ get; set; } = "";

        [Column("birthday")]
        public DateTime? Birthday{ get; set; }

        [Column("income")]
        public decimal Income{ get; set; }

        [Column("height")]
        public decimal Height{ get; set; }

        [Column("sex_id")]
        public int SexId{ get; set; }

        [Column("married")]
        public bool Married{ get; set; }

        [Column("remark")]
        public string Remark{ get; set; } = "";

        [Column("photo")]
        public byte[] Photo{ get; set; }

        [Column("register_time")]
        public DateTime RegisterTime{ get; set; }
    }
```

### Query
```csharp
public void Query(){
    var cn = new DbConnection(DbDatabaseType.SqlServer,
        "server=127.0.0.1;uid=test;pwd=test;database=DbLight");
    var db = new DbContext(cn);
    var users = db.Query<User>()
        .Where(x => x.UserId >= 1 && x.UserId < 10)
        .ToList();

    Console.WriteLine(JsonConvert.SerializeObject(users));
}
```

### Async Query
```csharp
public async Task QueryAsync(){
    var cn = new DbConnection(DbDatabaseType.SqlServer,
        "server=127.0.0.1;uid=test;pwd=test;database=DbLight");
    var db = new DbContext(cn);
    var users = await db.Query<User>()
        .Where(x => x.UserId >= 1 && x.UserId < 10)
        .ToListAsync();

    Console.WriteLine(JsonConvert.SerializeObject(users));
}
```

### Insert
```csharp
public static DbConnection BuildConnection(){
    return new DbConnection(DbDatabaseType.SqlServer,
        "server=127.0.0.1;uid=test;pwd=test;database=DbLight");
}

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
    user.RegisterTime = DateTime.Now;
    user.SexId = 2;

    await db.Insert(user).ExecuteAsync();
}
```

### Update
```csharp
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
    user.RegisterTime = DateTime.Now;
    user.SexId = 1;

    var updCount = await db.Update(user)
        .Where(x => x.UserId == user.UserId)
        .ExecuteAsync();

    Console.WriteLine($"Affect Count = {updCount}");
}
```

### Partial Update
```csharp
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
```

### Delete
```csharp
public async Task DeleteAsync(){
    var db = new DbContext(BuildConnection());

    var delCount = await db.Delete<User>()
        .Where(x => x.UserId == 2)
        .ExecuteAsync();

    Console.WriteLine($"Affect Count = {delCount}");
}
```

### Transaction
```csharp
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
```

### Batch
```csharp
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
```
