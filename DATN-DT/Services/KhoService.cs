using DATN_DT.Data;
using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Services
{
    public class KhoService : IKhoService
    {
        private readonly MyDbContext _context;

        public KhoService(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Kho>> GetAllKhos()
        {
            return await _context.Khos
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Kho?> GetKhoById(int id)
        {
            return await _context.Khos
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.IdKho == id);
        }

        public async Task<bool> CheckDuplicate(string tenKho, int excludeId)
        {
            return await _context.Khos
                .AnyAsync(k => k.TenKho.ToLower() == tenKho.ToLower()
                            && k.IdKho != excludeId);
        }

        public async Task Create(Kho kho)
        {
            var newKho = new Kho
            {
                TenKho = kho.TenKho.Trim(),
                DiaChiKho = kho.DiaChiKho.Trim(),
            };

            await _context.Khos.AddAsync(newKho);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Kho kho)
        {
            var existing = await _context.Khos
                .FirstOrDefaultAsync(k => k.IdKho == kho.IdKho);

            if (existing != null)
            {
                existing.TenKho = kho.TenKho.Trim();
                existing.DiaChiKho = kho.DiaChiKho.Trim();

                await _context.SaveChangesAsync();
            }
        }

        public async Task Delete(int id)
        {
            var kho = await _context.Khos
                .FirstOrDefaultAsync(k => k.IdKho == id);

            if (kho != null)
            {
                _context.Khos.Remove(kho);
                await _context.SaveChangesAsync();
            }
        }
    }
}