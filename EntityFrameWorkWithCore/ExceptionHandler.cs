using Microsoft.AspNetCore.Diagnostics;

namespace EntityFrameWorkWithCore
{
    public class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if(exception is not NotImplementedException)
            {
                logger.LogError("something wrong");

                var sss = new
                {
                    title = "something went wrong",
                    success = false,
                    errorMessage = exception.Message,
                    statusCode = StatusCodes.Status500InternalServerError,
                };
                await httpContext.Response.WriteAsJsonAsync(sss, cancellationToken);
                /*httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;*/

                return true;
            }
            return false;
        }
    }
}
