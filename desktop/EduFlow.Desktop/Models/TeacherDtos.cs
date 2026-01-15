namespace EduFlow.Desktop.Models;

public class AssignmentDto
{
    public string Id { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Topic { get; set; } = "";
    public int GradeLevel { get; set; }
    public string Difficulty { get; set; } = "";
    public string Status { get; set; } = "";
    public string? StudentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudentListDto
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public int GradeLevel { get; set; }
}

public class CreateAssignmentRequestDto
{
    public string Subject { get; set; } = "";
    public string Topic { get; set; } = "";
    public int GradeLevel { get; set; }
    public string Difficulty { get; set; } = "";
    public string TaskText { get; set; } = "";
}

public class AssignRequestDto
{
    public string StudentId { get; set; } = "";
}