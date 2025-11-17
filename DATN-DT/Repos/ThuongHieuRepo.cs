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
            if (await GetThuongHieuById(thuongHieu.IdThuongHieu) != null) throw new ArgumentException("ThuongHieu already exists");
            await _context.ThuongHieus.AddAsync(thuongHieu);
        }

        public async Task Delete(int id)
        {
            var thuongHieu = await GetThuongHieuById(id);
            if (thuongHieu == null) throw new KeyNotFoundException("ThuongHieu not found");
            _context.ThuongHieus.Remove(thuongHieu);
        }

        public async Task<List<ThuongHieu>> GetAllThuongHieus()
        {
            return await _context.ThuongHieus.ToListAsync();
        }

        public async Task<ThuongHieu?> GetThuongHieuById(int id)
        {
            return await _context.ThuongHieus.FindAsync(id);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(ThuongHieu thuongHieu)
        {
            if (await GetThuongHieuById(thuongHieu.IdThuongHieu) == null) throw new KeyNotFoundException("ThuongHieu not found");
            _context.Entry(thuongHieu).State = EntityState.Modified;
        }
    }
}
