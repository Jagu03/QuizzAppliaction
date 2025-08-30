using System.Data;

namespace QuizApi.Infrastructure
{
    public abstract class RepositoryBase
    {
        private readonly IDbConnectionFactory _factory;
        protected RepositoryBase(IDbConnectionFactory factory) => _factory = factory;
        protected IDbConnection Open() { var c = _factory.Create(); c.Open(); return c; }
    }
}
