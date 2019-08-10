using System;

namespace DbLight.Exceptions
{
    public class DbUnknownException : Exception
    {
        public DbUnknownException(string message) :
            base(string.Format("{0}\nplease report this error to the provider.", message)){
        }
    }
}