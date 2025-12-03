using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace QLCSV.Controllers
{
    /// <summary>
    /// Base controller with common helper methods for all controllers
    /// </summary>
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Get the current authenticated user's ID from JWT token claims
        /// </summary>
        /// <returns>User ID if authenticated, null otherwise</returns>
        protected long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            if (!long.TryParse(userIdClaim, out var userId)) return null;
            return userId;
        }
    }
}
