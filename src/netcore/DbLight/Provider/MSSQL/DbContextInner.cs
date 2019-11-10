using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DbLight.Common;
using DbLight.Mapping;

namespace DbLight.Provider.MSSQL
{
    internal class DbContextInner : IDbContextInner
    {
        public DbConnection Connection{ get; }

        private SqlConnection _cn;
        private SqlTransaction _trans;

        public DbContextInner(DbConnection connection){
            Connection = connection;
        }

        public void Dispose(){
            try{
                _trans?.Dispose();
            }
            catch (Exception ex){
                Connection.Warn("Dispose Transaction Failed.", ex);
            }
            finally{
                _trans = null;
            }

            try{
                _cn?.Dispose();
            }
            catch (Exception ex){
                Connection.Warn("Dispose Connection Failed.", ex);
            }
            finally{
                _cn = null;
            }
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

            if (isTest){
                if (!Connection.SqlServer.IntegratedSecurity){
                    return string.Format("server={0};uid={1};pwd={2};Connect Timeout=5",
                        Connection.SqlServer.ServerName,
                        Connection.SqlServer.UserName,
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
                        (string.IsNullOrEmpty(Connection.SqlServer.Database)
                            ? "Database=" + Connection.SqlServer.Database + ";"
                            : "") + "Connect Timeout=900",
                        Connection.SqlServer.ServerName,
                        Connection.SqlServer.UserName,
                        Connection.SqlServer.Password);
                }
                else{
                    return string.Format(
                        "Data Source={0};Integrated Security=True;" +
                        (string.IsNullOrEmpty(Connection.SqlServer.Database)
                            ? "Database=" + Connection.SqlServer.Database + ";"
                            : "") + "Connect Timeout=900", Connection.SqlServer.ServerName);
                }
            }
        }

        public void BeginTransaction(){
            if (Connection.TestConnection){
                using (var cn = new SqlConnection(BuildConnectingString(true))){
                    cn.Open();
                }
            }

            var success = false;
            try{
                _cn = new SqlConnection(BuildConnectingString(false));
                _cn.Open();

                _trans = _cn.BeginTransaction();

                success = true;
            }
            catch (Exception ex){
                Connection.Warn("Begin Transaction Failed", ex);
                throw;
            }
            finally{
                if (!success){
                    try{
                        if (_trans != null){
                            _trans.Dispose();
                            _trans = null;
                        }
                    }
                    catch (Exception ex){
                        Connection.Warn("Dispose Transaction Failed.", ex);
                    }

                    try{
                        if (_cn != null){
                            _cn.Dispose();
                            _cn = null;
                        }
                    }
                    catch (Exception ex){
                        Connection.Warn("Dispose Connection Failed.", ex);
                    }
                }
            }
        }

        public async Task BeginTransactionAsync(){
            if (Connection.TestConnection){
                using (var cn = new SqlConnection(BuildConnectingString(true))){
                    await cn.OpenAsync();
                }
            }

            var success = false;
            try{
                _cn = new SqlConnection(BuildConnectingString(false));
                await _cn.OpenAsync();

                _trans = _cn.BeginTransaction();

                success = true;
            }
            catch (Exception ex){
                Connection.Warn("Begin Transaction Failed", ex);
                throw;
            }
            finally{
                if (!success){
                    try{
                        if (_trans != null){
                            _trans.Dispose();
                            _trans = null;
                        }
                    }
                    catch (Exception ex){
                        Connection.Warn("Dispose Transaction Failed.", ex);
                    }

                    try{
                        if (_cn != null){
                            _cn.Dispose();
                            _cn = null;
                        }
                    }
                    catch (Exception ex){
                        Connection.Warn("Dispose Connection Failed.", ex);
                    }
                }
            }
        }

        public void Commit(){
            _trans.Commit();
            _trans.Dispose();
            _trans = null;

            _cn.Dispose();
            _cn = null;
        }

        public void Rollback(){
            _trans.Rollback();
            _trans.Dispose();
            _trans = null;

            _cn.Dispose();
            _cn = null;
        }

        public bool TransactionOpened => (_trans != null);

        public DataTable ExecQueryToDataTable(string sql, int maxRecords){
            try{
                DataTable dt;

                if (_cn == null){
                    if (Connection.TestConnection){
                        using (var cn = new SqlConnection()){
                            cn.ConnectionString = BuildConnectingString(true);
                            cn.Open();
                        }
                    }

                    using (var cn = new SqlConnection()){
                        cn.ConnectionString = BuildConnectingString(false);
                        cn.Open();

                        dt = ExecQueryToDataTable_ExecuteReader(cn, null, sql, maxRecords);
                    }
                }
                else{
                    dt = ExecQueryToDataTable_ExecuteReader(_cn, _trans, sql, maxRecords);
                }

                Connection.Info(sql);
                return dt;
            }
            catch (Exception ex){
                Connection.Error(sql, ex);
                throw;
            }
        }

        private DataTable ExecQueryToDataTable_ExecuteReader(SqlConnection cn,
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

        public async Task<DataTable> ExecQueryToDataTableAsync(string sql, int maxRecords){
            try{
                DataTable dt;

                if (_cn == null){
                    if (Connection.TestConnection){
                        using (var cn = new SqlConnection()){
                            cn.ConnectionString = BuildConnectingString(true);
                            await cn.OpenAsync();
                        }
                    }

                    using (var cn = new SqlConnection()){
                        cn.ConnectionString = BuildConnectingString(false);
                        await cn.OpenAsync();
                        dt = await ExecQueryToDataTable_ExecuteReaderAsync(cn,
                            null, sql, maxRecords);
                    }
                }
                else{
                    dt = await ExecQueryToDataTable_ExecuteReaderAsync(_cn,
                        _trans, sql, maxRecords);
                }

                Connection.Info(sql);
                return dt;
            }
            catch (Exception ex){
                Connection.Error(sql, ex);
                throw;
            }
        }

        private async Task<DataTable> ExecQueryToDataTable_ExecuteReaderAsync(SqlConnection cn,
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

        public async Task<List<TResult>> ExecQueryToListAsync<T, TResult>(string sql, int maxRecords,
            Func<T, TResult> converter) where T : new(){
            try{
                List<TResult> list;

                if (_cn == null){
                    if (Connection.TestConnection){
                        using (var cn = new SqlConnection()){
                            cn.ConnectionString = BuildConnectingString(true);
                            await cn.OpenAsync();
                        }
                    }

                    using (var cn = new SqlConnection()){
                        cn.ConnectionString = BuildConnectingString(false);
                        await cn.OpenAsync();

                        list = await ExecQueryToListAsync_ExecuteReaderAsync(
                            cn, null, sql, maxRecords, converter);
                    }
                }
                else{
                    list = await ExecQueryToListAsync_ExecuteReaderAsync(
                        _cn, _trans, sql, maxRecords, converter);
                }

                Connection.Info(sql);
                return list;
            }
            catch (Exception ex){
                Connection.Error(sql, ex);
                throw;
            }
        }

        private async Task<List<TResult>> ExecQueryToListAsync_ExecuteReaderAsync<T, TResult>(
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

        public List<TResult> ExecQueryToList<T, TResult>(string sql, int maxRecords,
            Func<T, TResult> converter) where T : new(){
            try{
                List<TResult> list;

                if (_cn == null){
                    if (Connection.TestConnection){
                        using (var cn = new SqlConnection()){
                            cn.ConnectionString = BuildConnectingString(true);
                            cn.Open();
                        }
                    }

                    using (var cn = new SqlConnection()){
                        cn.ConnectionString = BuildConnectingString(false);
                        cn.Open();

                        list = ExecQueryToList_ExecuteReader(cn, null, sql, maxRecords, converter);
                    }
                }
                else{
                    list = ExecQueryToList_ExecuteReader(_cn, _trans, sql, maxRecords,
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

        private List<TResult> ExecQueryToList_ExecuteReader<T, TResult>(
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

        public async Task<int> ExecNoQueryAsync(string sql){
            try{
                int cnt;

                if (_cn == null){
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
                        cmd.Connection = _cn;
                        cmd.CommandTimeout = 0;
                        cmd.Transaction = _trans;
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

        public int ExecNoQuery(string sql){
            try{
                int cnt;

                if (_cn == null){
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
                        cmd.Connection = _cn;
                        cmd.CommandTimeout = 0;
                        cmd.Transaction = _trans;
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
    }
}