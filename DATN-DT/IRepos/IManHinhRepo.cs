using DATN_DT.Models;
using System.ComponentModel;

namespace DATN_DT.IRepos
{
    public interface IManHinhRepo
    {
        Task Create(ManHinh manhinh);
        Task Delete(int id);
        Task<ManHinh> GetManHinhById(int id);
        Task<List<ManHinh>> GetAllManHinhs();
        Task SaveChanges();
        Task Update(ManHinh manhinh);
    }
}
