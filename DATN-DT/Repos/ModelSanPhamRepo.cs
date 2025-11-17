using DATN_DT.Data;
using DATN_DT.IRepos;
using DATN_DT.Models;
using Microsoft.EntityFrameworkCore;

namespace DATN_DT.Repos
{
    public class ModelSanPhamRepo : IModelSanPhamRepo
    {
        private readonly MyDbContext _context;
        public ModelSanPhamRepo(MyDbContext context)
        {
            _context = context;
        }
        public async Task Create(ModelSanPham modelSanPham)
        {
            if (await GetModelSanPhamById(modelSanPham.IdModelSanPham) != null) throw new ArgumentException("This product already exists!!");
            await _context.ModelSanPhams.AddAsync(modelSanPham);
        }

        public async Task Delete(int id)
        {
            var modelSanPham = await GetModelSanPhamById(id);
            if (modelSanPham == null) throw new KeyNotFoundException("Not found this product!!");
            _context.ModelSanPhams.Remove(modelSanPham);
        }

        public async Task<List<ModelSanPham>> GetAllModelSanPhams()
        {
            return await _context.ModelSanPhams.ToListAsync();
        }

        public async Task<ModelSanPham?> GetModelSanPhamById(int id)
        {
            return await _context.ModelSanPhams.FindAsync(id);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(ModelSanPham modelSanPham)
        {
            if (await GetModelSanPhamById(modelSanPham.IdModelSanPham) == null) throw new KeyNotFoundException("Not found this product!!");
            _context.Entry(modelSanPham).State = EntityState.Modified;
        }
    }
}
