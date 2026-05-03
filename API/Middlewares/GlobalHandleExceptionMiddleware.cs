using Domain.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace API.Middlewares
{
    public class GlobalHandleExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalHandleExceptionMiddleware> _logger;

        public GlobalHandleExceptionMiddleware(RequestDelegate next, ILogger<GlobalHandleExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                int statusCode;
                string message;

                if (ex is NotFoundException)
                {
                    statusCode = StatusCodes.Status404NotFound;
                    message = ex.Message;
                }
                else if (ex is UnauthorizedAccessException)
                {
                    statusCode = StatusCodes.Status403Forbidden;
                    message = ex.Message;
                }
                else if (ex is ArgumentException)
                {
                    statusCode = StatusCodes.Status400BadRequest;
                    message = "Invalid input provided.";
                }
                else if (ex is InvalidOperationException)
                {
                    statusCode = StatusCodes.Status409Conflict;
                    message = "Operation cannot be completed due to current state.";
                }
                else if (ex is SqlException)
                {
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "A database error occurred.";
                }
                else if (ex is NullReferenceException)
                {
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "An unexpected error occurred.";
                }
                else if (ex is DbUpdateConcurrencyException)
                {
                    statusCode = StatusCodes.Status409Conflict;
                    message = "The item was modified by someone else. Please reload and try again.";
                }
                else if (ex is BusinessRuleException)
                {
                    statusCode = StatusCodes.Status409Conflict;
                    message = ex.Message;
                }
                else if (ex is DbUpdateException)
                {
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "A database update error occurred.";
                }
                else
                {
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "An unexpected error occurred.";
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    StatusCode = statusCode,
                    Message = message
                }));

                HandleException(ex);
            }
        }

        private void HandleException(Exception ex)
        {
            if (ex is NotFoundException notFoundEx)
                _logger.LogWarning(new EventId(1000), notFoundEx, notFoundEx.Message);

            else if (ex is UnauthorizedAccessException unauthorizedEx)
                _logger.LogWarning(new EventId(1001), unauthorizedEx, unauthorizedEx.Message);

            else if (ex is ArgumentException argEx)
                _logger.LogError(new EventId(1002), argEx, argEx.Message);

            else if (ex is InvalidOperationException ioEx)
            {
                _logger.LogError(new EventId(1003), ioEx, "An Invalid Operation Exception occurred."
                   + " This is usually caused by a database call that expects "
                   + "one result, but receives none or more than one.");
            }

            else if (ex is SqlException sqlEx)
                _logger.LogError(new EventId(1004), sqlEx, $"A SQL database exception occurred. Error Number {sqlEx.Number}");

            else if (ex is NullReferenceException nullEx)
                _logger.LogError(new EventId(1005), nullEx, $"A Null Reference Exception occurred. Source: {nullEx.Source}.");

            else if (ex is DbUpdateConcurrencyException dbEx)
            {
                _logger.LogError(new EventId(1006), dbEx, "A database error occurred while trying to update your item." +
                    " This is usually due to someone else modifying the item since you loaded it.");
            }
            else
                _logger.LogError(new EventId(1007), ex, "An unhandled exception has occurred.");
        }
    }
}
