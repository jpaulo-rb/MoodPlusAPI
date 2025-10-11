using System.Security.Claims;
using MoodPlusAPI.Usuarios;

namespace MoodPlusAPI.Extensions
{
    public class RequestContextMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, RequestContext requestContext)
        {
            if (context.User.Identity?.IsAuthenticated ?? false)
            {
                requestContext.UsuarioSessao = new UsuarioSessao()
                {
                    Id = context.User.FindFirst("Id")?.Value,
                    EmpresaId = context.User.FindFirst("EmpresaId")?.Value,
                    Email = context.User.FindFirst("Email")?.Value,
                    Regra = (Regra)Enum.Parse(typeof(Regra), context.User.FindFirst(ClaimTypes.Role)?.Value)
                };

                /*
                if (string.IsNullOrEmpty(requestContext.EmpresaId))
                    // Opcional: bloquear request se EmpresaId não estiver presente
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("EmpresaId não encontrado no token.");
                    return;
                }
                */
            }

            await _next(context);
        }
    }
}
