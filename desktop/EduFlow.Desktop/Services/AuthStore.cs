namespace EduFlow.Desktop.Services;

public interface IAuthStore
{
    string? Token { get; set; }
    string? Role { get; set; }
    void Clear();
}

public class AuthStore : IAuthStore
{
    public string? Token { get; set; }
    public string? Role { get; set; }

    public void Clear()
    {
        Token = null;
        Role = null;
    }
}