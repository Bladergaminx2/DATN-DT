using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Repos
{
    public class PinRepo : IPinRepo
    {
        private readonly MyDbContext _context;
        public PinRepo(MyDbContext context)
        {
            _context = context;
        }

        public async Task Create(Pin pin)
        {
            if (await GetPinById(pin.IdPin) != null) throw new ArgumentException($"Pin {pin.IdPin} already exists!");
            await _context.Pins.AddAsync(pin);
        }

        public async Task Delete(int id)
        {
            var pin = await GetPinById(id);
            if (pin == null) throw new KeyNotFoundException("Pin not found!");
            _context.Pins.Remove(pin);
        }

        public async Task<List<Pin>> GetAllPins()
        {
            return await _context.Pins.ToListAsync();
        }

        public async Task<Pin?> GetPinById(int id)
        {
            return await _context.Pins.FindAsync(id);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(Pin pin)
        {
            if (await GetPinById(pin.IdPin) == null) throw new KeyNotFoundException("Pin not found!");
            _context.Entry(pin).State = EntityState.Modified;
        }
    }
}
