using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ZAD2_WebApplication.DAL;

namespace ZAD2_WebApplication.Middlewares
{
    public class ValidateStudentRequestPermission
    {
        private readonly RequestDelegate _next;

        public ValidateStudentRequestPermission(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("Index"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Brak numeru indexu w zapytaniu !");
                return;
            }
            string HeaderIndex = context.Request.Headers["Index"].ToString();
            if (!new SqlServerDbService().CheckIndexNumberInDB(HeaderIndex).Equals(HeaderIndex))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Numer indexu nieautoryzowany, brak dostępu do danych");
                return;
            }
            await _next(context);
        }
    }
}
