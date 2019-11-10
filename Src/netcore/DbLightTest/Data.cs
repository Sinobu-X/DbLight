using System;
using DbLight.DataAnnotations;

namespace DbLightTest
{
    public class Database
    {
        public const string DEMO = "Demo";
        public const string PUBLIC = "public";
    }

    [Table("Blogs", Database = Database.DEMO)]
    public class Blog
    {
        public int BlogId{ get; set; }
        public string Url{ get; set; } = "";
        public int Rating{ get; set; }
        [Column("Active")] public bool Enable{ get; set; }
        public DateTime? OpenTime{ get; set; }

        [NotMapped] public string Remark => string.Format("#{0} is {1}", BlogId, Enable ? "Active" : "Inactive");
        public Author PostAuthor{ get; set; } = new Author();
    }

    [Table("Posts", Database = Database.DEMO)]
    public class Post
    {
        public int PostId{ get; set; }
        public string Title{ get; set; } = "";
        public int BlogId{ get; set; }
        public int AuthorId{ get; set; }
        public decimal Price{ get; set; }
        
        public Blog PostBlog{ get; set; } = new Blog();
        public Author PostAuthor{ get; set; } = new Author();
    }

    [Table("Authors", Database = Database.DEMO)]
    public class Author
    {
        [Column(Identity = true)] public int ID{ get; set; }
        public int AuthorId{ get; set; }
        public string AuthorName{ get; set; } = "";
    }

    public class PostView : Post
    {
        public string AuthorName{ get; set; } = "";
    }

    [Table("role")]
    public class Role
    {
        [Column("role_id")]
        public int RoleId{ get; set; }

        [Column("role_name")]
        public string RoleName{ get; set; }
    }

    [Table("role_user")]
    public class RoleUser
    {
        [Column("role_id")]
        public int RoleId{ get; set; }

        [Column("user_id")]
        public int UserId{ get; set; }
    }

    [Table("sex")]
    public class Sex
    {
        [Column("sex_id")]
        public int SexId{ get; set; }

        [Column("sex_name")]
        public string SexName{ get; set; }
    }

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


}