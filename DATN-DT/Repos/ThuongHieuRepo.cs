// ThuongHieuRepo.cs
using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DATN_DT.Repos
{
    public class ThuongHieuRepo : IThuongHieuRepo
    {
        private readonly MyDbContext _context;

        public ThuongHieuRepo(MyDbContext context)
        {
            _context = context;
        }

        public async Task CreateThuongHieu(ThuongHieu thuongHieu)
        {
            await _context.ThuongHieus.AddAsync(thuongHieu);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteThuongHieu(int id)
        {
            var thuongHieu = await GetThuongHieuById(id);
            if (thuongHieu != null)
            {
                _context.ThuongHieus.Remove(thuongHieu);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ThuongHieu>> GetAllThuongHieus()
        {
            return await _context.ThuongHieus
                .OrderBy(th => th.TenThuongHieu)
                .ToListAsync();
        }

        public async Task<ThuongHieu?> GetThuongHieuById(int id)
        {
            return await _context.ThuongHieus
                .FirstOrDefaultAsync(th => th.IdThuongHieu == id);
        }

        public async Task UpdateThuongHieu(ThuongHieu thuongHieu)
        {
            _context.ThuongHieus.Update(thuongHieu);
            await _context.SaveChangesAsync();
        }
    }
}