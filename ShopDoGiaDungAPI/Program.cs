using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopDoGiaDungAPI.Authorization;
using ShopDoGiaDungAPI.Data;
using ShopDoGiaDungAPI.Services;
using ShopDoGiaDungAPI.Services.Implementations;
using ShopDoGiaDungAPI.Services.Interfaces;
using System.Text;

// Tạo builder
var builder = WebApplication.CreateBuilder(args);

// Cấu hình CORS với chính sách cụ thể
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowedOrigins", policy =>
    {
        policy.WithOrigins("https://localhost:7007", "https://10.0.2.2:7248", "http://192.168.1.40:8081", "http://10.0.2.2:5222") // Chỉ định các origin bạn cho phép
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10)); ;
    });
});

// Cấu hình JWT từ appsettings.json hoặc biến môi trường
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

// Cấu hình Authentication với JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true; // Đặt thành true trong môi trường production
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

// Đăng ký Authorization Policies dựa trên chức năng và hành động
builder.Services.AddAuthorization(options =>
{
   

    // Chức năng QuanLyTaiKhoan
    options.AddPolicy("QuanLyTaiKhoan.Xem", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyTaiKhoan", "Xem")));
    options.AddPolicy("QuanLyTaiKhoan.Sua", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyTaiKhoan", "Sua")));
    options.AddPolicy("QuanLyTaiKhoan.Xoa", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyTaiKhoan", "Xoa")));
    options.AddPolicy("QuanLyTaiKhoan.Them", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyTaiKhoan", "Them")));

    // Chức năng QuanLyHang
    options.AddPolicy("QuanLyHang.Them", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyHang", "Them")));
    options.AddPolicy("QuanLyHang.Sua", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyHang", "Sua")));
    options.AddPolicy("QuanLyHang.Xoa", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyHang", "Xoa")));
    options.AddPolicy("QuanLyHang.Xem", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyHang", "Xem")));

    // Chức năng QuanLyChucVu
    options.AddPolicy("QuanLyChucVu.Xem", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyChucVu", "Xem")));
    options.AddPolicy("QuanLyChucVu.Them", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyChucVu", "Them")));
    options.AddPolicy("QuanLyChucVu.Xoa", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyChucVu", "Xoa")));
    options.AddPolicy("QuanLyChucVu.Sua", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyChucVu", "Sua")));
    

    // Chức năng QuanLyDanhMuc
    options.AddPolicy("QuanLyDanhMuc.Them", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyDanhMuc", "Them")));
    options.AddPolicy("QuanLyDanhMuc.Sua", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyDanhMuc", "Sua")));
    options.AddPolicy("QuanLyDanhMuc.Xoa", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyDanhMuc", "Xoa")));

    // Chức năng DonHang
    options.AddPolicy("DonHang.Xem", policy =>
        policy.Requirements.Add(new PermissionRequirement("DonHang", "Xem")));
    options.AddPolicy("DonHang.Sua", policy =>
        policy.Requirements.Add(new PermissionRequirement("DonHang", "Sua")));

    // Chức năng TaiKhoan
    options.AddPolicy("TaiKhoan.Sua", policy =>
        policy.Requirements.Add(new PermissionRequirement("TaiKhoan", "Sua")));

    // Chức năng QuanLyDonHang
    options.AddPolicy("QuanLyDonHang.Xem", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyDonHang", "Xem")));
    options.AddPolicy("QuanLyDonHang.Sua", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLyDonHang", "Sua")));

    // Chức năng QuanLySanPham
    options.AddPolicy("QuanLySanPham.Xem", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLySanPham", "Xem")));
    options.AddPolicy("QuanLySanPham.Them", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLySanPham", "Them")));
    options.AddPolicy("QuanLySanPham.Xoa", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLySanPham", "Xoa")));
    options.AddPolicy("QuanLySanPham.Sua", policy =>
        policy.Requirements.Add(new PermissionRequirement("QuanLySanPham", "Sua")));

    // Chức năng ThongKe
    options.AddPolicy("ThongKe.Xem", policy =>
        policy.Requirements.Add(new PermissionRequirement("ThongKe", "Xem")));
    // Chức năng Access
    options.AddPolicy("Access.Xem", policy =>
       policy.Requirements.Add(new PermissionRequirement("Access", "Xem")));
    options.AddPolicy("Access.Sua", policy =>
       policy.Requirements.Add(new PermissionRequirement("Access", "Sua")));
    options.AddPolicy("Access.Xoa", policy =>
       policy.Requirements.Add(new PermissionRequirement("Access", "Xoa")));

});

// Đăng ký PermissionHandler và PermissionService
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

// Đăng ký các dịch vụ khác
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddDistributedMemoryCache();

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
//builder.Services.AddScoped<IMinioService, AwsS3Service>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITaiKhoanService, TaiKhoanService>();
builder.Services.AddScoped<IChucVuService, ChucVuService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IOrderNotificationService, OrderNotificationService>();

var mongoSettings = builder.Configuration.GetSection("MongoSettings");
string connectionString = mongoSettings["ConnectionString"];
string databaseName = mongoSettings["DatabaseName"];
builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    return new MongoDbContext(connectionString, databaseName);
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Information);  // Cấu hình log cho SignalR

// Cấu hình SignalR
builder.Services.AddSignalR();
// Xây dựng ứng dụng
var app = builder.Build();


// Cấu hình pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Sử dụng HTTPS
app.UseHttpsRedirection();

// Áp dụng chính sách CORS
app.UseCors("MyAllowedOrigins");
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();

    if (origin == "https://bogus.origin.hcl.com")
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden; 
        await context.Response.WriteAsync("CORS policy does not allow this origin.");
        return;
    }

    await next();
});

// Sử dụng Session
app.UseSession();

// Sử dụng Authentication và Authorization
app.UseAuthentication();
app.UseAuthorization();

// Thêm các header bảo mật bổ sung
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self';");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    await next();
});

app.MapHub<ChatHub>("/chathub");
app.MapHub<OrderHub>("/orderHub");


// Map các controller
app.MapControllers();

// Chạy ứng dụng
app.Run();
