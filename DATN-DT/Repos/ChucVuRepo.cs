using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Repos
{
    public class ChucVuRepo : IChucVuRepo
    {
        private readonly MyDbContext _context;
        public ChucVuRepo(MyDbContext context)
        {
            _context = context;
        }
        public async Task Create(ChucVu chucVu)
        {
            // Kiểm tra null trước khi so sánh
            if (string.IsNullOrWhiteSpace(chucVu.TenChucVu) || string.IsNullOrWhiteSpace(chucVu.TenChucVuVietHoa))
                throw new ArgumentException("Tên chức vụ và tên viết hoa không được để trống!");

            // Kiểm tra trùng tên chức vụ
            var tenChucVuLower = chucVu.TenChucVu.Trim().ToLower();
            var tenChucVuVietHoaUpper = chucVu.TenChucVuVietHoa.Trim().ToUpper();

            var existing = await _context.ChucVus
                .FirstOrDefaultAsync(cv => 
                    (cv.TenChucVu != null && cv.TenChucVu.Trim().ToLower() == tenChucVuLower) ||
                    (cv.TenChucVuVietHoa != null && cv.TenChucVuVietHoa.Trim().ToUpper() == tenChucVuVietHoaUpper));
            
            if (existing != null) 
                throw new ArgumentException($"Chức vụ đã tồn tại!");
            
            await _context.ChucVus.AddAsync(chucVu);
            _context.SaveChanges();
        }

        public async Task Delete(int id)
        {
            var chucVu = await GetChucVuById(id);
            if (chucVu == null) throw new KeyNotFoundException("Chức vụ không tìm thấy");
            _context.ChucVus.Remove(chucVu);
            _context.SaveChanges();
        }

        public async Task<List<ChucVu>> GetAllChucVus()
        {
            return await _context.ChucVus.ToListAsync();
        }

        public async Task<ChucVu?> GetChucVuById(int id)
        {
            return await _context.ChucVus.FindAsync(id);
        }

        public async Task Update(ChucVu chucVu)
        {
            _context.ChucVus.Update(chucVu);
            _context.SaveChanges();
        }
    }
}

