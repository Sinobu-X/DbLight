namespace DbLight.Sql
{
    public class SqlExp
    {
        private readonly string _sql;

        public SqlExp(string sql){
            _sql = sql;
        }
       
        public T To<T>(){
            return default(T);
        }

        public bool In<T>(T value){
            return true;
        }

        public override string ToString(){
            return _sql;
        }
    }
}