using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Repos
{
    public class ManHinhRepo : IManHinhRepo
    {
        private readonly MyDbContext _context;
        public ManHinhRepo(MyDbContext context)
        {
            _context = context;
        }
        public async Task Create(ManHinh manhinh)
        {
            if (await GetManHinhById(manhinh.IdManHinh) != null) throw new DuplicateWaitObjectException($"Man hinh : {manhinh.IdManHinh} already exists");
            await _context.ManHinhs.AddAsync(manhinh);
            _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var manhinh = await GetManHinhById(id);
            if (manhinh == null) throw new KeyNotFoundException("Not found!");
            _context.ManHinhs.Remove(manhinh);
           await _context.SaveChangesAsync();
        }

        public async Task<List<ManHinh>> GetAllManHinhs()
        {
            return await _context.ManHinhs.ToListAsync();
        }

        public async Task<ManHinh> GetManHinhById(int id)
        {
            return await _context.ManHinhs.FindAsync(id);
        }

        public async Task Update(ManHinh manhinh)
        {
            if (await GetManHinhById(manhinh.IdManHinh) == null) throw new KeyNotFoundException("Not found!");
            _context.ManHinhs.Update(manhinh);
            _context.SaveChangesAsync();
        }
    }
}
