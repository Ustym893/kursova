using EduFlow.Desktop.Models;
using EduFlow.Desktop.Services;

namespace EduFlow.Desktop.Views;

public partial class TeacherHomePage : ContentPage
{
    private readonly ApiClient _api;
    private readonly IAuthStore _auth;

    public TeacherHomePage()
    {
        InitializeComponent();
        _api = App.Services.GetRequiredService<ApiClient>();
        _auth = App.Services.GetRequiredService<IAuthStore>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            ErrorLabel.Text = "";
            var me = await _api.GetAsync<TeacherMeResponse>("/api/teacher/me");
            InfoLabel.Text = $"Email: {me.Email}\nInviteCode: {me.InviteCode}";
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.ToString();
            InfoLabel.Text = "Failed to load.";
        }
    }

    private async void OnOpenAssignments(object sender, EventArgs e)
        => await Navigation.PushAsync(new TeacherAssignmentsPage());

    private async void OnOpenStudents(object sender, EventArgs e)
        => await Navigation.PushAsync(new TeacherStudentsPage());

    private void OnLogout(object sender, EventArgs e)
    {
        _auth.Clear();
        Application.Current!.MainPage = new NavigationPage(new RolePage());
    }
}