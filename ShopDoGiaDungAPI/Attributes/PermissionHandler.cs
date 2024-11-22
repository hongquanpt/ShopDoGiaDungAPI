using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace ShopDoGiaDungAPI.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionHandler> _logger;

        public PermissionHandler(ILogger<PermissionHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                _logger.LogWarning("User is null.");
                return Task.CompletedTask;
            }

            // Lấy các claims về permissions
            var permissions = context.User.Claims
                .Where(c => c.Type == "permissions")
                .Select(c => c.Value)
                .ToList();

            _logger.LogInformation($"User Permissions: {string.Join(", ", permissions)}");
            _logger.LogInformation($"Required Permission: {requirement.FunctionCode}:{requirement.ActionCode}");

            // Kiểm tra xem user có quyền tương ứng không
            var requiredPermission = $"{requirement.FunctionCode}:{requirement.ActionCode}";
            if (permissions.Contains(requiredPermission))
            {
                _logger.LogInformation("Permission granted.");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("Permission denied.");
            }

            return Task.CompletedTask;
        }
    }
}
