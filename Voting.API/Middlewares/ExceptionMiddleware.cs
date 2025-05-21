using System.Net;
using System.Text.Json;
//using Microsoft.Data.SqlClient; //z Для обработки ошибок SQL Server
using Microsoft.EntityFrameworkCore;

namespace Voting.API.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                logger.LogError("Something went wrong: {ex}", ex);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = exception switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                ArgumentNullException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                //SqlException => (int)HttpStatusCode.ServiceUnavailable,    // Обработка ошибок подключения/запроса к базе данных SQL
                DbUpdateException => (int)HttpStatusCode.ServiceUnavailable, // Обработка ошибок обновления/инициализации базы данных через EF Core
                _ => (int)HttpStatusCode.InternalServerError,
            };

            var response = new
            {
                context.Response.StatusCode,
                Message = GetDefaultMessageForStatusCode(context.Response.StatusCode),
                Detailed = exception.Message // Для отладки
            };

            string jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }

        private static string GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                (int)HttpStatusCode.Unauthorized => "Unauthorized access.",
                (int)HttpStatusCode.BadRequest => "Bad request. Please verify your inputs.",
                (int)HttpStatusCode.NotFound => "Resource not found.",
                (int)HttpStatusCode.ServiceUnavailable => "Database connection issue. Please try again later.",
                (int)HttpStatusCode.InternalServerError => "Internal Server Error. Please try again later.",
                _ => "An unexpected error occurred."
            };
        }
    }
}
