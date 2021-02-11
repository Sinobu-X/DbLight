using System;
using System.Linq.Expressions;
using DbLight.DataAnnotations;

namespace DbLightTest.Postgres
{
    public class Database
    {
        public const string DEMO = "Demo";
        public const string PUBLIC = "public";
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