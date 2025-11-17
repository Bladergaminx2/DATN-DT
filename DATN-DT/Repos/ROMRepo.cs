using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Repos
{
    public class ROMRepo : IROMRepo
    {
        private readonly MyDbContext _context;
        public ROMRepo(MyDbContext context)
        {
            _context = context;
        }
        public async Task Create(ROM rom)
        {
            if (await GetROMById(rom.IdROM) != null) throw new ArgumentException($"ROM {rom.IdROM} already exists");
            await _context.ROMs.AddAsync(rom);
        }

        public async Task Delete(int id)
        {
            var rom = await GetROMById(id);
            if (rom == null) throw new KeyNotFoundException("Not found ROM to delete");
            _context.ROMs.Remove(rom);
        }

        public async Task<List<ROM>> GetAllROMs()
        {
            return await _context.ROMs.ToListAsync();
        }

        public async Task<ROM?> GetROMById(int id)
        {
            return await _context.ROMs.FindAsync(id);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(ROM rom)
        {
            if (await GetROMById(rom.IdROM) == null) throw new KeyNotFoundException("Not found ROM to update");
            _context.Entry(rom).State = EntityState.Modified;
        }
    }
}
