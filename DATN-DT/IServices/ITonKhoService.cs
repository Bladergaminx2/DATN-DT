using DATN_DT.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.IServices
{
    public interface ITonKhoService
    {
        Task Create(TonKho tonKho);
        Task Delete(int id);
        Task<List<TonKho>> GetAllTonKhos();
        Task<TonKho?> GetTonKhoById(int id);
        Task Update(TonKho tonKho);

        // Các phương thức bổ sung
        Task<int> GetSoLuongImeiConHang(int idModelSanPham);
        Task<List<Kho>> GetAllKho();
        Task<List<ModelSanPham>> GetAllModelSanPham();
        Task<bool> IsModelSanPhamInAnyKho(int idModelSanPham);
        Task<TonKho?> GetTonKhoByModelAndKho(int idModelSanPham, int idKho);
        Task UpdateSoLuongFromImei(int idModelSanPham);
        Task<TonKho?> GetTonKhoByModelSanPham(int idModelSanPham);

        // Các phương thức mới cho Cách 1
        Task RefreshTonKhoForModel(int idModelSanPham);
        Task RefreshAllTonKho();
        Task CheckAndUpdateTonKhoOnImeiChange(int idModelSanPham, string? oldTrangThai = null, string? newTrangThai = null);
    }
}