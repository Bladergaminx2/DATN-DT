using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class TonKhoService : ITonKhoService
    {
        private readonly HttpClient _httpClient;
        public TonKhoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task Create(TonKho tonKho)
        {
            await _httpClient.PostAsJsonAsync<TonKho>("https://localhost:7150/api/TonKho", tonKho);
        }

        public async Task Delete(int id)
        {
            await _httpClient.DeleteAsync($"https://localhost:7150/api/TonKho/{id}");
        }

        public Task<List<TonKho>> GetAllTonKhos()
        {
            var response =  _httpClient.GetFromJsonAsync<List<TonKho>>("https://localhost:7150/api/TonKho");
            return response;
        }

        public Task<TonKho?> GetTonKhoById(int id)
        {
            var response =  _httpClient.GetFromJsonAsync<TonKho>($"https://localhost:7150/api/TonKho/{id}");
            return response;
        }

        public async Task Update(TonKho tonKho)
        {
            await _httpClient.PutAsJsonAsync<TonKho>($"https://localhost:7150/api/TonKho/{tonKho.IdTonKho}", tonKho);
        }
    }
}
