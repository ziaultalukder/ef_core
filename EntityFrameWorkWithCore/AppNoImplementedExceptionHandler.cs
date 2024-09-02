using Microsoft.AspNetCore.Diagnostics;

namespace EntityFrameWorkWithCore
{
    public class AppNoImplementedExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is NotImplementedException)
            {
                logger.LogError("something wrong");

                var sss = new
                {
                    title = "something went wrong",
                    errorMessage = exception.Message,
                    statusCode = StatusCodes.Status501NotImplemented,
                };
                await httpContext.Response.WriteAsJsonAsync(sss, cancellationToken);

                return true;
            }
            return false;
        }
    }
}
