using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Repos
{
    public class ThuongHieuRepo : IThuongHieuRepo
    {
        private readonly MyDbContext _context;

        public ThuongHieuRepo(MyDbContext context)
        {
            _context = context;
        }

        public async Task Create(ThuongHieu thuongHieu)
        {
            // Kiểm tra trùng tên thương hiệu thay vì ID
            var existing = await _context.ThuongHieus
                .FirstOrDefaultAsync(x => x.TenThuongHieu.ToLower() == thuongHieu.TenThuongHieu.ToLower());

            if (existing != null)
                throw new ArgumentException("Thương hiệu đã tồn tại");

            await _context.ThuongHieus.AddAsync(thuongHieu);
            await _context.SaveChangesAsync(); // Nên save changes ở repo
        }

        public async Task Delete(int id)
        {
            var thuongHieu = await GetThuongHieuById(id);
            if (thuongHieu == null)
                throw new KeyNotFoundException("Không tìm thấy thương hiệu");

            _context.ThuongHieus.Remove(thuongHieu);
            await _context.SaveChangesAsync(); // Nên save changes ở repo
        }

        public async Task<List<ThuongHieu>> GetAllThuongHieus()
        {
            return await _context.ThuongHieus.ToListAsync();
        }

        public async Task<ThuongHieu?> GetThuongHieuById(int id)
        {
            // Sửa lỗi: thiếu FirstOrDefaultAsync
            return await _context.ThuongHieus
                .FirstOrDefaultAsync(x => x.IdThuongHieu == id);
        }

        public async Task<ThuongHieu?> GetThuongHieuByName(string name)
        {
            return await _context.ThuongHieus
                .FirstOrDefaultAsync(x => x.TenThuongHieu.ToLower() == name.ToLower());
        }

        public async Task Update(ThuongHieu thuongHieu)
        {
            var existing = await GetThuongHieuById(thuongHieu.IdThuongHieu);
            if (existing == null)
                throw new KeyNotFoundException("Không tìm thấy thương hiệu");

            // Kiểm tra trùng tên (trừ chính nó)
            var duplicate = await _context.ThuongHieus
                .FirstOrDefaultAsync(x =>
                    x.TenThuongHieu.ToLower() == thuongHieu.TenThuongHieu.ToLower() &&
                    x.IdThuongHieu != thuongHieu.IdThuongHieu);

            if (duplicate != null)
                throw new ArgumentException("Tên thương hiệu đã tồn tại");

            // Cập nhật thông tin
            existing.TenThuongHieu = thuongHieu.TenThuongHieu;
            existing.TrangThaiThuongHieu = thuongHieu.TrangThaiThuongHieu;
            // Có thể thêm các trường khác nếu có

            _context.ThuongHieus.Update(existing);
            await _context.SaveChangesAsync(); // Nên save changes ở repo
        }
    }
}