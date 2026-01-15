using EduFlow.Desktop.Models;
using EduFlow.Desktop.Services;

namespace EduFlow.Desktop.Views;

public partial class DashboardPage : ContentPage
{
    private readonly string _role;
    private readonly ApiClient _api;
    private readonly IAuthStore _auth;

    public DashboardPage(string role)
    {
        InitializeComponent();
        _role = role;

        _api = App.Services.GetRequiredService<ApiClient>();
        _auth = App.Services.GetRequiredService<IAuthStore>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            if (_role == "TEACHER")
            {
                var me = await _api.GetAsync<TeacherMeResponse>("/api/teacher/me");
                InfoLabel.Text = $"Teacher: {me.Email}\nInviteCode: {me.InviteCode}";
            }
            else
            {
                var me = await _api.GetAsync<StudentMeResponse>("/api/student/me");
                InfoLabel.Text = $"Student: {me.Email}\nGrade: {me.GradeLevel}";
            }
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Error: {ex.Message}";
        }
    }

    private async void OnLogout(object sender, EventArgs e)
    {
        _auth.Clear();
        await Navigation.PopToRootAsync();
    }
}