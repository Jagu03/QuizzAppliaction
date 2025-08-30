using System.Data;

namespace QuizApi.Infrastructure
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}
