using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace DATN_DT.Repos
{
    public class SanPhamRepo : ISanPhamRepo
    {
        private readonly MyDbContext _context;
        public SanPhamRepo(MyDbContext context)
        {
            _context = context;
        }

        public async Task Create(SanPham sanPham)
        {
            await _context.SanPhams.AddAsync(sanPham);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var sanPham = await GetSanPhamById(id);
            if (sanPham == null) throw new KeyNotFoundException("Sản phẩm không tìm thấy!");
            _context.SanPhams.Remove(sanPham);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SanPham>> GetAllSanPhams()
        {
            return await _context.SanPhams
                .Include(sp => sp.ThuongHieu)
                .ToListAsync();
        }

        public async Task<SanPham?> GetSanPhamById(int id)
        {
            return await _context.SanPhams
                .Include(sp => sp.ThuongHieu)
                .FirstOrDefaultAsync(sp => sp.IdSanPham == id);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(SanPham sanPham)
        {
            // TÌM SẢN PHẨM HIỆN TẠI
            var existingSanPham = await _context.SanPhams.FindAsync(sanPham.IdSanPham);
            if (existingSanPham == null)
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID = {sanPham.IdSanPham}");

            // KIỂM TRA XEM SẢN PHẨM CÓ ĐANG ĐƯỢC THEO DÕI KHÔNG
            var trackedEntity = _context.ChangeTracker.Entries<SanPham>()
                .FirstOrDefault(e => e.Entity.IdSanPham == sanPham.IdSanPham);

            // NẾU KHÔNG ĐƯỢC THEO DÕI, ATTACH VÀO CONTEXT
            if (trackedEntity == null)
            {
                // Detach entity nếu đã tồn tại
                var local = _context.Set<SanPham>()
                    .Local
                    .FirstOrDefault(entry => entry.IdSanPham == sanPham.IdSanPham);

                if (local != null)
                {
                    _context.Entry(local).State = EntityState.Detached;
                }

                // Attach entity mới
                _context.SanPhams.Attach(sanPham);
                _context.Entry(sanPham).State = EntityState.Modified;
            }
            else
            {
                // CẬP NHẬT TỪNG THUỘC TÍNH
                trackedEntity.Entity.MaSanPham = sanPham.MaSanPham;
                trackedEntity.Entity.TenSanPham = sanPham.TenSanPham;
                trackedEntity.Entity.IdThuongHieu = sanPham.IdThuongHieu;
                trackedEntity.Entity.MoTa = sanPham.MoTa;
                trackedEntity.Entity.GiaGoc = sanPham.GiaGoc;
                trackedEntity.Entity.GiaNiemYet = sanPham.GiaNiemYet;
                trackedEntity.Entity.VAT = sanPham.VAT;
                trackedEntity.Entity.TrangThaiSP = sanPham.TrangThaiSP;
            }

            // LƯU THAY ĐỔI
            await _context.SaveChangesAsync();
        }

        // THÊM PHƯƠNG THỨC LẤY DANH SÁCH THƯƠNG HIỆU
        public async Task<List<ThuongHieu>> GetAllThuongHieu()
        {
            return await _context.ThuongHieus
                .Where(th => th.TrangThaiThuongHieu == "Đang hoạt động") // Sửa thành TrangThaiThuongHieu
                .OrderBy(th => th.TenThuongHieu)
                .ToListAsync();
        }
    }
}