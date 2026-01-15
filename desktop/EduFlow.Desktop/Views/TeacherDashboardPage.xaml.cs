using EduFlow.Desktop.Models;
using EduFlow.Desktop.Services;

namespace EduFlow.Desktop.Views;

public partial class TeacherDashboardPage : ContentPage
{
    private readonly ApiClient _api;

    public TeacherDashboardPage()
    {
        InitializeComponent();
        _api = App.Services.GetRequiredService<ApiClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var me = await _api.GetAsync<TeacherMeResponse>("/api/teacher/me");
            InfoLabel.Text = $"Teacher: {me.Email}\nInviteCode: {me.InviteCode}";
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Error: {ex.Message}";
        }
    }
}