namespace DbLight.Common
{
    internal interface IDbSqlInner
    {
        string GetTableName(DbConnection connection, string database, string schema, string table);

        string GetColumnName(string column);

        string ValueToWhereSql(object value);

        string ValueToSetSql(object value);

        string ValueToLikeSql(DbWhereLikeType likeType, string value);

        string ToLikeSql(string express, DbWhereLikeType likeType, string value);

        string TopSqlTop(int top);
        string TopSqlWhere(int top);
        string TopSqlLimit(int top);
        string OffsetSql(int offset);
    }
}