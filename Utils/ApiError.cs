using System.Net;

namespace MoodPlusAPI.Utils
{
    public class ApiError
    {
        public string Message { get; }
        public string InternalMessage { get; }
        public HttpStatusCode StatusCode { get; }

        public ApiError(string message, string internalMessage, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            Message = message;
            InternalMessage = internalMessage;
            StatusCode = statusCode;
        }

        public static ApiError NotFound(string message = "Entidade/Recurso não encontrado")
        {
            return new ApiError(message, "NotFound", HttpStatusCode.NotFound);
        }

        public static ApiError Unauthorized(string message = "Usuário não autorizado")
        {
            return new ApiError(message, "Unauthorized", HttpStatusCode.Unauthorized);
        }

        public static ApiError Conflict(string message = "Entidade/Recurso em conflito")
        {
            return new ApiError(message, "Conflict", HttpStatusCode.Conflict);
        }

        public static ApiError BadRequest(string message = "Requisição inválida")
        {
            return new ApiError(message, "BadRequest", HttpStatusCode.BadRequest);
        }

        public static ApiError Internal(string message = "Erro interno")
        {
            return new ApiError(message, "Internal", HttpStatusCode.InternalServerError);
        }
    }
}
