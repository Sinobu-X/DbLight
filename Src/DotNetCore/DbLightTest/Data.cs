using System;
using DbLight.DataAnnotations;

namespace DbLightTest
{
    public class Database
    {
        public const string DEMO = "Demo";
    }

    [Table("Blogs", Database = Database.DEMO)]
    public class Blog
    {
        public int BlogId{ get; set; }
        public string Url{ get; set; } = "";
        public int Rating{ get; set; }
        [Column("Active")] public bool Enable{ get; set; }
        public DateTime OpenTime{ get; set; } = DateTime.MinValue;

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
}