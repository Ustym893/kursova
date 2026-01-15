namespace EduFlow.Desktop.Models;

public class AuthResponse
{
    public string Token { get; set; } = "";
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? InviteCode { get; set; }
    public string? TeacherId { get; set; }
    public string? StudentId { get; set; }
}

public class TeacherLoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class StudentLoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class TeacherMeResponse
{
    public string TeacherId { get; set; } = "";
    public string Email { get; set; } = "";
    public string? DisplayName { get; set; }
    public string InviteCode { get; set; } = "";
}

public class StudentMeResponse
{
    public string StudentId { get; set; } = "";
    public string Email { get; set; } = "";
    public int GradeLevel { get; set; }
    public string TeacherId { get; set; } = "";
} 