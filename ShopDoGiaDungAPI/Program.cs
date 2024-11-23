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
        policy.WithOrigins("https://localhost:7007") // Chỉ định các origin bạn cho phép
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
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
    // Chức năng Access
    options.AddPolicy("Access.Xem", policy =>
       policy.Requirements.Add(new PermissionRequirement("Access", "Xem")));

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

    // Chức năng GioHang
    options.AddPolicy("GioHang.Xem", policy =>
        policy.Requirements.Add(new PermissionRequirement("GioHang", "Xem")));
    options.AddPolicy("GioHang.Them", policy =>
        policy.Requirements.Add(new PermissionRequirement("GioHang", "Them")));
    options.AddPolicy("GioHang.Xoa", policy =>
        policy.Requirements.Add(new PermissionRequirement("GioHang", "Xoa")));
    options.AddPolicy("GioHang.Sua", policy =>
        policy.Requirements.Add(new PermissionRequirement("GioHang", "Sua")));
    

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
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITaiKhoanService, TaiKhoanService>();
builder.Services.AddScoped<IChucVuService, ChucVuService>();

builder.Services.AddSingleton<ILogService>(sp =>
{
    // Sử dụng cấu hình từ appsettings.json
    var mongoSettings = builder.Configuration.GetSection("MongoDB");
    string mongoConnectionString = mongoSettings["ConnectionString"];
    string databaseName = mongoSettings["DatabaseName"];
    string collectionName = mongoSettings["CollectionName"];

    return new LogService(mongoConnectionString, databaseName, collectionName);
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

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

// Map các controller
app.MapControllers();

// Chạy ứng dụng
app.Run();
