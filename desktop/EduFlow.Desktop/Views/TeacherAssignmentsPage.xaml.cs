using EduFlow.Desktop.Models;
using EduFlow.Desktop.Services;

namespace EduFlow.Desktop.Views;

public partial class TeacherAssignmentsPage : ContentPage
{
    private readonly ApiClient _api;
    private bool _loaded;

    public TeacherAssignmentsPage()
    {
        InitializeComponent();
        _api = App.Services.GetRequiredService<ApiClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded) return;
        _loaded = true;
        await LoadAssignments();
    }

    private async void OnRefresh(object sender, EventArgs e) => await LoadAssignments();

    private async Task LoadAssignments()
    {
        ErrorLabel.Text = "";
        try
        {
            var items = await _api.GetAsync<List<AssignmentDto>>("/api/teacher/assignments");
            List.ItemsSource = items;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.ToString();
        }
    }

    private async void OnCreate(object sender, EventArgs e)
    {
        CreateError.Text = "";
        try
        {
            var subject = SubjectEntry.Text?.Trim() ?? "";
            var topic = TopicEntry.Text?.Trim() ?? "";
            var diff = DifficultyEntry.Text?.Trim() ?? "Medium";
            var task = TaskEditor.Text?.Trim() ?? "";

            if (!int.TryParse(GradeEntry.Text?.Trim(), out var grade))
            {
                CreateError.Text = "GradeLevel must be a number.";
                return;
            }

            await _api.PostAsync<object>("/api/teacher/assignments", new CreateAssignmentRequestDto
            {
                Subject = subject,
                Topic = topic,
                GradeLevel = grade,
                Difficulty = diff,
                TaskText = task
            });

            await LoadAssignments();
        }
        catch (Exception ex)
        {
            CreateError.Text = ex.ToString();
        }
    }

    private async void OnAssignClicked(object sender, EventArgs e)
    {
        ErrorLabel.Text = "";
        try
        {
            if (sender is not Button btn) return;
            if (btn.BindingContext is not AssignmentDto a) return;

            if (!string.Equals(a.Status, "CREATED", StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert("Cannot assign", "Only CREATED assignments can be assigned.", "OK");
                return;
            }

            var students = await _api.GetAsync<List<StudentListDto>>("/api/teacher/students");
            if (students.Count == 0)
            {
                await DisplayAlert("No students", "Register a student first using invite code.", "OK");
                return;
            }

            var labels = students.Select(s => $"{s.Email} (Grade {s.GradeLevel})").ToArray();
            var chosen = await DisplayActionSheet("Assign to student", "Cancel", null, labels);
            if (chosen is null || chosen == "Cancel") return;

            var picked = students.First(s => $"{s.Email} (Grade {s.GradeLevel})" == chosen);

            await _api.PostAsync<object>($"/api/teacher/assignments/{a.Id}/assign", new AssignRequestDto
            {
                StudentId = picked.Id
            });

            await LoadAssignments();
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.ToString();
        }
    }
}