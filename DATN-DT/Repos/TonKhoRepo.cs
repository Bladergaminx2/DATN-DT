using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

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
            // Kiểm tra xem ModelSanPham đã có trong kho nào chưa
            var modelInOtherKho = await _context.TonKhos
                .FirstOrDefaultAsync(tk => tk.IdModelSanPham == tonKho.IdModelSanPham);

            if (modelInOtherKho != null)
            {
                // Lấy tên kho để hiển thị thông báo chi tiết hơn
                var khoInfo = await _context.Khos
                    .FirstOrDefaultAsync(k => k.IdKho == modelInOtherKho.IdKho);
                var khoName = khoInfo != null ? khoInfo.TenKho : $"ID: {modelInOtherKho.IdKho}";

                throw new InvalidOperationException($"Model sản phẩm đã được lưu giữ ở kho '{khoName}'. Một model chỉ có thể ở một kho.");
            }

            // Tính số lượng từ IMEI có trạng thái "Còn hàng"
            var soLuongImei = await GetSoLuongImeiConHang(tonKho.IdModelSanPham.Value);

            // Gán số lượng tự động từ IMEI
            tonKho.SoLuong = soLuongImei;

            await _context.TonKhos.AddAsync(tonKho);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var tonKho = await GetTonKhoById(id);
            if (tonKho == null) throw new KeyNotFoundException("Tồn kho không tìm thấy!");
            _context.TonKhos.Remove(tonKho);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TonKho>> GetAllTonKhos()
        {
            return await _context.TonKhos
                .Include(tk => tk.ModelSanPham)
                    .ThenInclude(m => m.SanPham)
                .Include(tk => tk.Kho)
                .Include(tk => tk.ModelSanPham)
                    .ThenInclude(m => m.Imeis)
                .OrderBy(tk => tk.IdKho)
                .ThenBy(tk => tk.IdModelSanPham)
                .ToListAsync();
        }

        public async Task<TonKho?> GetTonKhoById(int id)
        {
            return await _context.TonKhos
                .Include(tk => tk.ModelSanPham)
                    .ThenInclude(m => m.SanPham)
                .Include(tk => tk.Kho)
                .Include(tk => tk.ModelSanPham)
                    .ThenInclude(m => m.Imeis)
                .FirstOrDefaultAsync(tk => tk.IdTonKho == id);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(TonKho tonKho)
        {
            var existingTonKho = await _context.TonKhos.FindAsync(tonKho.IdTonKho);
            if (existingTonKho == null)
                throw new KeyNotFoundException($"Không tìm thấy tồn kho với ID = {tonKho.IdTonKho}");

            // Nếu ModelSanPham bị thay đổi, kiểm tra xem model mới đã có trong kho khác chưa
            if (existingTonKho.IdModelSanPham != tonKho.IdModelSanPham)
            {
                if (await IsModelSanPhamInOtherKho(tonKho.IdModelSanPham.Value, tonKho.IdTonKho))
                {
                    var existingTonKhoForModel = await _context.TonKhos
                        .Include(tk => tk.Kho)
                        .FirstOrDefaultAsync(tk => tk.IdModelSanPham == tonKho.IdModelSanPham);

                    var khoName = existingTonKhoForModel?.Kho?.TenKho ?? $"ID: {existingTonKhoForModel?.IdKho}";
                    throw new InvalidOperationException($"Model sản phẩm đã được lưu giữ ở kho '{khoName}'. Một model chỉ có thể ở một kho.");
                }
            }

            // Tính toán số lượng mới từ IMEI
            var soLuongImei = await GetSoLuongImeiConHang(tonKho.IdModelSanPham.Value);

            // Cập nhật thông tin
            existingTonKho.IdModelSanPham = tonKho.IdModelSanPham;
            existingTonKho.IdKho = tonKho.IdKho;
            existingTonKho.SoLuong = soLuongImei; // Cập nhật số lượng từ IMEI

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetSoLuongImeiConHang(int idModelSanPham)
        {
            // Đếm số lượng IMEI có trạng thái "Còn hàng" của model sản phẩm
            return await _context.Imeis
                .Where(i => i.IdModelSanPham == idModelSanPham && i.TrangThai == "Còn hàng")
                .CountAsync();
        }

        public async Task<List<Kho>> GetAllKho()
        {
            // Sửa: Bỏ điều kiện TrangThaiKho vì model không có thuộc tính này
            // Có thể thêm điều kiện nếu model có thuộc tính trạng thái
            return await _context.Khos
                .OrderBy(k => k.TenKho)
                .ToListAsync();
        }

        public async Task<List<ModelSanPham>> GetAllModelSanPham()
        {
            return await _context.ModelSanPhams
                .Include(m => m.SanPham)
                .Where(m => m.TrangThai == 1) // Chỉ lấy model đang active
                .OrderBy(m => m.SanPham.TenSanPham)
                .ThenBy(m => m.TenModel)
                .ToListAsync();
        }

        public async Task<bool> IsModelSanPhamInAnyKho(int idModelSanPham)
        {
            return await _context.TonKhos
                .AnyAsync(tk => tk.IdModelSanPham == idModelSanPham);
        }

        public async Task<TonKho?> GetTonKhoByModelAndKho(int idModelSanPham, int idKho)
        {
            return await _context.TonKhos
                .Include(tk => tk.Kho)
                .FirstOrDefaultAsync(tk => tk.IdModelSanPham == idModelSanPham && tk.IdKho == idKho);
        }

        public async Task<bool> IsModelSanPhamInOtherKho(int idModelSanPham, int excludeIdTonKho)
        {
            return await _context.TonKhos
                .AnyAsync(tk => tk.IdModelSanPham == idModelSanPham && tk.IdTonKho != excludeIdTonKho);
        }

        // Phương thức để cập nhật số lượng tồn kho khi IMEI thay đổi
        public async Task UpdateSoLuongFromImei(int idModelSanPham)
        {
            var tonKhos = await _context.TonKhos
                .Where(tk => tk.IdModelSanPham == idModelSanPham)
                .ToListAsync();

            if (tonKhos.Any())
            {
                var soLuongImei = await GetSoLuongImeiConHang(idModelSanPham);

                foreach (var tonKho in tonKhos)
                {
                    tonKho.SoLuong = soLuongImei;
                }

                await _context.SaveChangesAsync();
            }
        }

        // Phương thức để lấy tồn kho theo model sản phẩm
        public async Task<TonKho?> GetTonKhoByModelSanPham(int idModelSanPham)
        {
            return await _context.TonKhos
                .Include(tk => tk.Kho)
                .Include(tk => tk.ModelSanPham)
                    .ThenInclude(m => m.SanPham)
                .FirstOrDefaultAsync(tk => tk.IdModelSanPham == idModelSanPham);
        }

        // PHƯƠNG THỨC MỚI: Refresh tồn kho cho model (Cách 1 bạn chọn)
        public async Task RefreshTonKhoForModel(int idModelSanPham)
        {
            // Tính lại số lượng từ IMEI có trạng thái "Còn hàng"
            var soLuongImei = await GetSoLuongImeiConHang(idModelSanPham);

            // Tìm tất cả tồn kho của model này
            var tonKhos = await _context.TonKhos
                .Where(tk => tk.IdModelSanPham == idModelSanPham)
                .ToListAsync();

            if (tonKhos.Any())
            {
                // Cập nhật số lượng cho tất cả tồn kho của model này
                foreach (var tonKho in tonKhos)
                {
                    tonKho.SoLuong = soLuongImei;
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                // 🔹 Nếu không có tồn kho, tạo mới để đảm bảo status được hiển thị đúng
                var khoMacDinh = await _context.Khos.FirstOrDefaultAsync();
                if (khoMacDinh != null)
                {
                    var newTonKho = new TonKho
                    {
                        IdModelSanPham = idModelSanPham,
                        IdKho = khoMacDinh.IdKho,
                        SoLuong = soLuongImei
                    };
                    _context.TonKhos.Add(newTonKho);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // PHƯƠNG THỨC MỚI: Refresh tất cả tồn kho (nếu cần)
        public async Task RefreshAllTonKho()
        {
            // Lấy tất cả model có trong tồn kho
            var modelIds = await _context.TonKhos
                .Select(tk => tk.IdModelSanPham)
                .Distinct()
                .ToListAsync();

            foreach (var modelId in modelIds)
            {
                if (modelId.HasValue)
                {
                    await RefreshTonKhoForModel(modelId.Value);
                }
            }
        }

        // PHƯƠNG THỨC MỚI: Kiểm tra và cập nhật tồn kho khi IMEI thay đổi
        public async Task CheckAndUpdateTonKhoOnImeiChange(int idModelSanPham, string? oldTrangThai = null, string? newTrangThai = null)
        {
            // Chỉ cập nhật nếu trạng thái IMEI thay đổi giữa "Còn hàng" và các trạng thái khác
            bool needUpdate = false;

            if (oldTrangThai != null && newTrangThai != null)
            {
                // Nếu chuyển từ "Còn hàng" sang trạng thái khác, hoặc ngược lại
                needUpdate = (oldTrangThai == "Còn hàng" && newTrangThai != "Còn hàng") ||
                            (oldTrangThai != "Còn hàng" && newTrangThai == "Còn hàng");
            }
            else
            {
                // Nếu không có thông tin cũ, luôn cập nhật
                needUpdate = true;
            }

            if (needUpdate)
            {
                await RefreshTonKhoForModel(idModelSanPham);
            }
        }
    }
}