using System;

namespace DbLight.Exceptions
{
    public class DbUnexpectedDbTypeException : Exception
    {
        public DbUnexpectedDbTypeException() : base("Unexpected database type."){
        }
    }
}