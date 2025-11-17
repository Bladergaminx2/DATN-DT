using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
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
            if (await GetTonKhoById(tonKho.IdTonKho) != null) throw new ArgumentException($"TonKho {tonKho.IdTonKho} already exists");
            await _context.TonKhos.AddAsync(tonKho);
        }

        public async Task Delete(int id)
        {
            var tonKho = await GetTonKhoById(id);
            if (tonKho == null) throw new KeyNotFoundException("TonKho is not found");
            _context.TonKhos.Remove(tonKho);
        }

        public async Task<List<TonKho>> GetAllTonKhos()
        {
            return await _context.TonKhos.ToListAsync();
        }

        public async Task<TonKho?> GetTonKhoById(int id)
        {
            return await _context.TonKhos.FindAsync(id);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(TonKho tonKho)
        {
            if (await GetTonKhoById(tonKho.IdTonKho) == null) throw new KeyNotFoundException("Kho is not found");
            _context.Entry(tonKho).State = EntityState.Modified;
        }
    }
}
