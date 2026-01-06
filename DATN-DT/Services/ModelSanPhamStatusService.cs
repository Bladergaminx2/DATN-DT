using DATN_DT.Data;
using DATN_DT.IServices;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Services
{
    /// <summary>
    /// Service tự động cập nhật trạng thái ModelSanPham dựa trên số lượng tồn kho
    /// Logic:
    /// - Nếu số lượng = 0 và TrangThai != 0 (Ngừng kinh doanh) và != 3 (Đang nhập hàng) → đổi thành 2 (Hết hàng)
    /// - Nếu số lượng > 0 và TrangThai == 2 (Hết hàng) → đổi thành 1 (Còn hàng)
    /// - Nếu TrangThai == 0 (Ngừng kinh doanh) hoặc == 3 (Đang nhập hàng) → giữ nguyên
    /// </summary>
    public class ModelSanPhamStatusService : IModelSanPhamStatusService
    {
        private readonly MyDbContext _context;

        public ModelSanPhamStatusService(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tự động cập nhật trạng thái cho một ModelSanPham dựa trên số lượng tồn kho
        /// </summary>
        public async Task<int> UpdateStatusBasedOnStock(int idModelSanPham)
        {
            var model = await _context.ModelSanPhams
                .FirstOrDefaultAsync(m => m.IdModelSanPham == idModelSanPham);

            if (model == null)
                return 0;

            // Tính tổng số lượng tồn kho của model này
            var totalStock = await _context.TonKhos
                .Where(t => t.IdModelSanPham == idModelSanPham)
                .SumAsync(t => (int?)t.SoLuong) ?? 0;

            // Nếu không có bản ghi TonKho, kiểm tra trực tiếp từ IMEI
            if (totalStock == 0)
            {
                var imeiCount = await _context.Imeis
                    .Where(i => i.IdModelSanPham == idModelSanPham && i.TrangThai == "Còn hàng")
                    .CountAsync();
                
                totalStock = imeiCount;
            }

            // Lấy trạng thái hiện tại
            int currentStatus = model.TrangThai;
            int newStatus = currentStatus;

            // Logic cập nhật trạng thái:
            // - TrangThai = 0: Ngừng kinh doanh → giữ nguyên
            // - TrangThai = 1: Còn hàng (Active)
            // - TrangThai = 2: Hết hàng (Out of stock)
            // - TrangThai = 3: Đang nhập hàng → giữ nguyên

            if (currentStatus == 0 || currentStatus == 3)
            {
                // Ngừng kinh doanh hoặc Đang nhập hàng → giữ nguyên
                return 0;
            }

            if (totalStock == 0)
            {
                // Hết hàng → chuyển sang trạng thái 2 (Hết hàng)
                if (currentStatus != 2)
                {
                    newStatus = 2;
                }
            }
            else
            {
                // Còn hàng → chuyển sang trạng thái 1 (Còn hàng)
                if (currentStatus == 2)
                {
                    newStatus = 1;
                }
            }

            // Chỉ cập nhật nếu trạng thái thay đổi
            if (newStatus != currentStatus)
            {
                model.TrangThai = newStatus;
                
                // Đồng bộ trạng thái với SanPham.TrangThaiSP
                await SyncSanPhamStatus(model.IdSanPham);
                
                await _context.SaveChangesAsync();
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Đồng bộ trạng thái SanPham.TrangThaiSP dựa trên trạng thái của tất cả ModelSanPham
        /// </summary>
        private async Task SyncSanPhamStatus(int? idSanPham)
        {
            if (!idSanPham.HasValue)
                return;

            var sanPham = await _context.SanPhams
                .Include(sp => sp.ModelSanPhams)
                .FirstOrDefaultAsync(sp => sp.IdSanPham == idSanPham.Value);

            if (sanPham == null || sanPham.ModelSanPhams == null || !sanPham.ModelSanPhams.Any())
                return;

            // Kiểm tra tồn kho của tất cả ModelSanPham
            bool hasAnyInStock = false;
            bool allOutOfStock = true;

            foreach (var model in sanPham.ModelSanPhams)
            {
                // Bỏ qua các model đang Ngừng kinh doanh hoặc Đang nhập hàng
                if (model.TrangThai == 0 || model.TrangThai == 3)
                    continue;

                // Tính tổng số lượng tồn kho của model này
                var totalStock = await _context.TonKhos
                    .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                    .SumAsync(t => (int?)t.SoLuong) ?? 0;

                // Nếu không có bản ghi TonKho, kiểm tra trực tiếp từ IMEI
                if (totalStock == 0)
                {
                    var imeiCount = await _context.Imeis
                        .Where(i => i.IdModelSanPham == model.IdModelSanPham && i.TrangThai == "Còn hàng")
                        .CountAsync();
                    
                    totalStock = imeiCount;
                }

                if (totalStock > 0)
                {
                    hasAnyInStock = true;
                    allOutOfStock = false;
                    break; // Chỉ cần 1 model còn hàng là đủ
                }
            }

            // Cập nhật TrangThaiSP của SanPham
            // Chỉ cập nhật nếu không phải "Ngừng kinh doanh" hoặc "Đang nhập hàng"
            if (sanPham.TrangThaiSP != "Ngừng kinh doanh" && sanPham.TrangThaiSP != "Đang nhập hàng")
            {
                string newTrangThaiSP = sanPham.TrangThaiSP;
                
                if (allOutOfStock && !hasAnyInStock)
                {
                    newTrangThaiSP = "Hết hàng";
                }
                else if (hasAnyInStock)
                {
                    newTrangThaiSP = "Còn hàng";
                }
                
                // Chỉ cập nhật nếu thay đổi
                if (sanPham.TrangThaiSP != newTrangThaiSP)
                {
                    sanPham.TrangThaiSP = newTrangThaiSP;
                }
            }
        }

        /// <summary>
        /// Tự động cập nhật trạng thái cho tất cả ModelSanPham
        /// </summary>
        public async Task<int> UpdateAllStatusesBasedOnStock()
        {
            // Lấy tất cả ModelSanPham (trừ những cái đang Ngừng kinh doanh hoặc Đang nhập hàng)
            var models = await _context.ModelSanPhams
                .Where(m => m.TrangThai != 0 && m.TrangThai != 3) // Bỏ qua Ngừng kinh doanh và Đang nhập hàng
                .ToListAsync();

            int updatedCount = 0;

            foreach (var model in models)
            {
                // Tính tổng số lượng tồn kho
                var totalStock = await _context.TonKhos
                    .Where(t => t.IdModelSanPham == model.IdModelSanPham)
                    .SumAsync(t => (int?)t.SoLuong) ?? 0;

                // Nếu không có bản ghi TonKho, kiểm tra trực tiếp từ IMEI
                if (totalStock == 0)
                {
                    var imeiCount = await _context.Imeis
                        .Where(i => i.IdModelSanPham == model.IdModelSanPham && i.TrangThai == "Còn hàng")
                        .CountAsync();
                    
                    totalStock = imeiCount;
                }

                int currentStatus = model.TrangThai;
                int newStatus = currentStatus;

                if (totalStock == 0)
                {
                    // Hết hàng
                    if (currentStatus != 2)
                    {
                        newStatus = 2;
                    }
                }
                else
                {
                    // Còn hàng
                    if (currentStatus == 2)
                    {
                        newStatus = 1;
                    }
                }

                // Cập nhật nếu thay đổi
                if (newStatus != currentStatus)
                {
                    model.TrangThai = newStatus;
                    
                    // Đồng bộ trạng thái với SanPham.TrangThaiSP
                    await SyncSanPhamStatus(model.IdSanPham);
                    
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return updatedCount;
        }
    }
}

