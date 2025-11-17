using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class SanPhamService : ISanPhamService
    {
        private readonly HttpClient _httpClient;
        public SanPhamService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task Create(SanPham sanpham)
        {
            await _httpClient.PostAsJsonAsync<SanPham>("https://localhost:7150/api/SanPhams", sanpham);
        }

        public async Task Delete(int id)
        {
            await _httpClient.DeleteAsync($"https://localhost:7150/api/SanPhams/{id}");
        }

        public Task<List<SanPham>> GetAllSanPhams()
        {
            var response = _httpClient.GetFromJsonAsync<List<SanPham>>("https://localhost:7150/api/SanPhams");
            return response;
        }

        public Task<SanPham?> GetSanPhamById(int id)
        {
            var response = _httpClient.GetFromJsonAsync<SanPham>($"https://localhost:7150/api/SanPhams/{id}");
            return response;
        }

        public async Task Update(SanPham sanpham)
        {
            await _httpClient.PutAsJsonAsync<SanPham>($"https://localhost:7150/api/SanPhams/{sanpham.IdSanPham}", sanpham);
        }
    }
}
