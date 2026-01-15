using EduFlow.Desktop.Models;
using EduFlow.Desktop.Services;

namespace EduFlow.Desktop.Views;

public partial class TeacherStudentsPage : ContentPage
{
    private readonly ApiClient _api;
    private bool _loaded;

    public TeacherStudentsPage()
    {
        InitializeComponent();
        _api = App.Services.GetRequiredService<ApiClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded) return;
        _loaded = true;
        await Load();
    }

    private async void OnRefresh(object sender, EventArgs e) => await Load();

    private async Task Load()
    {
        ErrorLabel.Text = "";
        try
        {
            var students = await _api.GetAsync<List<StudentListDto>>("/api/teacher/students");
            List.ItemsSource = students;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.ToString();
        }
    }
}