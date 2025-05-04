using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DATN.Services;

namespace DATN.Authorization
{
    public class UserResourceAuthorizationHandler : AuthorizationHandler<UserResourceRequirement, int>
    {
        private readonly IUserService _userService;

        public UserResourceAuthorizationHandler(IUserService userService)
        {
            _userService = userService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            UserResourceRequirement requirement,
            int targetUserId)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out int currentUserId))
            {
                return;
            }

            var canAccess = await _userService.CanAccessUserDataAsync(currentUserId, targetUserId);
            if (canAccess)
            {
                context.Succeed(requirement);
            }
        }
    }

    public class UserResourceRequirement : IAuthorizationRequirement
    {
        // Placeholder class for the requirement
    }
}