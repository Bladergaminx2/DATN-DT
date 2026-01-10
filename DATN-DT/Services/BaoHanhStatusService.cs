using DATN_DT.Data;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Services
{
    public interface IBaoHanhStatusService
    {
        Task UpdateBaoHanhStatusAsync();
    }

    public class BaoHanhStatusService : IBaoHanhStatusService
    {
        private readonly MyDbContext _context;

        public BaoHanhStatusService(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tự động cập nhật trạng thái "Hết bảo hành" cho các phiếu bảo hành đã quá ngày trả
        /// </summary>
        public async Task UpdateBaoHanhStatusAsync()
        {
            var now = DateTime.Now;
            
            // Lấy các phiếu bảo hành cần cập nhật:
            // - Có NgayTra < hiện tại
            // - Trạng thái chưa phải "Hết bảo hành", "Đã hoàn thành", "Từ chối"
            var baoHanhsCanUpdate = await _context.BaoHanhs
                .Where(b => b.NgayTra.HasValue && 
                           b.NgayTra.Value < now &&
                           b.TrangThai != "Hết bảo hành" &&
                           b.TrangThai != "Đã hoàn thành" &&
                           b.TrangThai != "Từ chối" &&
                           b.TrangThai != "Hoàn tất")
                .ToListAsync();

            foreach (var baoHanh in baoHanhsCanUpdate)
            {
                var trangThaiCu = baoHanh.TrangThai;
                baoHanh.TrangThai = "Hết bảo hành";
                
                // Ghi lịch sử
                var lichSu = new BaoHanhLichSu
                {
                    IdBaoHanh = baoHanh.IdBaoHanh,
                    ThaoTac = "Tự động cập nhật trạng thái",
                    TrangThaiCu = trangThaiCu,
                    TrangThaiMoi = "Hết bảo hành",
                    MoTa = $"Tự động chuyển sang 'Hết bảo hành' do quá ngày trả ({baoHanh.NgayTra.Value:dd/MM/yyyy})",
                    ThoiGian = now
                };
                
                _context.BaoHanhLichSus.Add(lichSu);
            }

            if (baoHanhsCanUpdate.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}

