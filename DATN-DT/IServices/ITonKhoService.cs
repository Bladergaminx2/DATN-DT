using DATN_DT.Models;

namespace DATN_DT.IServices
{
    public interface ITonKhoService
    {
        Task Create(TonKho tonKho);
        Task Delete(int id);
        Task<TonKho?> GetTonKhoById(int id);
        Task<List<TonKho>> GetAllTonKhos();
        Task Update(TonKho tonKho);
    }
}
