using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SimulationInterface.Models;

namespace SimulationInterface
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "https://localhost:7101";

        public ApiService()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            _client = new HttpClient(handler);
        }

        public async Task<PatientModel?> GetCaseForStudent(int studentId)
        {
            var response = await _client.GetAsync($"{BaseUrl}/api/case/{studentId}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<PatientModel>(json, options);
        }
    }
}