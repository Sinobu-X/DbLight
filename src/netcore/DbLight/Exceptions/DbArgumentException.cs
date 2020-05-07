using System;

namespace DbLight.Exceptions
{
    public class DbArgumentException: Exception
    {
        public DbArgumentException(string message) : base(message){
        }
    }
}