using EduFlow.Desktop.Models;
using EduFlow.Desktop.Services;

namespace EduFlow.Desktop.Views;

public partial class LoginPage : ContentPage
{
    private readonly string _role; // "TEACHER" or "STUDENT"
    private bool _isStudentRegisterMode;

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

        // show register toggle only for students
        StudentModeRow.IsVisible = _role == "STUDENT";

        // default mode
        if (_role == "STUDENT")
            SetStudentMode(isRegister: false);
        else
            SetStudentMode(isRegister: false);
    }

    private void SetStudentMode(bool isRegister)
    {
        _isStudentRegisterMode = isRegister;

        InviteCodeEntry.IsVisible = _role == "STUDENT" && isRegister;
        GradeLevelEntry.IsVisible = _role == "STUDENT" && isRegister;

        // optional UX: change button text
        SubmitButton.Text = (_role == "STUDENT" && isRegister) ? "Register" : "Login";

        ErrorLabel.Text = "";
    }

    private void OnStudentLoginMode(object sender, EventArgs e) => SetStudentMode(isRegister: false);

    private void OnStudentRegisterMode(object sender, EventArgs e) => SetStudentMode(isRegister: true);

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
                // Teacher login
                res = await _api.PostAsync<AuthResponse>(
                    "/api/auth/teacher/login",
                    new TeacherLoginRequest { Email = email, Password = password }
                );

                _auth.Token = res.Token;
                _auth.Role = res.Role;

                Application.Current!.MainPage = new NavigationPage(new TeacherHomePage());
                return;
            }

            // STUDENT: register vs login
            if (_isStudentRegisterMode)
            {
                var invite = InviteCodeEntry.Text?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(invite))
                {
                    ErrorLabel.Text = "Invite code is required for student registration.";
                    return;
                }

                if (!int.TryParse(GradeLevelEntry.Text?.Trim(), out var gradeLevel))
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

            Application.Current!.MainPage = new NavigationPage(new StudentHomePage());
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.ToString();
        }
    }
}