using EduFlow.Desktop.Models;
using EduFlow.Desktop.Services;

namespace EduFlow.Desktop.Views;

public partial class LoginPage : ContentPage
{
    private readonly string _role; // "TEACHER" or "STUDENT"

    private bool _isTeacherRegisterMode;
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

        // Show correct toggles
        TeacherModeRow.IsVisible = _role == "TEACHER";
        StudentModeRow.IsVisible = _role == "STUDENT";

        // Default mode
        if (_role == "TEACHER")
            SetTeacherMode(isRegister: false);
        else
            SetStudentMode(isRegister: false);
    }

    private void SetTeacherMode(bool isRegister)
    {
        _isTeacherRegisterMode = isRegister;
        _isStudentRegisterMode = false;

        TeacherDisplayNameEntry.IsVisible = (_role == "TEACHER") && isRegister;
        InviteCodeEntry.IsVisible = false;
        GradeLevelEntry.IsVisible = false;

        SubmitButton.Text = isRegister ? "Register" : "Login";
        ErrorLabel.Text = "";
    }

    private void SetStudentMode(bool isRegister)
    {
        _isStudentRegisterMode = isRegister;
        _isTeacherRegisterMode = false;

        InviteCodeEntry.IsVisible = (_role == "STUDENT") && isRegister;
        GradeLevelEntry.IsVisible = (_role == "STUDENT") && isRegister;
        TeacherDisplayNameEntry.IsVisible = false;

        SubmitButton.Text = isRegister ? "Register" : "Login";
        ErrorLabel.Text = "";
    }

    private void OnTeacherLoginMode(object sender, EventArgs e) => SetTeacherMode(false);
    private void OnTeacherRegisterMode(object sender, EventArgs e) => SetTeacherMode(true);

    private void OnStudentLoginMode(object sender, EventArgs e) => SetStudentMode(false);
    private void OnStudentRegisterMode(object sender, EventArgs e) => SetStudentMode(true);

    private async void OnSubmit(object sender, EventArgs e)
    {
        if (_isBusy) return;

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

            // ==========================
            // TEACHER
            // ==========================
            if (_role == "TEACHER")
            {
                if (_isTeacherRegisterMode)
                {
                    var displayName = TeacherDisplayNameEntry.Text?.Trim();

                    res = await _api.PostAsync<AuthResponse>(
                        "/api/auth/teacher/register",
                        new TeacherRegisterRequest
                        {
                            Email = email,
                            Password = password,
                            DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName
                        }
                    );
                }
                else
                {
                    res = await _api.PostAsync<AuthResponse>(
                        "/api/auth/teacher/login",
                        new TeacherLoginRequest { Email = email, Password = password }
                    );
                }

                _auth.Token = res.Token;
                _auth.Role = res.Role;
                await _auth.SaveAsync();

                Application.Current!.MainPage = new NavigationPage(new TeacherHomePage());
                return;
            }

            // ==========================
            // STUDENT
            // ==========================
            if (_role == "STUDENT")
            {
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

                Application.Current!.MainPage = new NavigationPage(new StudentHomePage());
                return;
            }

            ErrorLabel.Text = "Unknown role.";
        }
        catch (Exception ex)
        {
            // show message (not whole ToString for UI cleanliness)
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