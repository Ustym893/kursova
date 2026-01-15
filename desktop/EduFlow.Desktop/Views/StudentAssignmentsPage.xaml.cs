using EduFlow.Desktop.Models;
using EduFlow.Desktop.Services;

namespace EduFlow.Desktop.Views;

public partial class StudentAssignmentsPage : ContentPage
{
    private readonly ApiClient _api;
    private bool _loaded;

    public StudentAssignmentsPage()
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
            var items = await _api.GetAsync<List<StudentAssignmentDto>>("/api/student/assignments");
            List.ItemsSource = items;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.ToString();
        }
    }

    private async void OnOpen(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.BindingContext is not StudentAssignmentDto a) return;
        await Navigation.PushAsync(new StudentAssignmentDetailPage(a.Id));
    }
}