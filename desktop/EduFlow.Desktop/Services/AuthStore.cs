using Microsoft.Maui.Storage;

namespace EduFlow.Desktop.Services;

public interface IAuthStore
{
    string? Token { get; set; }
    string? Role { get; set; }

    Task LoadAsync();
    Task SaveAsync();
    void Clear();
}

public class AuthStore : IAuthStore
{
    private const string TokenKey = "eduflow_token";
    private const string RoleKey = "eduflow_role";

    public string? Token { get; set; }
    public string? Role { get; set; }

    public async Task LoadAsync()
    {
        Token = await SafeGetAsync(TokenKey);
        Role = await SafeGetAsync(RoleKey);
    }

    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Token))
            return;

        await SafeSetAsync(TokenKey, Token);

        if (!string.IsNullOrWhiteSpace(Role))
            await SafeSetAsync(RoleKey, Role);
    }

    public void Clear()
    {
        Token = null;
        Role = null;

        try
        {
            SecureStorage.Default.Remove(TokenKey);
            SecureStorage.Default.Remove(RoleKey);
        }
        catch
        {
            Preferences.Remove(TokenKey);
            Preferences.Remove(RoleKey);
        }
    }

    // ---------- helpers ----------

    private static async Task SafeSetAsync(string key, string value)
    {
        try
        {
            await SecureStorage.Default.SetAsync(key, value);
        }
        catch
        {
            // MacCatalyst fallback
            Preferences.Set(key, value);
        }
    }

    private static async Task<string?> SafeGetAsync(string key)
    {
        try
        {
            return await SecureStorage.Default.GetAsync(key);
        }
        catch
        {
            return Preferences.Get(key, null);
        }
    }
}