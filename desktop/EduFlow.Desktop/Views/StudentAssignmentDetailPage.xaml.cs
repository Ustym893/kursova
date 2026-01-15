using EduFlow.Desktop.Models;
using EduFlow.Desktop.Services;

namespace EduFlow.Desktop.Views;

public partial class StudentAssignmentDetailPage : ContentPage
{
    private readonly ApiClient _api;
    private readonly string _id;
    private StudentAssignmentDto? _a;

    public StudentAssignmentDetailPage(string assignmentId)
    {
        InitializeComponent();
        _api = App.Services.GetRequiredService<ApiClient>();
        _id = assignmentId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Load();
    }

    private async Task Load()
    {
        ErrorLabel.Text = "";
        OkLabel.Text = "";
        try
        {
            _a = await _api.GetAsync<StudentAssignmentDto>($"/api/student/assignments/{_id}");
            HeaderLabel.Text = $"{_a.Subject} • {_a.Topic} • {_a.Status}";
            TaskLabel.Text = _a.TaskText;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.ToString();
        }
    }

    private async void OnSubmit(object sender, EventArgs e)
    {
        ErrorLabel.Text = "";
        OkLabel.Text = "";

        try
        {
            var answer = AnswerEditor.Text?.Trim() ?? "";
            if (answer.Length < 3)
            {
                ErrorLabel.Text = "Write an answer first.";
                return;
            }

            await _api.PostAsync<object>($"/api/student/assignments/{_id}/submit", new SubmitAssignmentRequestDto
            {
                AnswerText = answer
            });

            OkLabel.Text = "Submitted ✅";
            await Load();
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.ToString();
        }
    }
}