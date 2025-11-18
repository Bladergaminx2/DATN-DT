using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;
using DATN_DT.Repos;
using DATN_DT.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ----------------------
// JWT Key
// ----------------------
var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);



builder.Services.AddHttpClient("ServerApi")
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7150" ?? ""));
                

// ----------------------
// Add services to DI
// ----------------------
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(typeof(DATN_DT.CustomAttribute.AuthorizeRoleFromTokenGlobalFilter));
});

// DbContext
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// ----------------------
// Repos & Services
// ----------------------
// Scoped: dùng DbContext → nên Scoped
builder.Services.AddScoped<ISanPhamRepo, SanPhamRepo>();
builder.Services.AddScoped<ISanPhamService, SanPhamService>();
builder.Services.AddScoped<IThuongHieuRepo, ThuongHieuRepo>();
builder.Services.AddScoped<IThuongHieuService, ThuongHieuService>();
builder.Services.AddScoped<ITonKhoRepo, TonKhoRepo>();
builder.Services.AddScoped<ITonKhoService, TonKhoService>();
builder.Services.AddScoped<IModelSanPhamRepo, ModelSanPhamRepo>();
builder.Services.AddScoped<IModelSanPhamService, ModelSanPhamService>();
builder.Services.AddScoped<IManHinhRepo, ManHinhRepo>();
builder.Services.AddScoped<IManHinhService, ManHinhService>();
builder.Services.AddScoped<INhanVienRepo, NhanVienRepo>();
builder.Services.AddScoped<INhanVienService, NhanVienService>();
builder.Services.AddScoped<IRAMRepo, RAMRepo>();
builder.Services.AddScoped<IRAMService, RAMService>();
builder.Services.AddScoped<IPinRepo, PinRepo>();
builder.Services.AddScoped<IPinService, PinService>();
builder.Services.AddScoped<IROMRepo, ROMRepo>();
builder.Services.AddScoped<IROMService, ROMService>();

// HttpClient cho service gọi API
builder.Services.AddHttpClient();

// ----------------------
// JWT Authentication
// ----------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    // Lấy token từ cookie "jwt"
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.HttpContext.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
                context.Token = token;
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// ----------------------
// Middleware
// ----------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ----------------------
// Seed Role & Admin
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

    // Tạo role ADMIN nếu chưa có
    var roleAdmin = await db.ChucVus.FirstOrDefaultAsync(r => r.TenChucVuVietHoa == "ADMIN");
    if (roleAdmin == null)
    {
        roleAdmin = new ChucVu
        {
            TenChucVu = "Admin",
            TenChucVuVietHoa = "ADMIN"
        };
        db.ChucVus.Add(roleAdmin);
        await db.SaveChangesAsync();
    }

    // Tạo tài khoản admin nếu chưa có
    var admin = await db.NhanViens.FirstOrDefaultAsync(nv => nv.TenTaiKhoanNV == "admin");
    if (admin == null)
    {
        string password = "admin123"; // mật khẩu mặc định
        string hashedPassword;
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            hashedPassword = Convert.ToBase64String(bytes);
        }

        admin = new NhanVien
        {
            TenTaiKhoanNV = "admin",
            Password = hashedPassword,
            HoTenNhanVien = "Tài khoản quản trị",
            IdChucVu = roleAdmin.IdChucVu,
            TrangThaiNV = 1,
            NgayVaoLam = DateTime.Now
        };

        db.NhanViens.Add(admin);
        await db.SaveChangesAsync();
    }
}

// ----------------------
// Routing
// ----------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
