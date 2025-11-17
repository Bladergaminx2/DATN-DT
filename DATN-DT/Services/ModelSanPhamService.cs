using DATN_DT.IServices;
using DATN_DT.Models;

namespace DATN_DT.Services
{
    public class ModelSanPhamService : IModelSanPhamService
    {
        private readonly HttpClient _httpClient;
        public ModelSanPhamService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task Create(ModelSanPham modelSanPham)
        {
            await _httpClient.PostAsJsonAsync<ModelSanPham>("https://localhost:7150/api/ModelSanPhams", modelSanPham);
        }

        public async Task Delete(int id)
        {
            await _httpClient.DeleteAsync($"https://localhost:7150/api/ModelSanPhams/{id}");
        }

        public Task<List<ModelSanPham>> GetAllModelSanPhams()
        {
            var response =  _httpClient.GetFromJsonAsync<List< ModelSanPham>>("https://localhost:7150/api/ModelSanPhams");
            return response;
        }

        public async Task<ModelSanPham?> GetModelSanPhamById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ModelSanPham>($"https://localhost:7150/api/ModelSanPhams/{id}");
            return response;
        }

        public async Task Update(ModelSanPham modelSanPham)
        {
            await _httpClient.PutAsJsonAsync<ModelSanPham>($"https://localhost:7150/api/ModelSanPhams/{modelSanPham.IdModelSanPham}", modelSanPham);
        }
    }
}
