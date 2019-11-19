using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DbLight.Common;

namespace DbLight
{
    public interface IDbContextInner : IDisposable
    {
        DbConnection Connection{ get; }

        void BeginTransaction();

        Task BeginTransactionAsync();

        void Commit();

        void Rollback();

        bool TransactionOpened{ get; }

        DataTable ExecQueryToDataTable(string sql, int maxRecords);

        Task<DataTable> ExecQueryToDataTableAsync(string sql, int maxRecords);

        Task<List<TResult>> ExecQueryToListAsync<T, TResult>(string sql, int maxRecords,
            Func<T, TResult> converter) where T : new();

        List<TResult> ExecQueryToList<T, TResult>(string sql, int maxRecords, Func<T, TResult> converter)
            where T : new();

        Task<int> ExecNoQueryAsync(string sql);

        int ExecNoQuery(string sql);
    }
}