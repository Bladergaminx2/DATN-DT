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
            if (await GetSanPhamById(sanpham.IdSanPham) != null) 
            {
                throw new Exception("SanPham with the same Id already exists.");
            }
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
            return await _context.SanPhams.ToListAsync();
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
            if (GetSanPhamById(sanpham.IdSanPham) == null)
            {
                throw new Exception("SanPham not found.");
            }
            _context.Entry(sanpham).State = EntityState.Modified;
        }
    }
}       
