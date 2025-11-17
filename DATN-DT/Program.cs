using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using DATN_DT.Models;
using DATN_DT.Data;


var builder = WebApplication.CreateBuilder(args);
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Gắn filter toàn cục
    options.Filters.Add(typeof(DATN_DT.CustomAttribute.AuthorizeRoleFromTokenGlobalFilter));
});


builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



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
    name: "login",
    pattern: "{controller=Login}/{action=Index}");


app.Run();
