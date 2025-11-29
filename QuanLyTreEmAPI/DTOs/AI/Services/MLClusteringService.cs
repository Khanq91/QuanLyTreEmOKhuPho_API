using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuanLyTreEmAPI.DTOs.AI;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace QuanLyTreEmAPI.Services
{
    public interface IMLClusteringService
    {
        Task<ClusterPrediction> PredictAsync(TreEmFeatures features);
        Task<BatchPredictionResponse> PredictBatchAsync(List<TreEmFeatures> featuresList);
        Task<bool> IsHealthyAsync();
    }

    public class MLClusteringService : IMLClusteringService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MLClusteringService> _logger;
        private readonly string _pythonApiUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public MLClusteringService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<MLClusteringService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _pythonApiUrl = configuration["MLService:Url"] ?? "http://localhost:5000";

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            _logger.LogInformation($"ML Service URL: {_pythonApiUrl}");
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_pythonApiUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ML Service health check failed: {ex.Message}");
                return false;
            }
        }

        public async Task<ClusterPrediction> PredictAsync(TreEmFeatures features)
        {
            try
            {
                var json = JsonSerializer.Serialize(features, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"Sending prediction request for TreEm ID: {features.TreEmId}");

                var response = await _httpClient.PostAsync(
                    $"{_pythonApiUrl}/predict",
                    content
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"ML API error: {response.StatusCode} - {errorContent}");
                    throw new HttpRequestException($"ML API returned {response.StatusCode}");
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ClusterPrediction>(resultJson, _jsonOptions);

                _logger.LogInformation($"Prediction successful: Cluster={result.Cluster}, Priority={result.PriorityLevel}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PredictAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<BatchPredictionResponse> PredictBatchAsync(List<TreEmFeatures> featuresList)
        {
            try
            {
                var request = new BatchPredictionRequest { Children = featuresList };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"Sending batch prediction for {featuresList.Count} children");

                var response = await _httpClient.PostAsync(
                    $"{_pythonApiUrl}/predict_batch",
                    content
                );

                response.EnsureSuccessStatusCode();

                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<BatchPredictionResponse>(resultJson, _jsonOptions);

                _logger.LogInformation($"Batch prediction completed: {result.Total} results");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PredictBatchAsync: {ex.Message}");
                throw;
            }
        }
    }
}