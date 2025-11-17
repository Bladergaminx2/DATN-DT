using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class ThuongHieuService : IThuongHieuService
    {
        private readonly HttpClient _httpClient;
        public ThuongHieuService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task CreateThuongHieu(ThuongHieu thuongHieu)
        {
            await _httpClient.PostAsJsonAsync("https://localhost:7150/api/ThuongHieu", thuongHieu);
        }

        public async Task DeleteThuongHieu(int id)
        {
            await _httpClient.DeleteAsync($"https://localhost:7150/api/ThuongHieu/{id}");
        }

        public Task<List<ThuongHieu>> GetAllThuongHieus()
        {
            var response =  _httpClient.GetFromJsonAsync<List<ThuongHieu>>("https://localhost:7150/api/ThuongHieu");
            return response;
        }

        public Task<ThuongHieu?> GetThuongHieuById(int id)
        {
            var response =  _httpClient.GetFromJsonAsync<ThuongHieu>($"https://localhost:7150/api/ThuongHieu/{id}");
            return response;
        }

        public async Task UpdateThuongHieu(int id)
        {
            await _httpClient.PutAsJsonAsync($"https://localhost:7150/api/ThuongHieu/{id}", id);
        }
    }
}
