using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Repos
{
    public class NhanVienRepo : INhanVienRepo
    {
        private readonly MyDbContext _context;
        public NhanVienRepo(MyDbContext context)
        {
            _context = context;
        }
        public async Task Create(NhanVien nhanvien)
        {
            if (await GetNhanVienById(nhanvien.IdNhanVien) != null) throw new DuplicateWaitObjectException($"NhanVien {nhanvien.IdNhanVien} already exists");
            await _context.NhanViens.AddAsync(nhanvien);
        }

        public async Task Delete(int id)
        {
            var nhanvien = await GetNhanVienById(id);
            if (nhanvien == null) throw new KeyNotFoundException("NhanVien not found");
            _context.NhanViens.Remove(nhanvien);
        }

        public async Task<List<NhanVien>> GetAllNhanViens()
        {
            return await _context.NhanViens.ToListAsync();
        }

        public async Task<NhanVien?> GetNhanVienById(int id)
        {
            return await _context.NhanViens.FindAsync(id);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(NhanVien nhanvien)
        {
            if (await GetNhanVienById(nhanvien.IdNhanVien) == null) throw new KeyNotFoundException("NhanVien not found");
            _context.Entry(nhanvien).State = EntityState.Modified;
        }
    }
}
