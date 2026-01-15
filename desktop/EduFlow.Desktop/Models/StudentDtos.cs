namespace EduFlow.Desktop.Models;

public class StudentAssignmentDto
{
    public string Id { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Topic { get; set; } = "";
    public string TaskText { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class SubmitAssignmentRequestDto
{
    public string AnswerText { get; set; } = "";
}