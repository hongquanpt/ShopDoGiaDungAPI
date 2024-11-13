using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Services;
using ShopDoGiaDungAPI.Services.Implementations;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ CORS với chính sách được cập nhật
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowedOrigins", policy =>
    {
        policy.WithOrigins("https://localhost:7007") // Chỉ định các origin bạn cho phép
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Chỉ nếu bạn cần gửi cookie hoặc thông tin xác thực
    });
});

// Cấu hình JWT từ appsettings.json hoặc Environment Variables
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

// Cấu hình Authentication với JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Giảm thiểu thời gian lệch chu kỳ (default là 5 phút)
    };
});

// Cấu hình Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("admin")); // Chỉ cho Admin truy cập
    options.AddPolicy("UserPolicy", policy =>
       policy.RequireRole("user"));
    options.AddPolicy("ModPolicy", policy =>
       policy.RequireRole("mod"));
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout set to 30 minutes
    options.Cookie.HttpOnly = true; // Ensures the session cookie is accessible only via HTTP requests
    options.Cookie.IsEssential = true; // Indicates the session cookie is required for the application
});
builder.Services.AddDistributedMemoryCache();

// Cấu hình Kestrel để sử dụng TLS 1.2 và TLS 1.3
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7248, listenOptions =>
    {
        listenOptions.UseHttps("C:\\Users\\Admin\\Desktop\\ShopDoGiaDungAPI\\ShopDoGiaDungAPI\\https\\localhost.pfx", "quan", httpsOptions =>
        {
            // Chỉ định các phiên bản TLS an toàn
            httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

            // Thiết lập cài đặt OS-level (nếu CipherSuitesPolicy không khả dụng)
            // Đảm bảo hệ điều hành được cấu hình để vô hiệu hóa các cipher suite yếu.
        });
    });
});

// Thêm các dịch vụ khác
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OnlineShopContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDB"));
});

// Đăng ký các service
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMinioService, MinioService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRoleService, RoleService>();

var app = builder.Build();

// Cấu hình pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Áp dụng chính sách CORS
app.UseCors("MyAllowedOrigins");

// Thêm middleware bảo mật bổ sung
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});

// Kích hoạt WebSockets với các tùy chọn bảo mật
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2),
    // Bạn có thể cấu hình thêm các tùy chọn khác nếu cần
});

// Middleware xác thực JWT cho WebSocket
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        // Xác minh tiêu đề Origin
        var origin = context.Request.Headers["Origin"].ToString();
        var allowedOrigins = new List<string> { "https://localhost:7007" };
        if (!allowedOrigins.Contains(origin))
        {
            context.Response.StatusCode = 403; // Forbidden
            return;
        }

        // Lấy token từ query string hoặc header
        string token = context.Request.Query["token"];
        if (string.IsNullOrEmpty(token))
        {
            // Thử lấy từ header Authorization
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer "))
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }
            }
        }

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401; // Unauthorized
            return;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            // Nếu xác thực thành công, tiếp tục xử lý WebSocket
            await next();
        }
        catch
        {
            context.Response.StatusCode = 401; // Unauthorized
            return;
        }
    }
    else
    {
        await next();
    }
});

// Thêm Authentication Middleware
app.UseAuthentication();

app.UseAuthorization();
app.UseSession();

// Xử lý kết nối WebSocket
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        // Thực hiện các thao tác với WebSocket tại đây
        await Echo(context, webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// Ví dụ về hàm Echo cho WebSocket
async Task Echo(HttpContext context, WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    while (!result.CloseStatus.HasValue)
    {
        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }
    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}

app.MapControllers();

app.Run();
