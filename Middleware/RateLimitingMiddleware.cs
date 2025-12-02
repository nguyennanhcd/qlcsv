using System.Collections.Concurrent;

namespace QLCSV.Middleware
{
    /// <summary>
    /// Simple rate limiting middleware to prevent brute force attacks
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, RequestCounter> _requestCounters = new();
        private static readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(15);
        private const int MaxRequestsPerWindow = 10; // 10 requests per 15 minutes

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only apply rate limiting to authentication endpoints
            var path = context.Request.Path.Value?.ToLower() ?? "";
            
            if (path.Contains("/api/auth/login") || path.Contains("/api/auth/register"))
            {
                var clientId = GetClientIdentifier(context);
                var counter = _requestCounters.GetOrAdd(clientId, _ => new RequestCounter());

                bool rateLimitExceeded = false;
                
                lock (counter)
                {
                    // Clean up old requests outside the time window
                    counter.Requests.RemoveAll(r => DateTime.UtcNow - r > _timeWindow);

                    if (counter.Requests.Count >= MaxRequestsPerWindow)
                    {
                        rateLimitExceeded = true;
                    }
                    else
                    {
                        counter.Requests.Add(DateTime.UtcNow);
                    }
                }

                if (rateLimitExceeded)
                {
                    context.Response.StatusCode = 429; // Too Many Requests
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Quá nhiều yêu cầu. Vui lòng thử lại sau 15 phút.",
                        retryAfter = 900 // seconds
                    });
                    return;
                }

                // Clean up old entries periodically
                CleanupOldEntries();
            }

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Try to get IP address from X-Forwarded-For header (for proxies/load balancers)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Fall back to direct IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private void CleanupOldEntries()
        {
            var keysToRemove = _requestCounters
                .Where(kvp => DateTime.UtcNow - kvp.Value.LastRequest > TimeSpan.FromHours(1))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _requestCounters.TryRemove(key, out _);
            }
        }

        private class RequestCounter
        {
            public List<DateTime> Requests { get; set; } = new();
            public DateTime LastRequest => Requests.LastOrDefault();
        }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
