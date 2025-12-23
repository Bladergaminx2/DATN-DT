// IThuongHieuRepo.cs
using DATN_DT.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN_DT.IRepos
{
    public interface IThuongHieuRepo
    {
        Task CreateThuongHieu(ThuongHieu thuongHieu);
        Task DeleteThuongHieu(int id);
        Task<List<ThuongHieu>> GetAllThuongHieus();
        Task<ThuongHieu?> GetThuongHieuById(int id);
        Task UpdateThuongHieu(ThuongHieu thuongHieu);
    }
}