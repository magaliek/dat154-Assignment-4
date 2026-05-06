using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharedLogic;

namespace SimulationInterface
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:5049";
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ApiService()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            _client = new HttpClient(handler);
        }

        public async Task<int?> StartSessionAsync(int patientId, int studentCustomUserId)
        {
            var body = JsonSerializer.Serialize(new StartSessionDto { PatientId = patientId, StudentCustomUserId = studentCustomUserId }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var resp = await _client.PostAsync($"{BaseUrl}/api/sessions", new StringContent(body, Encoding.UTF8, "application/json"));
            if (!resp.IsSuccessStatusCode)
                return null;
            var s = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(s, JsonOptions);
        }

        public async Task PostActionAsync(int sessionId, string drug, double doseMg, string route)
        {
            var dto = new RegisterActionDto { Kind = "Medication", Drug = drug, DoseMg = doseMg, Route = route };
            var body = JsonSerializer.Serialize(dto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await _client.PostAsync($"{BaseUrl}/api/sessions/{sessionId}/actions", new StringContent(body, Encoding.UTF8, "application/json"));
        }

        public async Task<CaseDto?> GetCaseForStudent(int studentId)
        {
            var response = await _client.GetAsync($"{BaseUrl}/api/case/{studentId}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<CaseDto>(json, JsonOptions);
        }
    }
}