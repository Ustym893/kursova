using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EduFlow.Desktop.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IAuthStore _auth;

    public ApiClient(IAuthStore auth)
    {
        _auth = auth;
        _http = new HttpClient();

        // macOS/Windows: if API runs locally, use localhost
        BaseUrl = "http://localhost:5012";
    }

    public string BaseUrl { get; set; }

    public async Task<T> PostAsync<T>(string path, object body)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}{path}");
        AddAuth(req);

        var json = JsonSerializer.Serialize(body);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var res = await _http.SendAsync(req);
        var text = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
            throw new ApiException((int)res.StatusCode, text);

        return JsonSerializer.Deserialize<T>(text, JsonOptions())!;
    }

    public async Task<T> GetAsync<T>(string path)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}{path}");
        AddAuth(req);

        var res = await _http.SendAsync(req);
        var text = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
            throw new ApiException((int)res.StatusCode, text);

        return JsonSerializer.Deserialize<T>(text, JsonOptions())!;
    }

    private void AddAuth(HttpRequestMessage req)
    {
        var token = _auth.Token;
        if (!string.IsNullOrWhiteSpace(token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNameCaseInsensitive = true
    };
}

public class ApiException(int statusCode, string body) : Exception($"API {statusCode}: {body}")
{
    public int StatusCode { get; } = statusCode;
    public string Body { get; } = body;
}