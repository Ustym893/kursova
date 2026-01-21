using EduFlow.Desktop.Models;
using EduFlow.Desktop.Services;

namespace EduFlow.Desktop.Views;

public partial class LoginPage : ContentPage
{
    private readonly string _role;
    private bool _isStudentRegisterMode;
    private bool _isBusy;

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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        StudentModeRow.IsVisible = _role == "STUDENT";
        SetStudentMode(false);
    }

    private void SetStudentMode(bool isRegister)
    {
        _isStudentRegisterMode = isRegister;

        InviteCodeEntry.IsVisible = _role == "STUDENT" && isRegister;
        GradeLevelEntry.IsVisible = _role == "STUDENT" && isRegister;

        SubmitButton.Text = (_role == "STUDENT" && isRegister)
            ? "Register"
            : "Login";

        ErrorLabel.Text = "";
    }

    private void OnStudentLoginMode(object sender, EventArgs e)
        => SetStudentMode(false);

    private void OnStudentRegisterMode(object sender, EventArgs e)
        => SetStudentMode(true);

    private async void OnLogin(object sender, EventArgs e)
    {
        if (_isBusy)
            return;

        _isBusy = true;
        SubmitButton.IsEnabled = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
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

                _auth.Token = res.Token;
                _auth.Role = res.Role;
                await _auth.SaveAsync();

                Application.Current!.MainPage =
                    new NavigationPage(new TeacherHomePage());
                return;
            }

            // STUDENT
            if (_isStudentRegisterMode)
            {
                var invite = InviteCodeEntry.Text?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(invite))
                {
                    ErrorLabel.Text = "Invite code is required.";
                    return;
                }

                if (!int.TryParse(GradeLevelEntry.Text, out var gradeLevel))
                {
                    ErrorLabel.Text = "Grade level must be a number.";
                    return;
                }

                res = await _api.PostAsync<AuthResponse>(
                    "/api/auth/student/register",
                    new StudentRegisterRequest
                    {
                        Email = email,
                        Password = password,
                        InviteCode = invite,
                        GradeLevel = gradeLevel
                    }
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
            await _auth.SaveAsync();

            Application.Current!.MainPage =
                new NavigationPage(new StudentHomePage());
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message;
        }
        finally
        {
            _isBusy = false;
            SubmitButton.IsEnabled = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}