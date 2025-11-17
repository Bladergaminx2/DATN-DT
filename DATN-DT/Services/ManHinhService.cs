using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class ManHinhService : IManHinhService
    {
        private readonly HttpClient _httpClient;
        public ManHinhService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task Create(ManHinh manHinh)
        {
            await _httpClient.PostAsJsonAsync("https://localhost:7150/api/ManHinh", manHinh);
        }

        public async Task Delete(int id)
        {
            await _httpClient.DeleteAsync($"https://localhost:7150/api/ManHinh/{id}");
        }

        public Task<List<ManHinh>> GetAllManHinhs()
        {
            var response = _httpClient.GetFromJsonAsync<List<ManHinh>>($"https://localhost:7150/api/ManHinh");
            return response;
        }

        public async Task<ManHinh?> GetManHinhById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ManHinh>($"https://localhost:7150/api/ManHinh/{id}");
            return response;
        }

        public async Task Update(ManHinh manHinh)
        {
            await _httpClient.PutAsJsonAsync($"https://localhost:7150/api/ManHinh/{manHinh.IdManHinh}", manHinh);
        }
    }
}
