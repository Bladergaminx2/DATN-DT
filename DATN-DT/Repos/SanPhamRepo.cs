using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Repos
{
    public class SanPhamRepo : ISanPhamRepo
    {
        private readonly MyDbContext _context;
        public SanPhamRepo(MyDbContext context)
        {
            _context = context;
        }
        public async Task Create(SanPham sanpham)
        {
            // Không cần check ID vì ID sẽ được tự động tạo bởi database (Identity column)
            // Chỉ cần add entity, EF sẽ tự động tạo ID mới
            await _context.SanPhams.AddAsync(sanpham);
        }

        public async Task Delete(int id)
        {
            var sanpham = await GetSanPhamById(id);
            if (sanpham == null)
            {
               throw new Exception("SanPham not found.");
            }
            _context.SanPhams.Remove(sanpham);
        }

        public async Task<List<SanPham>> GetAllSanPhams()
        {
            return await _context.SanPhams
                .Include(s => s.ThuongHieu)
                .ToListAsync();
        }

        public async Task<SanPham?> GetSanPhamById(int id)
        {
            return await _context.SanPhams.FindAsync(id);
        }

        public async Task SaveChanges()
        {
           await _context.SaveChangesAsync();
        }

        public async Task Update(SanPham sanpham)
        {
            var existing = await GetSanPhamById(sanpham.IdSanPham);
            if (existing == null)
            {
                throw new Exception("SanPham not found.");
            }
            
            // Cập nhật các thuộc tính từ entity mới vào entity đã tồn tại
            existing.MaSanPham = sanpham.MaSanPham;
            existing.TenSanPham = sanpham.TenSanPham;
            existing.IdThuongHieu = sanpham.IdThuongHieu;
            existing.MoTa = sanpham.MoTa;
            existing.GiaGoc = sanpham.GiaGoc;
            existing.GiaNiemYet = sanpham.GiaNiemYet;
            existing.VAT = sanpham.VAT;
            existing.TrangThaiSP = sanpham.TrangThaiSP;
            
            _context.SanPhams.Update(existing);
        }
    }
}       
