using System;

namespace DbLight.Exceptions
{
    public class DbCrashException : Exception
    {
        public DbCrashException(string message) :
            base(string.Format("{0}\nplease report this error to the provider.", message)){
        }
    }
}