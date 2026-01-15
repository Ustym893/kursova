using EduFlow.Desktop.Models;
using EduFlow.Desktop.Services;

namespace EduFlow.Desktop.Views;

public partial class StudentHomePage : ContentPage
{
    private readonly ApiClient _api;
    private readonly IAuthStore _auth;

    public StudentHomePage()
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
            var me = await _api.GetAsync<StudentMeResponse>("/api/student/me");
            InfoLabel.Text = $"Email: {me.Email}\nGradeLevel: {me.GradeLevel}\nTeacherId: {me.TeacherId}";
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.ToString();
            InfoLabel.Text = "Failed to load.";
        }
    }

    private async void OnOpenAssignments(object sender, EventArgs e)
        => await Navigation.PushAsync(new StudentAssignmentsPage());

    private void OnLogout(object sender, EventArgs e)
    {
        _auth.Clear();
        Application.Current!.MainPage = new NavigationPage(new RolePage());
    }
}