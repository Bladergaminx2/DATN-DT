using DATN_DT.Models;

namespace DATN_DT.IRepos
{
    public interface ITonKhoRepo
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
