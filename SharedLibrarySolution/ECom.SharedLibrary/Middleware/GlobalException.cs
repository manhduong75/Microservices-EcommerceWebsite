using System.Net;
using System.Text.Json;
using ECom.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace ECom.SharedLibrary.Middleware
{
    public class GlobalException(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
           string message = "Sorry, internal server error occurred. Kindly try again";
           int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";

            try
            {
                await next(context);
                // Check if Response is too many requests // 429 status code
                if(context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = "Too many requests made";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    await ModifyHeader(context, title, message, statusCode);
                }

                // If Response is Unauthorized // 401 status code

                if(context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Warning";
                    message = "You are not authorized to access";
                    statusCode = (int)StatusCodes.Status401Unauthorized;
                    await ModifyHeader(context, title, message, statusCode);
                }

                // If Response is Forbidden // 403 status code
                if(context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Out of Access";
                    message = "You are not allowed/required to access";
                    statusCode = (int)StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message, statusCode);
                }
            }
            catch(Exception ex)
            {
                // Log Original Exceptios /File, Debugger, Console
                LogException.LogExceptions(ex);

                // check if Exception is Timeout
                if(ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = "Out of time";
                    message = "Request Timeout";
                    statusCode = StatusCodes.Status408RequestTimeout;
                }
                
                // If Exception is Caught
                // If none of the exceptions above, then do the default
                await ModifyHeader(context, title, message, statusCode);

            }

        }

        private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            //display scary-free message to client
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
            {
                Detail = message,
                Title = title,
                Status = statusCode
            }), CancellationToken.None);
            return;
        }
    }
}
