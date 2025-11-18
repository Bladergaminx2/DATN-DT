using DATN_DT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.IServices
{
    public interface ITonKhoService
    {
        Task Create(TonKho tonKho);
        Task Delete(int id);
        Task<TonKho?> GetTonKhoById(int id);
        Task<List<TonKho>> GetAllTonKhos();

        Task<List<ModelSanPham>> GetModelSanPhams();
        Task<bool> CheckDuplicate(int? idModelSanPham, int? idKho, int excludeId);
        Task<List<Kho>> GetKhos();

        Task Update(TonKho tonKho);
    }
}
