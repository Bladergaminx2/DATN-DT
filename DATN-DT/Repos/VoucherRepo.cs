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
    public class VoucherRepo : IVoucherRepo
    {
        private readonly MyDbContext _context;
        public VoucherRepo(MyDbContext context)
        {
            _context = context;
        }

        public async Task<Voucher> CreateAsync(Voucher voucher)
        {
            await _context.Vouchers.AddAsync(voucher);
            await _context.SaveChangesAsync();
            return voucher;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var v = await _context.Vouchers.FindAsync(id);
            if (v == null) return false;
            _context.Vouchers.Remove(v);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Voucher>> GetAllAsync()
        {
            return await _context.Vouchers
                .OrderByDescending(v => v.NgayBatDau)
                .ToListAsync();
        }

        public async Task<Voucher?> GetByCodeAsync(string maVoucher)
        {
            if (string.IsNullOrWhiteSpace(maVoucher)) return null;
            return await _context.Vouchers
                .FirstOrDefaultAsync(v => v.MaVoucher == maVoucher);
        }

        public async Task<Voucher?> GetByIdAsync(int id)
        {
            return await _context.Vouchers.FindAsync(id);
        }

        public async Task<int> CountUsageByCustomerAsync(int voucherId, int idKhachHang)
        {
            return await _context.VoucherSuDungs
                .CountAsync(x => x.IdVoucher == voucherId && x.IdKhachHang == idKhachHang);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Voucher voucher)
        {
            var existing = await _context.Vouchers.FindAsync(voucher.IdVoucher);
            if (existing == null) return false;

            // Update fields
            existing.MaVoucher = voucher.MaVoucher;
            existing.TenVoucher = voucher.TenVoucher;
            existing.MoTa = voucher.MoTa;
            existing.LoaiGiam = voucher.LoaiGiam;
            existing.GiaTri = voucher.GiaTri;
            existing.DonHangToiThieu = voucher.DonHangToiThieu;
            existing.GiamToiDa = voucher.GiamToiDa;
            existing.NgayBatDau = voucher.NgayBatDau;
            existing.NgayKetThuc = voucher.NgayKetThuc;
            existing.SoLuongSuDung = voucher.SoLuongSuDung;
            existing.SoLuongDaSuDung = voucher.SoLuongDaSuDung;
            existing.SoLuongMoiKhachHang = voucher.SoLuongMoiKhachHang;
            existing.TrangThai = voucher.TrangThai;
            existing.ApDungCho = voucher.ApDungCho;
            existing.DanhSachId = voucher.DanhSachId;

            _context.Vouchers.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}