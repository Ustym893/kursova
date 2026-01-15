using System.ComponentModel.DataAnnotations;

namespace EduFlow.Api.Data;

public enum UserRole { TEACHER, STUDENT, ADMIN }
public enum AssignmentStatus { CREATED, ASSIGNED, SUBMITTED, REVIEWED }
public enum SubmissionStatus { SUBMITTED, REVIEWED }

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(320)]
    public string Email { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.STUDENT;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TeacherProfile? TeacherProfile { get; set; }
    public StudentProfile? StudentProfile { get; set; }
}

public class TeacherProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    [MaxLength(32)]
    public string InviteCode { get; set; } = default!;

    [MaxLength(120)]
    public string? DisplayName { get; set; }

    public List<StudentProfile> Students { get; set; } = new();
    public List<Assignment> Assignments { get; set; } = new();
}

public class StudentProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid TeacherId { get; set; }
    public TeacherProfile Teacher { get; set; } = default!;

    public int GradeLevel { get; set; }

    public List<Submission> Submissions { get; set; } = new();
    public List<Assignment> Assignments { get; set; } = new();
}

public class Assignment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TeacherId { get; set; }
    public TeacherProfile Teacher { get; set; } = default!;

    public Guid? StudentId { get; set; }
    public StudentProfile? Student { get; set; }

    [MaxLength(80)]
    public string Subject { get; set; } = default!;

    [MaxLength(120)]
    public string Topic { get; set; } = default!;

    public int GradeLevel { get; set; }

    [MaxLength(40)]
    public string Difficulty { get; set; } = "Medium";

    public string TaskText { get; set; } = default!;

    public AssignmentStatus Status { get; set; } = AssignmentStatus.CREATED;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Submission> Submissions { get; set; } = new();
}

public class Submission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = default!;

    public Guid StudentId { get; set; }
    public StudentProfile Student { get; set; } = default!;

    public string AnswerText { get; set; } = default!;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public int? AiScore { get; set; }
    public int? TeacherGrade { get; set; }
    public string? TeacherComment { get; set; }
    public string? AiFeedback { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.SUBMITTED;
}