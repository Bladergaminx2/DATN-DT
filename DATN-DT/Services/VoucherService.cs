using DATN_DT.Data;
using DATN_DT.IServices;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_DT.Services
{
    // Implementation only — interface lives in DATN_DT.IServices\IVoucherService.cs
    public class VoucherService : IVoucherService
    {
        private readonly MyDbContext _context;

        public VoucherService(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Voucher>> GetAllVouchersAsync()
        {
            return await _context.Vouchers
                .OrderByDescending(v => v.NgayBatDau)
                .ToListAsync();
        }

        public async Task<Voucher?> GetVoucherByIdAsync(int id)
        {
            return await _context.Vouchers.FindAsync(id);
        }

        public async Task<Voucher?> GetVoucherByCodeAsync(string maVoucher)
        {
            if (string.IsNullOrWhiteSpace(maVoucher)) return null;
            return await _context.Vouchers
                .FirstOrDefaultAsync(v => v.MaVoucher == maVoucher);
        }

        public async Task<bool> ValidateVoucherAsync(string maVoucher, int idKhachHang, decimal tongTienDonHang)
        {
            var voucher = await GetVoucherByCodeAsync(maVoucher);
            if (voucher == null) return false;

            var now = DateTime.Now;

            if (voucher.TrangThai != "HoatDong") return false;
            if (now < voucher.NgayBatDau || now > voucher.NgayKetThuc) return false;
            if (voucher.DonHangToiThieu.HasValue && tongTienDonHang < voucher.DonHangToiThieu.Value) return false;
            if (voucher.SoLuongSuDung.HasValue && voucher.SoLuongDaSuDung >= voucher.SoLuongSuDung.Value) return false;

            if (voucher.SoLuongMoiKhachHang.HasValue && voucher.SoLuongMoiKhachHang.Value > 0)
            {
                var used = await _context.VoucherSuDungs
                    .CountAsync(x => x.IdVoucher == voucher.IdVoucher && x.IdKhachHang == idKhachHang);
                if (used >= voucher.SoLuongMoiKhachHang.Value) return false;
            }

            return true;
        }

        public async Task<decimal> CalculateDiscountAsync(Voucher voucher, decimal tongTienDonHang, List<int>? danhSachIdSanPham = null)
        {
            if (voucher == null) return 0m;
            if (voucher.DonHangToiThieu.HasValue && tongTienDonHang < voucher.DonHangToiThieu.Value) return 0m;

            decimal discount = 0m;
            if (voucher.LoaiGiam?.Equals("PhanTram", StringComparison.OrdinalIgnoreCase) == true)
            {
                discount = Math.Round(tongTienDonHang * (voucher.GiaTri / 100m), 0);
                if (voucher.GiamToiDa.HasValue) discount = Math.Min(discount, voucher.GiamToiDa.Value);
            }
            else // SoTien
            {
                discount = voucher.GiaTri;
                if (voucher.GiamToiDa.HasValue) discount = Math.Min(discount, voucher.GiamToiDa.Value);
                if (discount > tongTienDonHang) discount = tongTienDonHang;
            }

            return discount;
        }

        public async Task<bool> UseVoucherAsync(int idVoucher, int idKhachHang, int? idHoaDon, decimal soTienGiam)
        {
            var voucher = await GetVoucherByIdAsync(idVoucher);
            if (voucher == null) return false;

            var suDung = new VoucherSuDung
            {
                IdVoucher = idVoucher,
                IdKhachHang = idKhachHang,
                IdHoaDon = idHoaDon,
                SoTienGiam = soTienGiam,
                NgaySuDung = DateTime.Now
            };

            await _context.VoucherSuDungs.AddAsync(suDung);

            voucher.SoLuongDaSuDung++;
            if (voucher.SoLuongSuDung.HasValue && voucher.SoLuongDaSuDung >= voucher.SoLuongSuDung.Value)
            {
                voucher.TrangThai = "HetHan";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Voucher> CreateVoucherAsync(Voucher voucher)
        {
            voucher.SoLuongDaSuDung = 0;
            if (string.IsNullOrEmpty(voucher.TrangThai))
            {
                var now = DateTime.Now;
                voucher.TrangThai = now < voucher.NgayBatDau ? "SapDienRa"
                    : (now >= voucher.NgayBatDau && now <= voucher.NgayKetThuc) ? "HoatDong" : "HetHan";
            }

            await _context.Vouchers.AddAsync(voucher);
            await _context.SaveChangesAsync();
            return voucher;
        }

        public async Task<bool> UpdateVoucherAsync(Voucher voucher)
        {
            try
            {
                _context.Vouchers.Update(voucher);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteVoucherAsync(int id)
        {
            var voucher = await GetVoucherByIdAsync(id);
            if (voucher == null) return false;

            var hasUsage = await _context.VoucherSuDungs.AnyAsync(v => v.IdVoucher == id);
            if (hasUsage)
            {
                voucher.TrangThai = "TamDung";
                await _context.SaveChangesAsync();
                return true;
            }

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateVoucherStatusAsync()
        {
            var now = DateTime.Now;
            var vouchers = await _context.Vouchers
                .Where(v => v.TrangThai == "HoatDong" || v.TrangThai == "SapDienRa")
                .ToListAsync();

            foreach (var v in vouchers)
            {
                if (now > v.NgayKetThuc) v.TrangThai = "HetHan";
                else if (now >= v.NgayBatDau && now <= v.NgayKetThuc)
                {
                    if (v.TrangThai == "SapDienRa") v.TrangThai = "HoatDong";
                }
                else if (now < v.NgayBatDau) v.TrangThai = "SapDienRa";
            }

            await _context.SaveChangesAsync();
        }
    }
}