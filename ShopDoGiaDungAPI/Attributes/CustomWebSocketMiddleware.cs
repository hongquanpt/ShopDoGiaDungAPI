using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

public class CustomWebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string[] _allowedOrigins = { "https://localhost:7007" };

    public CustomWebSocketMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var origin = context.Request.Headers["Origin"].ToString();
            if (!_allowedOrigins.Contains(origin))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: Origin not allowed.");
                return;
            }
        }
        await _next(context);
    }
}
