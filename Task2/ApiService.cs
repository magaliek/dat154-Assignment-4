using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharedLogic;

namespace Task2
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ApiService()
        {
            var baseUri = ApiBaseUrl.AsHttpClientBaseAddress();
            var handler = new HttpClientHandler();
            if (ApiBaseUrl.IsLocalhost(baseUri))
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            _client = new HttpClient(handler) { BaseAddress = baseUri };
        }

        public async Task<int?> StartSessionAsync(int patientId, int studentCustomUserId)
        {
            var body = JsonSerializer.Serialize(new StartSessionDto { PatientId = patientId, StudentCustomUserId = studentCustomUserId }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var resp = await _client.PostAsync("api/sessions", new StringContent(body, Encoding.UTF8, "application/json"));
            if (!resp.IsSuccessStatusCode)
                return null;
            var s = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(s, JsonOptions);
        }

        public async Task PostActionAsync(int sessionId, string drug, double doseMg, string route)
        {
            var dto = new RegisterActionDto { Kind = "Medication", Drug = drug, DoseMg = doseMg, Route = route };
            var body = JsonSerializer.Serialize(dto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await _client.PostAsync($"api/sessions/{sessionId}/actions", new StringContent(body, Encoding.UTF8, "application/json"));
        }

        public async Task<(CaseDto? Case, string? ErrorMessage)> GetCaseForStudent(int studentId)
        {
            var response = await _client.GetAsync($"api/case/{studentId}");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                string? err = body;
                if (!string.IsNullOrEmpty(body) && body[0] == '"')
                {
                    try
                    {
                        err = JsonSerializer.Deserialize<string>(body, JsonOptions);
                    }
                    catch
                    {
                        /* use raw body */
                    }
                }

                return (null, string.IsNullOrWhiteSpace(err) ? response.ReasonPhrase : err);
            }

            var dto = JsonSerializer.Deserialize<CaseDto>(body, JsonOptions);
            return (dto, null);
        }
    }
}