using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZAD2_WebApplication.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

            if (context.Request != null)
            {
                // Odczytanie request
                string path = context.Request.Path;
                string method = context.Request.Method.ToString();
                string querystring = context.Request.QueryString.ToString();
                string bodyStr = "";
                using (StreamReader reader =
                    new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }
                // Zapis do pliku
                string logsDir = Directory.GetCurrentDirectory().ToString()+"\\Logs\\requestsLog.txt";
                using (StreamWriter file =
                    new StreamWriter(@logsDir, true))
                {
                    file.WriteLine("[t] : "+DateTime.Now.ToString()+" | "+"[method] : "+method+" | "+"[path] : "+ path + " | " + "[querystring] : " + querystring + " | "+"[bodyStr] : "+bodyStr);
                }
            }

            await _next(context);
        }
    }
}
