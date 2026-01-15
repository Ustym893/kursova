namespace EduFlow.Api.Auth;

public record TeacherRegisterRequest(string Email, string Password, string? DisplayName);
public record StudentRegisterRequest(string Email, string Password, string InviteCode, int GradeLevel);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    Guid UserId,
    string Email,
    string Role,
    string? InviteCode,
    Guid? TeacherId,
    Guid? StudentId
);