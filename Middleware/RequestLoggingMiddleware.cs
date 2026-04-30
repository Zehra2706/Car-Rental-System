using car.Data;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;


public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ApplicationDbContext db)
    {
        await _next(context);

        if (context.Request.Method == "POST")
        {
            var path = context.Request.Path.Value.ToLower();

            if (path.Contains("login") ||
                path.Contains("register") ||
                path.Contains("payment") ||
                path.Contains("token"))
                return;
            // 1. Önce Identity'den ismi almayı dene
            string userName = context.User.Identity?.Name;

            // 2. Eğer boşsa (Login anındaysak veya JWT maplenemediyse) Session'dan al
            if (string.IsNullOrEmpty(userName))
            {
                userName = context.Session.GetString("UserName");
            }

            // 3. Hala boşsa (Kullanıcı gerçekten giriş yapmamışsa) Anonim de
            userName ??= "Anonim";

            var log = new Log
            {
                Username = userName, // Artık Session'dan "Melisa" vb. gelecek
                Action = context.GetEndpoint()?.DisplayName ?? "Unknown",
                Path = context.Request.Path.Value,
                Method = context.Request.Method,
                IPAddress = context.Connection.RemoteIpAddress?.ToString(),
                StatusCode = context.Response.StatusCode,
                Date = DateTime.Now
            };

            db.Logs.Add(log);
            await db.SaveChangesAsync();
        }
    }
}