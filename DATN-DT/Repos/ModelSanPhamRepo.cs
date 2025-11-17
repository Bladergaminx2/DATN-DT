using DATN_DT.IRepos;
using DATN_DT.Models;

namespace DATN_DT.Repos
{
    public class ModelSanPhamRepo : IModelSanPhamRepo
    {
        public Task Create(ModelSanPham modelSanPham)
        {
            throw new NotImplementedException();
        }

        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<ModelSanPham>> GetAllModelSanPhams()
        {
            throw new NotImplementedException();
        }

        public async Task<ModelSanPham?> GetModelSanPhamById(int id)
        {
            return await
        }

        public Task SaveChanges()
        {
            throw new NotImplementedException();
        }

        public Task Update(ModelSanPham modelSanPham)
        {
            throw new NotImplementedException();
        }
    }
}
