using DATN_DT.Models;

namespace DATN_DT.IRepos
{
    public interface ITonKhoRepo
    {
        Task Create(TonKho tonKho);
        Task Delete(int id);
        Task<TonKho?> GetTonKhoById(int id);
        Task<List<TonKho>> GetAllTonKhos();
        Task SaveChanges();
        Task Update(TonKho tonKho);
    }
}
