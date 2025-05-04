using DATN.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DATN.Middleware
{
    public class RelationshipAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RelationshipAuthorizationMiddleware> _logger;

        public RelationshipAuthorizationMiddleware(RequestDelegate next, ILogger<RelationshipAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, StrokeDbContext dbContext)
        {
           
            if (context.Request.Path.StartsWithSegments("/api/User/users") ||
                context.Request.Path.StartsWithSegments("/api/doctor/patient"))
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    await _next(context);
                    return;
                }

               
                var currentUserIdString = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out int currentUserId))
                {
                    await _next(context);
                    return;
                }

                
                var routePath = context.Request.Path.Value;
                var segments = routePath.Split('/');
                if (segments.Length < 4)
                {
                    await _next(context);
                    return;
                }

                if (!int.TryParse(segments[4], out int targetUserId))
                {
                    await _next(context);
                    return;
                }

                if (currentUserId == targetUserId)
                {
                    await _next(context);
                    return;
                }

                if (context.User.IsInRole("admin"))
                {
                    await _next(context);
                    return;
                }

                
                if (context.User.IsInRole("doctor"))
                {
                    var isPatient = await dbContext.UserRoles
                        .AnyAsync(ur => ur.UserId == targetUserId && ur.Role.RoleName == "user" && ur.IsActive);

                    if (!isPatient)
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsJsonAsync(new { message = "You can only view information of your patients" });
                        return;
                    }

                    var hasRelationship = await dbContext.Relationships
                        .AnyAsync(r => r.InviterId == currentUserId &&
                                      r.UserId == targetUserId &&
                                      r.RelationshipType == "doctor-patient");

                    if (!hasRelationship)
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsJsonAsync(new { message = "This patient is not connected to you" });
                        return;
                    }
                }
                else
                {
                    
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { message = "You don't have permission to view this user's information" });
                    return;
                }
            }

            await _next(context);
        }
    }

 
    public static class RelationshipAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseRelationshipAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RelationshipAuthorizationMiddleware>();
        }
    }
}