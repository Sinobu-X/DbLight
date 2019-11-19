using System;
using DbLight.DataAnnotations;

namespace DbLightTest.MSSQL
{
    public class Database
    {
        public const string DEMO = "Demo";
        public const string PUBLIC = "public";
    }

    public class Role
    {
        public int RoleId{ get; set; }

        public string RoleName{ get; set; }
    }

    public class RoleUser
    {
        public int RoleId{ get; set; }

        public int UserId{ get; set; }
    }

    public class Sex
    {
        public int SexId{ get; set; }

        public string SexName{ get; set; }
    }

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


}