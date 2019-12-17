using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using DbLight.Sql;
using DbLight.Common;
using DbLight.Exceptions;

namespace DbLight
{
    public class DbContext : IDisposable
    {
        //private
        private readonly IDbContextInner _inner;

        public DbContext(DbConnection connection){
            if (connection.DbType == DbDatabaseType.SqlServer){
                _inner = new Provider.MSSQL.DbContextInner(connection);
                return;
            }
            else if (connection.DbType == DbDatabaseType.Postgres){
                _inner = new Provider.Postgres.DbContextInner(connection);
                return;
            }

            throw new DbUnexpectedDbTypeException();
        }

        public DbConnection Connection => _inner.Connection;

        public void Dispose(){
            _inner.Dispose();
        }

        public void BeginTransaction(){
            _inner.BeginTransaction();
        }

        public Task BeginTransactionAsync(){
            return _inner.BeginTransactionAsync();
        }

        public void Commit(){
            _inner.Commit();
        }

        public void Rollback(){
            _inner.Rollback();
        }

        public bool TransactionOpened => _inner.TransactionOpened;

        //Sync for DataTable
        public DataTable ExecQueryToDataTable(string sql, int maxRecords = 0){
            return _inner.ExecQueryToDataTable(sql, maxRecords);
        }

        //Async for DataTable
        public Task<DataTable> ExecQueryToDataTableAsync(string sql, int maxRecords = 0){
            return _inner.ExecQueryToDataTableAsync(sql, maxRecords);
        }

        //Async for List<T>
        public Task<List<T>> ExecQueryToListAsync<T>(string sql, int maxRecords = 0) where T : new(){
            return ExecQueryToListAsync<T, T>(sql, maxRecords, x => x);
        }

        public Task<List<TResult>> ExecQueryToListAsync<T, TResult>(string sql, Func<T, TResult> converter)
            where T : new(){
            return ExecQueryToListAsync(sql, 0, converter);
        }

        public Task<List<TResult>> ExecQueryToListAsync<T, TResult>(string sql, int maxRecords,
            Func<T, TResult> converter) where T : new(){
            return _inner.ExecQueryToListAsync(sql, maxRecords, converter);
        }

        //Sync for List<T>
        public List<T> ExecQueryToList<T>(string sql, int maxRecords = 0) where T : new(){
            return ExecQueryToList<T, T>(sql, maxRecords, x => x);
        }

        public List<TResult> ExecQueryToList<T, TResult>(string sql, Func<T, TResult> converter) where T : new(){
            return ExecQueryToList(sql, 0, converter);
        }

        public List<TResult> ExecQueryToList<T, TResult>(string sql, int maxRecords, Func<T, TResult> converter)
            where T : new(){
            return _inner.ExecQueryToList(sql, maxRecords, converter);
        }

        //Async for NoQuery
        public async Task ExecNoQueryAsync(IEnumerable<string> batchSql){
            var needBeginTransaction = false;
            if (!_inner.TransactionOpened){
                needBeginTransaction = true;
                await _inner.BeginTransactionAsync();
            }

            try{
                var sb = new StringBuilder();
                var contentLength = 0;
                foreach (var sql in batchSql){
                    contentLength += sql.Length;
                    sb.Append(sql);

                    if (contentLength > 8000){
                        await ExecNoQueryAsync(sb.ToString());
                        sb = new StringBuilder();
                        contentLength = 0;
                    }
                }

                if (contentLength > 0){
                    await ExecNoQueryAsync(sb.ToString());
                }

                if (needBeginTransaction){
                    _inner.Commit();
                }
            }
            catch(Exception){
                if (needBeginTransaction){
                   _inner.Rollback();
                }
                throw;
            }
        }

        public async Task<int> ExecNoQueryAsync(string sql){
            return await _inner.ExecNoQueryAsync(sql);
        }

        public void ExecNoQuery(IEnumerable<string> batchSql){
            var needBeginTransaction = false;
            if (!_inner.TransactionOpened){
                needBeginTransaction = true;
                _inner.BeginTransaction();
            }

            try{
                var sb = new StringBuilder();
                var contentLength = 0;
                foreach (var sql in batchSql){
                    contentLength += sql.Length;
                    sb.Append(sql);

                    if (contentLength > 8000){
                        ExecNoQuery(sb.ToString());
                        sb = new StringBuilder();
                        contentLength = 0;
                    }
                }

                if (contentLength > 0){
                    ExecNoQuery(sb.ToString());
                }

                if (needBeginTransaction){
                    _inner.Commit();
                }
            }
            catch(Exception){
                if (needBeginTransaction){
                    _inner.Rollback();
                }
                throw;
            }
        }

        //Sync for NoQuery
        public int ExecNoQuery(string sql){
            return _inner.ExecNoQuery(sql);
        }

        public SqlQuery<T> Query<T>() where T : new(){
            return new SqlQuery<T>(_inner.Connection, this);
        }

        public SqlQuery<T> Query<T>(string sql) where T : new(){
            return new SqlQuery<T>(sql, this);
        }

        public SqlQuery<T> ChildQuery<T>() where T : new(){
            return new SqlQuery<T>(_inner.Connection, this){
                IsChildQuery = true
            };
        }

        public SqlQuery<T> ChildQuery<T>(string sql) where T : new(){
            return new SqlQuery<T>(sql, this){
                IsChildQuery = true
            };
        }

        public SqlQuery ChildQuery(string sql){
            return new SqlQuery(sql, this){
                IsChildQuery = true
            };
        }

        public SqlExp Exp(string sql){
            return new SqlExp(sql);
        }

        public SqlDelete<T> Delete<T>() where T : new(){
            return new SqlDelete<T>(_inner.Connection, this);
        }

        public SqlInsert<T> Insert<T>(T item) where T : new(){
            return new SqlInsert<T>(_inner.Connection, this, item);
        }

        public SqlUpdate<T> Update<T>(T item) where T : new(){
            return new SqlUpdate<T>(_inner.Connection, this, item);
        }
    }
}