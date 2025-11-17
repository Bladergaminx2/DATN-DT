using DATN_DT.Models;

namespace DATN_DT.IRepos
{
    public interface IModelSanPhamRepo
    {
        Task Create(ModelSanPham modelSanPham);
        Task Delete(int id);
        Task<ModelSanPham?> GetModelSanPhamById(int id);
        Task<List<ModelSanPham>> GetAllModelSanPhams();
        Task SaveChanges();
        Task Update(ModelSanPham modelSanPham);
    }
}
