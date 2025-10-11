using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MoodPlusAPI.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception E)
            {
                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

                _logger.LogError(E, "Erro interno capturado pelo Middleware. TraceId: {traceId} ", traceId);

                var problemDetails = new ProblemDetails()
                {
                    Type = "https://httpstatuses.com/500",
                    Title = "Erro interno",
                    Status = 500,
                    Detail = "Ocorreu um erro interno, favor aguarde um momento ou solicite suporte.",
                    Extensions = { ["traceId"] = traceId }
                };

                context.Response.StatusCode = 500;

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
