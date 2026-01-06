using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.Services
{
    public class TonKhoService : ITonKhoService
    {
        private readonly ITonKhoRepo _tonKhoRepo;
        private readonly IModelSanPhamStatusService _statusService;
        
        public TonKhoService(ITonKhoRepo tonKhoRepo, IModelSanPhamStatusService statusService)
        {
            _tonKhoRepo = tonKhoRepo;
            _statusService = statusService;
        }

        public async Task Create(TonKho tonKho)
        {
            await _tonKhoRepo.Create(tonKho);
            // Tự động cập nhật trạng thái sau khi tạo tồn kho
            if (tonKho.IdModelSanPham.HasValue)
            {
                await _statusService.UpdateStatusBasedOnStock(tonKho.IdModelSanPham.Value);
            }
        }

        public async Task Delete(int id)
        {
            await _tonKhoRepo.Delete(id);
        }

        public async Task<List<TonKho>> GetAllTonKhos()
        {
            return await _tonKhoRepo.GetAllTonKhos();
        }

        public async Task<TonKho?> GetTonKhoById(int id)
        {
            return await _tonKhoRepo.GetTonKhoById(id);
        }

        public async Task Update(TonKho tonKho)
        {
            await _tonKhoRepo.Update(tonKho);
            // Tự động cập nhật trạng thái sau khi cập nhật tồn kho
            if (tonKho.IdModelSanPham.HasValue)
            {
                await _statusService.UpdateStatusBasedOnStock(tonKho.IdModelSanPham.Value);
            }
        }

        public async Task<int> GetSoLuongImeiConHang(int idModelSanPham)
        {
            return await _tonKhoRepo.GetSoLuongImeiConHang(idModelSanPham);
        }

        public async Task<List<Kho>> GetAllKho()
        {
            return await _tonKhoRepo.GetAllKho();
        }

        public async Task<List<ModelSanPham>> GetAllModelSanPham()
        {
            return await _tonKhoRepo.GetAllModelSanPham();
        }

        public async Task<bool> IsModelSanPhamInAnyKho(int idModelSanPham)
        {
            return await _tonKhoRepo.IsModelSanPhamInAnyKho(idModelSanPham);
        }

        public async Task<TonKho?> GetTonKhoByModelAndKho(int idModelSanPham, int idKho)
        {
            return await _tonKhoRepo.GetTonKhoByModelAndKho(idModelSanPham, idKho);
        }

        public async Task UpdateSoLuongFromImei(int idModelSanPham)
        {
            await _tonKhoRepo.UpdateSoLuongFromImei(idModelSanPham);
            // Tự động cập nhật trạng thái sau khi cập nhật số lượng từ IMEI
            await _statusService.UpdateStatusBasedOnStock(idModelSanPham);
        }

        public async Task<TonKho?> GetTonKhoByModelSanPham(int idModelSanPham)
        {
            return await _tonKhoRepo.GetTonKhoByModelSanPham(idModelSanPham);
        }

        // Các phương thức mới
        public async Task RefreshTonKhoForModel(int idModelSanPham)
        {
            await _tonKhoRepo.RefreshTonKhoForModel(idModelSanPham);
            // Tự động cập nhật trạng thái sau khi refresh tồn kho
            await _statusService.UpdateStatusBasedOnStock(idModelSanPham);
        }

        public async Task RefreshAllTonKho()
        {
            await _tonKhoRepo.RefreshAllTonKho();
            // Tự động cập nhật trạng thái cho tất cả sản phẩm
            await _statusService.UpdateAllStatusesBasedOnStock();
        }

        public async Task CheckAndUpdateTonKhoOnImeiChange(int idModelSanPham, string? oldTrangThai = null, string? newTrangThai = null)
        {
            await _tonKhoRepo.CheckAndUpdateTonKhoOnImeiChange(idModelSanPham, oldTrangThai, newTrangThai);
            // Tự động cập nhật trạng thái sau khi IMEI thay đổi
            await _statusService.UpdateStatusBasedOnStock(idModelSanPham);
        }
    }
}