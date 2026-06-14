using Hangfire.Dashboard;

namespace AlNady.API.Filters;

/// <summary>
/// Restricts Hangfire dashboard to authenticated Admin users.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
               && httpContext.User.IsInRole("Admin");
    }
}
