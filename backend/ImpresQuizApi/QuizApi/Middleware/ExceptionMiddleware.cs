using Microsoft.Data.SqlClient;
using System.Net;

namespace QuizApi.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx)
        {
            try { await _next(ctx); }
            catch (SqlException ex)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await ctx.Response.WriteAsJsonAsync(new { error = "SQL_ERROR", message = ex.Message });
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await ctx.Response.WriteAsJsonAsync(new { error = "SERVER_ERROR", message = ex.Message });
            }
        }
    }
}
