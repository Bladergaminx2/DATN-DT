using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;
using DATN_DT.Repos;
using DATN_DT.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DATN_DT.Services.Ghn;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ================= JWT KEY =================
var jwtKey = builder.Configuration["Jwt:Key"];
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

// ================= MVC + GLOBAL FILTER =================
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(typeof(DATN_DT.CustomAttribute.AuthorizeRoleFromTokenGlobalFilter));
})
.AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// ================= DB =================
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ================= DI =================
builder.Services.AddScoped<ISanPhamRepo, SanPhamRepo>();
builder.Services.AddScoped<ISanPhamService, SanPhamService>();

builder.Services.AddScoped<IRAMRepo, RAMRepo>();
builder.Services.AddScoped<IRAMService, RAMService>();

builder.Services.AddScoped<IROMRepo, ROMRepo>();
builder.Services.AddScoped<IROMService, ROMService>();

builder.Services.AddScoped<IManHinhRepo, ManHinhRepo>();
builder.Services.AddScoped<IManHinhService, ManHinhService>();

builder.Services.AddScoped<IThuongHieuRepo, ThuongHieuRepo>();
builder.Services.AddScoped<IThuongHieuService, ThuongHieuService>();

builder.Services.AddScoped<ITonKhoRepo, TonKhoRepo>();
builder.Services.AddScoped<ITonKhoService, TonKhoService>();

builder.Services.AddScoped<IKhoService, KhoService>();

builder.Services.AddScoped<INhanVienRepo, NhanVienRepo>();
builder.Services.AddScoped<INhanVienService, NhanVienService>();

builder.Services.AddScoped<IPinRepo, PinRepo>();
builder.Services.AddScoped<IPinService, PinService>();

<<<<<<< HEAD
builder.Services.AddScoped<IVoucherService, VoucherService>();
=======
builder.Services.AddScoped<IChucVuRepo, ChucVuRepo>();
builder.Services.AddScoped<IChucVuService, ChucVuService>();
>>>>>>> 8a06bd24b7e4eb70a8ecf4b1ab55a0a8daab1fe2

builder.Services.Configure<GhnOptions>(builder.Configuration.GetSection("GHN"));
builder.Services.AddHttpClient<IGhnClient, GhnClient>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// ================= JWT AUTH =================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero, // ⏱ không cho lệch giờ

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };

    // 🔐 Đọc JWT từ cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.HttpContext.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// ================= PIPELINE =================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // 🔐 BẮT BUỘC
app.UseAuthorization();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

    // ========== 1. TẠO ROLE ADMIN ==========
    var roleAdmin = db.ChucVus.FirstOrDefault(r => r.TenChucVuVietHoa == "ADMIN");

    if (roleAdmin == null)
    {
        roleAdmin = new ChucVu
        {
            TenChucVu = "Admin",
            TenChucVuVietHoa = "ADMIN"
        };
        db.ChucVus.Add(roleAdmin);
        db.SaveChanges();
    }

    // ========== 2. TẠO TÀI KHOẢN ADMIN ==========
    var admin = db.NhanViens.FirstOrDefault(nv => nv.TenTaiKhoanNV == "admin");

    if (admin == null)
    {
        string password = "admin123";  // mật khẩu mặc định (nên đổi sau)
        string hashedPassword;

        // hash SHA256
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
        db.SaveChanges();
    }
}


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);

app.Run();
