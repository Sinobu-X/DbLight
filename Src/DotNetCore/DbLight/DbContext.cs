using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DbLight.Sql;
using DbLight.Common;
using System.Collections.Generic;
using System.Text;
using DbLight.Exceptions;
using DbLight.Mapping;

namespace DbLight
{
    public class DbContext : IDisposable
    {
        //public
        public DbConnection Connection{ get; }

        //private
        private SqlConnection _cnSqlServer;
        private SqlTransaction _transSqlServer;

        public DbContext(DbConnection connection){
            Connection = connection;
        }

        public void Dispose(){
            if (Connection.DbType == DbDatabaseType.SqlServer){
                Dispose_SqlServer();
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        private void Dispose_SqlServer(){
            try{
                _transSqlServer?.Dispose();
            }
            catch (Exception ex){
                Connection.Warn("Dispose SqlServer Transaction Failed.", ex);
            }
            finally{
                _transSqlServer = null;
            }

            try{
                _cnSqlServer?.Dispose();
            }
            catch (Exception ex){
                Connection.Warn("Dispose SqlServer Connection Failed.", ex);
            }
            finally{
                _cnSqlServer = null;
            }
        }

        public void BeginTransaction(){
            if (Connection.DbType == DbDatabaseType.SqlServer){
                BeginTransaction_SqlServer();
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        private void BeginTransaction_SqlServer(){
            if (Connection.TestConnection){
                using (var cn = new SqlConnection(BuildConnectingString(true))){
                    cn.Open();
                }
            }

            var success = false;
            try{
                _cnSqlServer = new SqlConnection(BuildConnectingString(false));
                _cnSqlServer.Open();

                _transSqlServer = _cnSqlServer.BeginTransaction();

                success = true;
            }
            catch (Exception ex){
                Connection.Warn("Begin SqlServer Transaction Failed", ex);
                throw;
            }
            finally{
                if (!success){
                    try{
                        if (_transSqlServer != null){
                            _transSqlServer.Dispose();
                            _transSqlServer = null;
                        }
                    }
                    catch (Exception ex){
                        Connection.Warn("Dispose SqlServer Transaction Failed.", ex);
                    }

                    try{
                        if (_cnSqlServer != null){
                            _cnSqlServer.Dispose();
                            _cnSqlServer = null;
                        }
                    }
                    catch (Exception ex){
                        Connection.Warn("Dispose SqlServer Connection Failed.", ex);
                    }
                }
            }
        }

        public Task BeginTransactionAsync(){
            if (Connection.DbType == DbDatabaseType.SqlServer){
                return BeginTransaction_SqlServerAsync();
            }

            throw new DbUnexpectedDbTypeException();
        }

        private async Task BeginTransaction_SqlServerAsync(){
            if (Connection.TestConnection){
                using (var cn = new SqlConnection(BuildConnectingString(true))){
                    await cn.OpenAsync();
                }
            }

            var success = false;
            try{
                _cnSqlServer = new SqlConnection(BuildConnectingString(false));
                await _cnSqlServer.OpenAsync();

                _transSqlServer = _cnSqlServer.BeginTransaction();

                success = true;
            }
            catch (Exception ex){
                Connection.Warn("Begin SqlServer Transaction Failed", ex);
                throw;
            }
            finally{
                if (!success){
                    try{
                        if (_transSqlServer != null){
                            _transSqlServer.Dispose();
                            _transSqlServer = null;
                        }
                    }
                    catch (Exception ex){
                        Connection.Warn("Dispose SqlServer Transaction Failed.", ex);
                    }

                    try{
                        if (_cnSqlServer != null){
                            _cnSqlServer.Dispose();
                            _cnSqlServer = null;
                        }
                    }
                    catch (Exception ex){
                        Connection.Warn("Dispose SqlServer Connection Failed.", ex);
                    }
                }
            }
        }

        public void Commit(){
            if (Connection.DbType == DbDatabaseType.SqlServer){
                Commit_SqlServer();
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        private void Commit_SqlServer(){
            _transSqlServer.Commit();
            _transSqlServer.Dispose();
            _transSqlServer = null;

            _cnSqlServer.Dispose();
            _cnSqlServer = null;
        }

        public void Rollback(){
            if (Connection.DbType == DbDatabaseType.SqlServer){
                Rollback_SqlServer();
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        private void Rollback_SqlServer(){
            _transSqlServer.Rollback();
            _transSqlServer.Dispose();
            _transSqlServer = null;

            _cnSqlServer.Dispose();
            _cnSqlServer = null;
        }

        public bool TransactionOpened{
            get{
                if (Connection.DbType == DbDatabaseType.SqlServer){
                    return TransactionOpened_SqlServer();
                }
                else{
                    throw new DbUnexpectedDbTypeException();
                }
            }
        }

        private bool TransactionOpened_SqlServer(){
            return (_transSqlServer != null);
        }

        private string BuildConnectingString(bool isTest){
            if (Connection.ConnectionString != null){
                if (isTest && Connection.TestConnectionString != null){
                    return Connection.TestConnectionString;
                }
                else{
                    return Connection.ConnectionString;
                }
            }

            if (Connection.DbType == DbDatabaseType.SqlServer){
                #region SqlServer

                if (isTest){
                    if (!Connection.SqlServer.IntegratedSecurity){
                        return string.Format("server={0};uid={1};pwd={2};Connect Timeout=5",
                            Connection.SqlServer.ServerName, Connection.SqlServer.UserName,
                            Connection.SqlServer.Password);
                    }
                    else{
                        return string.Format("Data Source={0}; Integrated Security=True;Connect Timeout=5",
                            Connection.SqlServer.ServerName);
                    }
                }
                else{
                    if (!Connection.SqlServer.IntegratedSecurity){
                        return string.Format(
                            "server={0};uid={1};pwd={2};" +
                            (Connection.SqlServer.Database != ""
                                ? "Database=" + Connection.SqlServer.Database + ";"
                                : "") + "Connect Timeout=900", Connection.SqlServer.ServerName,
                            Connection.SqlServer.UserName, Connection.SqlServer.Password);
                    }
                    else{
                        return string.Format(
                            "Data Source={0};Integrated Security=True;" +
                            (Connection.SqlServer.Database != ""
                                ? "Database=" + Connection.SqlServer.Database + ";"
                                : "") + "Connect Timeout=900", Connection.SqlServer.ServerName);
                    }
                }

                #endregion
            }
            else if (Connection.DbType == DbDatabaseType.Access){
                #region Access

                if (isTest){
                    return string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"{0}\";Connect Timeout=5",
                        Connection.Access.FileName);
                }
                else{
                    return string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"{0}\";Connect Timeout=7200",
                        Connection.Access.FileName);
                }

                #endregion
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        //Sync for DataTable
        public DataTable ExecQueryToDataTable(string sql, int maxRecords = 0){
            if (Connection.DbType == DbDatabaseType.SqlServer){
                return ExecQueryToDataTable_SqlServer(sql, maxRecords);
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        private DataTable ExecQueryToDataTable_SqlServer(string sql, int maxRecords){
            try{
                DataTable dt;

                if (_cnSqlServer == null){
                    if (Connection.TestConnection){
                        using (var cn = new SqlConnection()){
                            cn.ConnectionString = BuildConnectingString(true);
                            cn.Open();
                        }
                    }

                    using (var cn = new SqlConnection()){
                        cn.ConnectionString = BuildConnectingString(false);
                        cn.Open();

                        dt = ExecQueryToDataTable_SqlServer_ExecuteReader(cn, null, sql, maxRecords);
                    }
                }
                else{
                    dt = ExecQueryToDataTable_SqlServer_ExecuteReader(_cnSqlServer, _transSqlServer, sql, maxRecords);
                }

                Connection.Info(sql);
                return dt;
            }
            catch (Exception ex){
                Connection.Error(sql, ex);
                throw;
            }
        }

        private DataTable ExecQueryToDataTable_SqlServer_ExecuteReader(SqlConnection cn,
            SqlTransaction transaction, string sql, int maxRecords){
            var dt = new DataTable();
            using (var cmd = new SqlCommand()){
                cmd.Connection = cn;
                cmd.CommandTimeout = 0;
                cmd.Transaction = transaction;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                using (var dr = cmd.ExecuteReader(CommandBehavior.SequentialAccess)){
                    dt.BeginLoadData();

                    for (var i = 0; i < dr.FieldCount; i++){
                        dt.Columns.Add(dr.GetName(i), dr.GetFieldType(i));
                    }

                    var rowCount = 0;
                    while (dr.Read()){
                        var items = new object[dr.FieldCount];
                        dr.GetValues(items);
                        dt.LoadDataRow(items, true);

                        rowCount++;
                        if (maxRecords > 0 && rowCount == maxRecords){
                            break;
                        }
                    }

                    dt.EndLoadData();
                }
            }

            return dt;
        }

        //Async for DataTable
        public Task<DataTable> ExecQueryToDataTableAsync(string sql, int maxRecords = 0){
            if (Connection.DbType == DbDatabaseType.SqlServer){
                return ExecQueryToDataTable_SqlServerAsync(sql, maxRecords);
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        private async Task<DataTable> ExecQueryToDataTable_SqlServerAsync(string sql, int maxRecords){
            try{
                DataTable dt;

                if (_cnSqlServer == null){
                    if (Connection.TestConnection){
                        using (var cn = new SqlConnection()){
                            cn.ConnectionString = BuildConnectingString(true);
                            await cn.OpenAsync();
                        }
                    }

                    using (var cn = new SqlConnection()){
                        cn.ConnectionString = BuildConnectingString(false);
                        await cn.OpenAsync();
                        dt = await ExecQueryToDataTable_SqlServer_ExecuteReaderAsync(cn,
                            null, sql, maxRecords);
                    }
                }
                else{
                    dt = await ExecQueryToDataTable_SqlServer_ExecuteReaderAsync(_cnSqlServer,
                        _transSqlServer, sql, maxRecords);
                }

                Connection.Info(sql);
                return dt;
            }
            catch (Exception ex){
                Connection.Error(sql, ex);
                throw;
            }
        }

        private async Task<DataTable> ExecQueryToDataTable_SqlServer_ExecuteReaderAsync(SqlConnection cn,
            SqlTransaction transaction, string sql, int maxRecords){
            var dt = new DataTable();

            using (var cmd = new SqlCommand()){
                cmd.Connection = cn;
                cmd.CommandTimeout = 0;
                cmd.Transaction = transaction;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                using (var dr = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess)){
                    dt.BeginLoadData();

                    for (var i = 0; i < dr.FieldCount; i++){
                        dt.Columns.Add(dr.GetName(i), dr.GetFieldType(i));
                    }

                    var rowCount = 0;
                    while (await dr.ReadAsync()){
                        var items = new object[dr.FieldCount];
                        dr.GetValues(items);
                        dt.LoadDataRow(items, true);

                        rowCount++;
                        if (maxRecords > 0 && rowCount == maxRecords){
                            break;
                        }
                    }

                    dt.EndLoadData();
                }
            }

            return dt;
        }


        //Async for List<T>
        public Task<List<T>> ExecQueryToListAsync<T>(string sql, int maxRecords = 0) where T : new(){
            return ExecQueryToListAsync<T, T>(sql, maxRecords, x => x);
        }

        public Task<List<TResult>> ExecQueryToListAsync<T, TResult>(string sql, Func<T, TResult> converter)
            where T : new(){
            return ExecQueryToListAsync<T, TResult>(sql, 0, converter);
        }

        public Task<List<TResult>> ExecQueryToListAsync<T, TResult>(string sql, int maxRecords,
            Func<T, TResult> converter) where T : new(){
            if (Connection.DbType == DbDatabaseType.SqlServer){
                return ExecQueryToListAsync_SqlServerAsync<T, TResult>(sql, maxRecords, converter);
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        private async Task<List<TResult>> ExecQueryToListAsync_SqlServerAsync<T, TResult>(string sql, int maxRecords,
            Func<T, TResult> converter) where T : new(){
            try{
                List<TResult> list;

                if (_cnSqlServer == null){
                    if (Connection.TestConnection){
                        using (var cn = new SqlConnection()){
                            cn.ConnectionString = BuildConnectingString(true);
                            await cn.OpenAsync();
                        }
                    }

                    using (var cn = new SqlConnection()){
                        cn.ConnectionString = BuildConnectingString(false);
                        await cn.OpenAsync();

                        list = await ExecQueryToListAsync_SqlServer_ExecuteReaderAsync(
                            cn, null, sql, maxRecords, converter);
                    }
                }
                else{
                    list = await ExecQueryToListAsync_SqlServer_ExecuteReaderAsync(
                        _cnSqlServer, _transSqlServer, sql, maxRecords, converter);
                }

                Connection.Info(sql);
                return list;
            }
            catch (Exception ex){
                Connection.Error(sql, ex);
                throw;
            }
        }

        private async Task<List<TResult>> ExecQueryToListAsync_SqlServer_ExecuteReaderAsync<T, TResult>(
            SqlConnection cn, SqlTransaction transaction, string sql, int maxRecords,
            Func<T, TResult> converter) where T : new(){
            List<TResult> list;

            using (var cmd = new SqlCommand()){
                cmd.Connection = cn;
                cmd.CommandTimeout = 0;
                cmd.Transaction = transaction;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                using (var dr = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess)){
                    var dt = new DataTable();
                    for (var i = 0; i < dr.FieldCount; i++){
                        dt.Columns.Add(dr.GetName(i), dr.GetFieldType(i));
                    }

                    var mapping = new DataTableMapping<T, TResult>(dt.Columns, converter);

                    var rowCount = 0;
                    while (await dr.ReadAsync()){
                        var values = new object[dr.FieldCount];
                        dr.GetValues(values);

                        mapping.AddRow(values);

                        rowCount++;
                        if (maxRecords > 0 && rowCount == maxRecords){
                            break;
                        }
                    }

                    list = mapping.ToList();
                }
            }

            return list;
        }


        //Sync for List<T>
        public List<T> ExecQueryToList<T>(string sql, int maxRecords = 0) where T : new(){
            return ExecQueryToList<T, T>(sql, maxRecords, x => x);
        }

        public List<TResult> ExecQueryToList<T, TResult>(string sql, Func<T, TResult> converter) where T : new(){
            return ExecQueryToList<T, TResult>(sql, 0, converter);
        }

        public List<TResult> ExecQueryToList<T, TResult>(string sql, int maxRecords, Func<T, TResult> converter)
            where T : new(){
            if (Connection.DbType == DbDatabaseType.SqlServer){
                return ExecQueryToList_SqlServer<T, TResult>(sql, maxRecords, converter);
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        private List<TResult> ExecQueryToList_SqlServer<T, TResult>(string sql, int maxRecords,
            Func<T, TResult> converter) where T : new(){
            try{
                List<TResult> list;

                if (_cnSqlServer == null){
                    if (Connection.TestConnection){
                        using (var cn = new SqlConnection()){
                            cn.ConnectionString = BuildConnectingString(true);
                            cn.Open();
                        }
                    }

                    using (var cn = new SqlConnection()){
                        cn.ConnectionString = BuildConnectingString(false);
                        cn.Open();

                        list = ExecQueryToList_SqlServer_ExecuteReader(cn, null, sql, maxRecords, converter);
                    }
                }
                else{
                    list = ExecQueryToList_SqlServer_ExecuteReader(_cnSqlServer, _transSqlServer, sql, maxRecords,
                        converter);
                }

                Connection.Info(sql);
                return list;
            }
            catch (Exception ex){
                Connection.Error(sql, ex);
                throw;
            }
        }

        private List<TResult> ExecQueryToList_SqlServer_ExecuteReader<T, TResult>(
            SqlConnection cn, SqlTransaction transaction,
            string sql, int maxRecords, Func<T, TResult> converter) where T : new(){
            List<TResult> list;

            using (var cmd = new SqlCommand()){
                cmd.Connection = cn;
                cmd.CommandTimeout = 0;
                cmd.Transaction = transaction;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                using (var dr = cmd.ExecuteReader(CommandBehavior.SequentialAccess)){
                    var dt = new DataTable();
                    for (var i = 0; i < dr.FieldCount; i++){
                        dt.Columns.Add(dr.GetName(i), dr.GetFieldType(i));
                    }

                    var mapping = new DataTableMapping<T, TResult>(dt.Columns, converter);

                    var rowCount = 0;
                    while (dr.Read()){
                        var values = new object[dr.FieldCount];
                        dr.GetValues(values);

                        mapping.AddRow(values);

                        rowCount++;
                        if (maxRecords > 0 && rowCount == maxRecords){
                            break;
                        }
                    }

                    list = mapping.ToList();
                }
            }

            return list;
        }


        //Async for NoQuery
        public async Task ExecNoQueryAsync(IEnumerable<string> batchSql){
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
        }
        
        public async Task<int> ExecNoQueryAsync(string sql){
            if (Connection.DbType == DbDatabaseType.SqlServer){
                return await ExecNoQuery_SqlServerAsync(sql);
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        private async Task<int> ExecNoQuery_SqlServerAsync(string sql){
            try{
                int cnt;

                if (_cnSqlServer == null){
                    if (Connection.TestConnection){
                        using (var cn = new SqlConnection()){
                            cn.ConnectionString = BuildConnectingString(true);
                            await cn.OpenAsync();
                        }
                    }

                    using (var cn = new SqlConnection()){
                        cn.ConnectionString = BuildConnectingString(false);
                        await cn.OpenAsync();

                        using (var cmd = new SqlCommand()){
                            cmd.Connection = cn;
                            cmd.CommandTimeout = 0;
                            //cmd.Transaction = _trans_sqlserver;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sql;

                            cnt = await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                else{
                    using (var cmd = new SqlCommand()){
                        cmd.Connection = _cnSqlServer;
                        cmd.CommandTimeout = 0;
                        cmd.Transaction = _transSqlServer;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;

                        cnt = await cmd.ExecuteNonQueryAsync();
                    }
                }

                Connection.Info(sql);
                return cnt;
            }
            catch (Exception ex){
                Connection.Error(sql, ex);
                throw;
            }
        }

        public void ExecNoQuery(IEnumerable<string> batchSql){
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
        }

        //Sync for NoQuery
        public int ExecNoQuery(string sql){
            if (Connection.DbType == DbDatabaseType.SqlServer){
                return ExecNoQuery_SqlServer(sql);
            }
            else{
                throw new DbUnexpectedDbTypeException();
            }
        }

        private int ExecNoQuery_SqlServer(string sql){
            try{
                int cnt;

                if (_cnSqlServer == null){
                    if (Connection.TestConnection){
                        using (var cn = new SqlConnection()){
                            cn.ConnectionString = BuildConnectingString(true);
                            cn.Open();
                        }
                    }

                    using (var cn = new SqlConnection()){
                        cn.ConnectionString = BuildConnectingString(false);
                        cn.Open();

                        using (var cmd = new SqlCommand()){
                            cmd.Connection = cn;
                            cmd.CommandTimeout = 0;
                            //cmd.Transaction = _trans_sqlserver;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sql;

                            cnt = cmd.ExecuteNonQuery();
                        }
                    }
                }
                else{
                    using (var cmd = new SqlCommand()){
                        cmd.Connection = _cnSqlServer;
                        cmd.CommandTimeout = 0;
                        cmd.Transaction = _transSqlServer;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;

                        cnt = cmd.ExecuteNonQuery();
                    }
                }

                Connection.Info(sql);
                return cnt;
            }
            catch (Exception ex){
                Connection.Error(sql, ex);
                throw;
            }
        }


        public SqlQuery<T> Query<T>() where T : new(){
            return new SqlQuery<T>(Connection, this);
        }

        public SqlQuery<T> Query<T>(string sql) where T : new(){
            return new SqlQuery<T>(sql, this);
        }

        public SqlQuery<T> ChildQuery<T>() where T : new(){
            return new SqlQuery<T>(Connection, this){
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
            return new SqlDelete<T>(Connection, this);
        }

        public SqlInsert<T> Insert<T>(T item) where T : new(){
            return new SqlInsert<T>(Connection, this, item);
        }

        public SqlUpdate<T> Update<T>(T item) where T : new(){
            return new SqlUpdate<T>(Connection, this, item);
        }
    }
}