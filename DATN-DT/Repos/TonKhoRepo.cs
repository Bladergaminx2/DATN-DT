using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Repos
{
    public class TonKhoRepo : ITonKhoRepo
    {
        private readonly MyDbContext _context;
        public TonKhoRepo(MyDbContext context)
        {
            _context = context;
        }
        public async Task Create(TonKho tonKho)
        {
            // Tạo mới không cần tracking
            var newTonKho = new TonKho
            {
                IdModelSanPham = tonKho.IdModelSanPham,
                IdKho = tonKho.IdKho,
                SoLuong = tonKho.SoLuong
            };

            await _context.TonKhos.AddAsync(newTonKho);
            await _context.SaveChangesAsync();
        }

        public async Task Update(TonKho tonKho)
        {
            // Cách 1: Tìm entity hiện có và cập nhật
            var existing = await _context.TonKhos
                .FirstOrDefaultAsync(tk => tk.IdTonKho == tonKho.IdTonKho);

            if (existing != null)
            {
                existing.IdModelSanPham = tonKho.IdModelSanPham;
                existing.IdKho = tonKho.IdKho;
                existing.SoLuong = tonKho.SoLuong;

                _context.TonKhos.Update(existing);
                await _context.SaveChangesAsync();
            }

            // Hoặc Cách 2: Attach và mark as modified
            // _context.TonKhos.Attach(tonKho);
            // _context.Entry(tonKho).State = EntityState.Modified;
            // await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var tonKho = await _context.TonKhos
                .FirstOrDefaultAsync(tk => tk.IdTonKho == id);

            if (tonKho != null)
            {
                _context.TonKhos.Remove(tonKho);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<TonKho>> GetAllTonKhos()
        {
            return await _context.TonKhos
                .Include(tk => tk.ModelSanPham)
                .Include(tk => tk.Kho)
                .AsNoTracking() // Thêm AsNoTracking để tránh tracking conflict
                .ToListAsync();
        }

        public async Task<List<ModelSanPham>> GetModelSanPhams()
        {
            return await _context.ModelSanPhams.ToListAsync();
        }

        public async Task<bool> CheckDuplicate(int? idModelSanPham, int? idKho, int excludeId)
        {
            return await _context.TonKhos
                .AnyAsync(tk => tk.IdModelSanPham == idModelSanPham
                             && tk.IdKho == idKho
                             && tk.IdTonKho != excludeId);
        }

        public async Task<List<Kho>> GetKhos()
        {
           return await _context.Khos.ToListAsync();
        }

        public async Task<TonKho?> GetTonKhoById(int id)
        {
            return await _context.TonKhos
                .Include(tk => tk.ModelSanPham)
                .Include(tk => tk.Kho)
                .AsNoTracking() // Thêm AsNoTracking
                .FirstOrDefaultAsync(tk => tk.IdTonKho == id);
        }
    }
}
