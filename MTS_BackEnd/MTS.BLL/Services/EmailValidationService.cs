using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace MTS.BLL.Services
{
    public interface IEmailValidationService
    {
        Task<bool> IsEmailValidAsync(string email);
    }

    public class EmailValidationService : IEmailValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public EmailValidationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["APIVerve:ApiKey"] ?? throw new InvalidOperationException("APIVerve API key not found.");
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        }

        public async Task<bool> IsEmailValidAsync(string email)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://api.apiverve.com/v1/emailvalidator?email={email}");

                if (!response.IsSuccessStatusCode)
                {
                    // Log the error or handle it as needed
                    return false; // Fail safely
                }

                var content = await response.Content.ReadAsStringAsync();
                var validationResult = JsonSerializer.Deserialize<EmailValidationResultDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return validationResult?.Data?.IsValid ?? false;
            }
            catch (Exception ex)
            {
                // Log exception
                return false; // Fail safely in case of exceptions
            }
        }
    }

    public class EmailValidationResultDto
    {
        public EmailValidationData? Data { get; set; }
    }

    public class EmailValidationData
    {
        public bool IsValid { get; set; }
    }
} 