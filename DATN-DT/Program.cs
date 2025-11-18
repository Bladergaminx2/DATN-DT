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
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddHttpClient(); // ← Add this line
// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Gắn filter toàn cục
    options.Filters.Add(typeof(DATN_DT.CustomAttribute.AuthorizeRoleFromTokenGlobalFilter));
});


builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Thêm các dịch vụ vào DI container
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

});
builder.Services.AddScoped<ISanPhamService, SanPhamService>();
builder.Services.AddScoped<ISanPhamRepo, SanPhamRepo>();
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

    // Đọc token từ cookie "jwt"
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

// Các service khác...
builder.Services.AddControllersWithViews();

// Đăng ký IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();  // 🔐 thêm dòng này
app.UseAuthorization();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

//    // ========== 1. TẠO ROLE ADMIN ==========
//    var roleAdmin = db.ChucVus.FirstOrDefault(r => r.TenChucVuVietHoa == "ADMIN");

//    if (roleAdmin == null)
//    {
//        roleAdmin = new ChucVu
//        {
//            TenChucVu = "Admin",
//            TenChucVuVietHoa = "ADMIN"
//        };
//        db.ChucVus.Add(roleAdmin);
//        db.SaveChanges();
//    }

//    // ========== 2. TẠO TÀI KHOẢN ADMIN ==========
//    var admin = db.NhanViens.FirstOrDefault(nv => nv.TenTaiKhoanNV == "admin");

//    if (admin == null)
//    {
//        string password = "admin123";  // mật khẩu mặc định (nên đổi sau)
//        string hashedPassword;

//        // hash SHA256
//        using (var sha = System.Security.Cryptography.SHA256.Create())
//        {
//            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
//            hashedPassword = Convert.ToBase64String(bytes);
//        }

//        admin = new NhanVien
//        {
//            TenTaiKhoanNV = "admin",
//            Password = hashedPassword,
//            HoTenNhanVien = "Tài khoản quản trị",
//            IdChucVu = roleAdmin.IdChucVu,
//            TrangThaiNV = 1,
//            NgayVaoLam = DateTime.Now
//        };

//        db.NhanViens.Add(admin);
//        db.SaveChanges();
//    }
//}

app.MapControllerRoute(
    name: "login",
    pattern: "{controller=Login}/{action=Index}");


app.Run();
