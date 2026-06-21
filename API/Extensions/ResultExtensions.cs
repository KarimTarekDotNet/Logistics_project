using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
        {
            if (result.IsSuccess)
                return result.StatusCode == 201
                    ? controller.StatusCode(201, result.Value)
                    : controller.Ok(result.Value);

            return result.StatusCode switch
            {
                404 => controller.NotFound(new { message = result.Error }),
                401 => controller.Unauthorized(new { message = result.Error }),
                403 => controller.StatusCode(403, new { message = result.Error }),
                _   => controller.BadRequest(new { message = result.Error })
            };
        }

        public static IActionResult ToActionResult(this Result result, ControllerBase controller)
        {
            if (result.IsSuccess) return controller.Ok();

            return result.StatusCode switch
            {
                404 => controller.NotFound(new { message = result.Error }),
                401 => controller.Unauthorized(new { message = result.Error }),
                403 => controller.StatusCode(403, new { message = result.Error }),
                _   => controller.BadRequest(new { message = result.Error })
            };
        }

        public static IActionResult ToCreatedResult<T>(this Result<T> result, ControllerBase controller, string actionName, object routeValues)
        {
            if (result.IsSuccess)
                return controller.CreatedAtAction(actionName, routeValues, result.Value);

            return result.StatusCode switch
            {
                404 => controller.NotFound(new { message = result.Error }),
                401 => controller.Unauthorized(new { message = result.Error }),
                403 => controller.StatusCode(403, new { message = result.Error }),
                _   => controller.BadRequest(new { message = result.Error })
            };
        }
    }
}
