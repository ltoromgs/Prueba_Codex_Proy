using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

public class SingleSessionMiddleware
{
    private readonly RequestDelegate _next;

    public SingleSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ISessionTrackerService sessionTracker)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var username = context.User.Identity.Name;
            var sessionClaim = context.User.FindFirst("SessionId")?.Value;

            var activeSession = await sessionTracker.GetSessionForUser(username);

            if (activeSession != sessionClaim)
            {
                await context.SignOutAsync();
                context.Response.Redirect("/Account/Login?reason=sesion_expirada");
                return;
            }
        }

        await _next(context);
    }
}