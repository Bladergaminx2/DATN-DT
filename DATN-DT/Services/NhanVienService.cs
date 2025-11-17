using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class NhanVienService : INhanVienService
    {
        private readonly HttpClient _httpClient;
        public NhanVienService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task Create(NhanVien nhanvien)
        {
            await _httpClient.PostAsJsonAsync<NhanVien>("https://localhost:7150/api/NhanVien", nhanvien);
        }

        public async Task Delete(int id)
        {
            await _httpClient.DeleteAsync($"https://localhost:7150/api/NhanVien/{id}");
        }

        public Task<List<NhanVien>> GetAllNhanViens()
        {
            var response =  _httpClient.GetFromJsonAsync<List<NhanVien>>("https://localhost:7150/api/NhanVien");
            return response;
        }

        public Task<NhanVien?> GetNhanVienById(int id)
        {
            var response =  _httpClient.GetFromJsonAsync<NhanVien>($"https://localhost:7150/api/NhanVien/{id}");
            return response;
        }

        public async Task Update(NhanVien nhanvien)
        {
            await _httpClient.PutAsJsonAsync<NhanVien>($"https://localhost:7150/api/NhanVien/{nhanvien.IdNhanVien}", nhanvien);
        }
    }
}
