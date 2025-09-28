using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoodPlusAPI.Utils;

namespace MoodPlusAPI.Core
{
    [Authorize]
    [ApiController]
    public class CoreController : ControllerBase
    {
        public CoreController() { }

        protected virtual ObjectResult ProblemResponse(ApiError apiError)
        {
            return base.Problem(
                title: apiError.InternalMessage,
                detail: apiError.Message,
                statusCode: (int)apiError.StatusCode
            );
        }
    }
}
