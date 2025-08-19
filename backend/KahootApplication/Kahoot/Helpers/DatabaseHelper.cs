using Microsoft.Data.SqlClient;
using System.Data;

namespace Kahoot.Helpers
{
    public interface IDatabaseHelper
    {
        IDbConnection GetConnection();
    }

    public class DatabaseHelper : IDatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}