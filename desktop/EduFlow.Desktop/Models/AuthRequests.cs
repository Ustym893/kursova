namespace EduFlow.Desktop.Models;

public class TeacherRegisterRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? DisplayName { get; set; }
}

public class StudentRegisterRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string InviteCode { get; set; } = "";
    public int GradeLevel { get; set; }
}