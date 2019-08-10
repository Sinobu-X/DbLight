using System;
using System.Collections.Generic;
using System.Reflection;

namespace DbLight.Common
{
//-------Object Model-------------------
//    [Table("Posts", Database = Database.DEMO)]
//    public class Post
//    {
//        public int PostId{ get; set; } = 0;
//        public string Title{ get; set; } = "";
//        public int BlogId{ get; set; } = 0;
//        public int AuthorId{ get; set; } = 0;
//        public Blog PostBlog{ get; set; } = new Blog();
//        public Author PostAuthor{ get; set; } = new Author();
//    }
//-------Object Sql-------------------
//    SELECT
//      [Post].[PostId] AS [PostId]
//     ,[Post].[Price] AS [Price]
//     ,[PostBlog].[Url] AS [PostBlog.Url]
//     ,[PostBlog].[Rating] AS [PostBlog.Rating]
//     ,[PostAuthor].[AuthorName] AS [PostAuthor.AuthorName]
//    FROM [EFDemo]..[Posts] AS [Post]
//    LEFT JOIN [EFDemo]..[Blogs] AS [PostBlog] ON [PostBlog].[BlogId] = [Post].[BlogId] 
//        LEFT JOIN [EFDemo]..[Authors] AS [PostAuthor] ON [PostAuthor].[AuthorId] = [Post].[AuthorId]
//--------------------------------------


//-------Tuple Model-------------------
//    Tuple<Blog, MaxRating, <Total, MaxPrice>>
//-------Object Sql-------------------
//    SELECT 
//        [Item1].[BlogId] AS [Item1.BlogId]
//       ,[Item1].[Url] AS [Item1.Url]
//       ,(SELECT MAX([a].[Rating]) FROM [EFDemo]..[Blogs] AS [a]) AS [Item2.Item]
//       ,[Item2].[Total] AS [Item3.Item1]
//       ,[Item2].[MaxPrice] AS [Item3.Item2]
//    FROM [EFDemo]..[Blogs] AS [Item1]
//    LEFT JOIN (SELECT
//          [a].[BlogId] AS [BlogId]
//         ,SUM([a].[Price]) AS [Total]
//         ,MAX([a].[Price]) AS [MaxPrice] 
//      FROM [EFDemo]..[Posts] AS [a] GROUP BY [a].[BlogId]) 
//      AS [Item2] ON [Item2].[BlogId] = [Item1].[BlogId]
//--------------------------------------


    public class DbModelInfo
    {
        public Type Type;
        public DbModelKind Kind = DbModelKind.Other;

        //Object & Tuple only
        public readonly List<DbMemberInfo> Members = new List<DbMemberInfo>();

        //Object only
        public string DatabaseName;
        public string TableName;
    }

    public class DbMemberInfo
    {
        public DbMemberType MemberType = DbMemberType.Property;
        public string MemberName;
        public PropertyInfo PropertyInfo;
        public FieldInfo FieldInfo;

        public string ColumnName;
        public bool Identity;
        public bool NotMapped;

        public DbModelInfo Model;
    }

    public enum DbModelKind
    {
        Tuple,
        Object,
        Value,
        Other,
    }

    public enum DbMemberType
    {
        Property,
        Field
    }

    public class DbTableModelInfo
    {
        public string Member{ get; set; } = "";
        public string Table{ get; set; } = "";
        public string Database{ get; set; } = "";
    }

    public class DbColumnModelInfo
    {
        public string Member{ get; set; } = "";
        public string Column{ get; set; } = "";
    }
}