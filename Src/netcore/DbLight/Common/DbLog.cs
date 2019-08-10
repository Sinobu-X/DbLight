using System;

namespace DbLight.Common
{
    public delegate void DbLogError(string message, Exception ex);

    public delegate void DBLogWarn(string message, Exception ex);
    
    public delegate void DBLogInfo(string message);
}