using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Repos
{
    public class NhanVienRepo : INhanVienRepo
    {
        private readonly MyDbContext _context;

        public NhanVienRepo(MyDbContext context)
        {
            _context = context;
        }

        public async Task Create(NhanVien nhanvien)
        {
            await _context.NhanViens.AddAsync(nhanvien);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien != null)
            {
                _context.NhanViens.Remove(nhanVien);
                await _context.SaveChangesAsync();
            }
        }

        // Lấy tất cả nhân viên KHÔNG phải Admin
        public async Task<List<NhanVien>> GetAllNhanViens()
        {
            return await _context.NhanViens
                .Include(nv => nv.ChucVu)
                .Where(nv => nv.ChucVu.TenChucVuVietHoa != "ADMIN" && nv.ChucVu.TenChucVu != "Admin")
                .AsNoTracking()
                .ToListAsync();
        }

        // Lấy tất cả nhân viên (bao gồm cả Admin) - nếu cần
        public async Task<List<NhanVien>> GetAllNhanViensIncludingAdmin()
        {
            return await _context.NhanViens
                .Include(nv => nv.ChucVu)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<NhanVien?> GetNhanVienById(int id)
        {
            return await _context.NhanViens
                .Include(nv => nv.ChucVu)
                .AsNoTracking()
                .FirstOrDefaultAsync(nv => nv.IdNhanVien == id);
        }

        public async Task<List<ChucVu>> GetChucVus()
        {
            return await _context.ChucVus
                .AsNoTracking()
                .ToListAsync();
        }

        // Lọc chức vụ trừ Admin
        public async Task<List<ChucVu>> GetChucVusExceptAdmin()
        {
            return await _context.ChucVus
                .Where(cv => cv.TenChucVuVietHoa != "ADMIN" && cv.TenChucVu != "Admin")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task Update(NhanVien nhanvien)
        {
            var existing = await _context.NhanViens
                .FirstOrDefaultAsync(nv => nv.IdNhanVien == nhanvien.IdNhanVien);

            if (existing != null)
            {
                existing.TenTaiKhoanNV = nhanvien.TenTaiKhoanNV;
                existing.HoTenNhanVien = nhanvien.HoTenNhanVien;
                existing.IdChucVu = nhanvien.IdChucVu;
                existing.SdtNhanVien = nhanvien.SdtNhanVien;
                existing.EmailNhanVien = nhanvien.EmailNhanVien;
                existing.DiaChiNV = nhanvien.DiaChiNV;
                existing.NgayVaoLam = nhanvien.NgayVaoLam;
                existing.TrangThaiNV = nhanvien.TrangThaiNV;

                // Chỉ cập nhật mật khẩu nếu có giá trị
                if (!string.IsNullOrEmpty(nhanvien.Password))
                {
                    existing.Password = nhanvien.Password;
                }

                await _context.SaveChangesAsync();
            }
        }

        // Kiểm tra xem chức vụ có phải Admin không
        public async Task<bool> IsAdminRole(int idChucVu)
        {
            var chucVu = await _context.ChucVus
                .FirstOrDefaultAsync(cv => cv.IdChucVu == idChucVu);

            return chucVu != null &&
                   (chucVu.TenChucVuVietHoa == "ADMIN" || chucVu.TenChucVu == "Admin");
        }
    }
}