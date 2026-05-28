using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace VetMed.App.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonSerializerOptions _serializeOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ApiClient(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public void SetAuthToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthToken()
    {
        _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<T?> GetAsync<T>(string path)
    {
        var response = await _http.GetAsync(path);
        if (!response.IsSuccessStatusCode) return default;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<T?> PostAsync<T>(string path, object body)
    {
        var content = new StringContent(JsonSerializer.Serialize(body, _serializeOptions), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync(path, content);
        if (!response.IsSuccessStatusCode) return default;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<T?> PutAsync<T>(string path, object body)
    {
        var content = new StringContent(JsonSerializer.Serialize(body, _serializeOptions), Encoding.UTF8, "application/json");
        var response = await _http.PutAsync(path, content);
        if (!response.IsSuccessStatusCode) return default;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<bool> DeleteAsync(string path)
    {
        var response = await _http.DeleteAsync(path);
        return response.IsSuccessStatusCode;
    }
}
