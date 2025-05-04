using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace DATN.Middleware
{
    public class AccessLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AccessLogMiddleware> _logger;

        public AccessLogMiddleware(RequestDelegate next, ILogger<AccessLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint()?.DisplayName;
            var path = context.Request.Path;

          
            if (path.StartsWithSegments("/api/admin/users") ||
                path.StartsWithSegments("/api/doctor/patients"))
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var roles = string.Join(",", context.User.FindAll(ClaimTypes.Role).Select(c => c.Value));
                var method = context.Request.Method;
                var query = context.Request.QueryString.ToString();

                _logger.LogInformation(
                    "User {UserId} with roles {Roles} {Method} request to {Path}{Query} at {Time}",
                    userId,
                    roles,
                    method,
                    path,
                    query,
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                );
            }

            await _next(context);
        }
    }

   
    public static class AccessLogMiddlewareExtensions
    {
        public static IApplicationBuilder UseAccessLog(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AccessLogMiddleware>();
        }
    }
}