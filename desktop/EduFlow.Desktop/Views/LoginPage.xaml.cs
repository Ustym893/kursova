using EduFlow.Desktop.Models;
using EduFlow.Desktop.Services;

namespace EduFlow.Desktop.Views;

public partial class LoginPage : ContentPage
{
    private readonly string _role;
    private readonly ApiClient _api;
    private readonly IAuthStore _auth;

    public LoginPage(string role)
    {
        InitializeComponent();
        _role = role;
        RoleLabel.Text = $"Login as {_role}";

        _api = App.Services.GetRequiredService<ApiClient>();
        _auth = App.Services.GetRequiredService<IAuthStore>();
    }

    private async void OnLogin(object sender, EventArgs e)
    {
        ErrorLabel.Text = "";

        try
        {
            var email = EmailEntry.Text?.Trim() ?? "";
            var password = PasswordEntry.Text ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorLabel.Text = "Enter email and password.";
                return;
            }

            AuthResponse res;

            if (_role == "TEACHER")
            {
                res = await _api.PostAsync<AuthResponse>(
                    "/api/auth/teacher/login",
                    new TeacherLoginRequest { Email = email, Password = password }
                );
            }
            else
            {
                res = await _api.PostAsync<AuthResponse>(
                    "/api/auth/student/login",
                    new StudentLoginRequest { Email = email, Password = password }
                );
            }

            _auth.Token = res.Token;
            _auth.Role = res.Role;

            await Navigation.PushAsync(new DashboardPage(_role));
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message;
        }
    }
}