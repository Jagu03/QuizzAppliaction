using Microsoft.Data.SqlClient;
using System.Data;

namespace QuizApi.Infrastructure
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _cs;
        public SqlConnectionFactory(string connectionString) => _cs = connectionString;
        public IDbConnection Create() => new SqlConnection(_cs);
    }
}
