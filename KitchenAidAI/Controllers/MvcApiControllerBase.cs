using System.Net.Http.Json;
using KitchenAidAI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    public abstract class MvcApiControllerBase : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        protected MvcApiControllerBase(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        protected HttpClient CreateApiClient()
        {
            var client = _httpClientFactory.CreateClient();
            var cookieHeader = Request.Headers["Cookie"].ToString();
            if (!string.IsNullOrWhiteSpace(cookieHeader))
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            return client;
        }

        protected string ApiUrl(string path)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            return $"{baseUrl}{path}";
        }

        protected async Task<ApiResponse<T>?> GetApiResponseAsync<T>(string path)
        {
            using var client = CreateApiClient();
            var response = await client.GetAsync(ApiUrl(path));
            return await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }

        protected async Task<ApiResponse<T>?> PostApiResponseAsync<T>(string path, object payload)
        {
            using var client = CreateApiClient();
            var response = await client.PostAsJsonAsync(ApiUrl(path), payload);
            return await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }

        protected async Task<ApiResponse<T>?> PutApiResponseAsync<T>(string path, object payload)
        {
            using var client = CreateApiClient();
            var response = await client.PutAsJsonAsync(ApiUrl(path), payload);
            return await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }

        protected async Task<ApiResponse<T>?> DeleteApiResponseAsync<T>(string path)
        {
            using var client = CreateApiClient();
            var response = await client.DeleteAsync(ApiUrl(path));
            return await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }

        protected string GetAlertMessage<T>(ApiResponse<T>? response, string fallback)
        {
            return response?.alert?.message ?? fallback;
        }
    }
}
