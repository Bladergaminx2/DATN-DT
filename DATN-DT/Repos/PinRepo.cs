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
            await _context.Pins.AddAsync(pin);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var pin = await GetPinById(id);
            if (pin == null) throw new KeyNotFoundException("Pin not found!");
            _context.Pins.Remove(pin);
            await _context.SaveChangesAsync();
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
            // TÌM PIN HIỆN TẠI
            var existingPin = await _context.Pins.FindAsync(pin.IdPin);
            if (existingPin == null)
                throw new KeyNotFoundException($"Không tìm thấy Pin với ID = {pin.IdPin}");

            // KIỂM TRA XEM PIN CÓ ĐANG ĐƯỢC THEO DÕI KHÔNG
            var trackedEntity = _context.ChangeTracker.Entries<Pin>()
                .FirstOrDefault(e => e.Entity.IdPin == pin.IdPin);

            // NẾU KHÔNG ĐƯỢC THEO DÕI, ATTACH VÀO CONTEXT
            if (trackedEntity == null)
            {
                // Detach entity nếu đã tồn tại
                var local = _context.Set<Pin>()
                    .Local
                    .FirstOrDefault(entry => entry.IdPin == pin.IdPin);

                if (local != null)
                {
                    _context.Entry(local).State = EntityState.Detached;
                }

                // Attach entity mới
                _context.Pins.Attach(pin);
                _context.Entry(pin).State = EntityState.Modified;
            }
            else
            {
                // CẬP NHẬT TỪNG THUỘC TÍNH
                trackedEntity.Entity.LoaiPin = pin.LoaiPin;
                trackedEntity.Entity.DungLuongPin = pin.DungLuongPin;
                trackedEntity.Entity.CongNgheSac = pin.CongNgheSac;
                trackedEntity.Entity.MoTaPin = pin.MoTaPin;
            }

            // LƯU THAY ĐỔI
            await _context.SaveChangesAsync();
        }
    }
}