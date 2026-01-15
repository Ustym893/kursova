namespace EduFlow.Desktop.Models;

public class StudentRegisterRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string InviteCode { get; set; } = "";
    public int GradeLevel { get; set; }
}