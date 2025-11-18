using DATN_DT.IRepos;
using DATN_DT.IServices;
using DATN_DT.Models;
using DATN_DT.Repos;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Services
{
    public class TonKhoService : ITonKhoService
    {
        private readonly ITonKhoRepo _tonKhoRepo;
        public TonKhoService(ITonKhoRepo tonKhoRepo)
        {
            _tonKhoRepo = tonKhoRepo;
        }
        public async Task Create(TonKho tonKho)
        {
            await _tonKhoRepo.Create(tonKho);
        }

        public async Task Delete(int id)
        {
            await _tonKhoRepo.Delete(id);
        }

        public async Task<List<TonKho>> GetAllTonKhos()
        {
           return await _tonKhoRepo.GetAllTonKhos();
        }

        public async Task<List<Kho>> GetKhos()
        {
            return await _tonKhoRepo.GetKhos();
        }
        public async Task<List<ModelSanPham>> GetModelSanPhams()
        {
            return await _tonKhoRepo.GetModelSanPhams();
        }
        public async Task<bool> CheckDuplicate(int? idModelSanPham, int? idKho, int excludeId)
        {
            return await _tonKhoRepo.CheckDuplicate(idModelSanPham, idKho, excludeId);
        }

        public async Task<TonKho?> GetTonKhoById(int id)
        {
            return await _tonKhoRepo.GetTonKhoById(id);
        }

        public async Task Update(TonKho tonKho)
        {
            await _tonKhoRepo.Update(tonKho);
        }
    }
}
