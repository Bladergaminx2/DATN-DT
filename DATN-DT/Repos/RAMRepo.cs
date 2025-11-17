using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Repos
{
    public class RAMRepo : IRAMRepo
    {
        private readonly MyDbContext _context;
        public RAMRepo(MyDbContext context)
        {
            _context = context;
        }
        public async Task Create(RAM ram)
        {
            if (await GetRAMById(ram.IdRAM) != null) throw new ArgumentException($"Ram {ram.IdRAM} already exists");
            await _context.RAMs.AddAsync(ram);
        }

        public async Task Delete(int id)
        {
            var ram = await GetRAMById(id);
            if (ram == null) throw new KeyNotFoundException("Ram not found");
            _context.RAMs.Remove(ram);
        }

        public async Task<List<RAM>> GetAllRAMs()
        {
            return await _context.RAMs.ToListAsync();
        }

        public async Task<RAM?> GetRAMById(int id)
        {
            return await _context.RAMs.FindAsync(id);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(RAM ram)
        {
            if (await GetRAMById(ram.IdRAM) == null) throw new KeyNotFoundException("Ram not found");
            _context.Entry(ram).State = EntityState.Modified;
        }
    }
}
