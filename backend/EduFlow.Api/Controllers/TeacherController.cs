using EduFlow.Api.Auth;
using EduFlow.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Api.Controllers;

public record CreateAssignmentRequest(string Subject, string Topic, int GradeLevel, string Difficulty, string TaskText);
public record AssignRequest(Guid StudentId);
public record ReviewSubmissionRequest(int TeacherGrade, string? TeacherComment);

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "TEACHER")]
public class TeacherController(AppDbContext db) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<object>> Me()
    {
        var userId = User.GetUserId();
        var teacher = await db.TeacherProfiles
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (teacher is null) return NotFound();

        return Ok(new
        {
            teacherId = teacher.Id,
            email = teacher.User.Email,
            displayName = teacher.DisplayName,
            inviteCode = teacher.InviteCode
        });
    }

    [HttpGet("students")]
    public async Task<ActionResult<object>> Students()
    {
        var teacher = await GetTeacherOrNull();
        if (teacher is null) return NotFound();

        var students = await db.StudentProfiles
            .Include(s => s.User)
            .Where(s => s.TeacherId == teacher.Id)
            .OrderBy(s => s.User.Email)
            .Select(s => new { s.Id, email = s.User.Email, s.GradeLevel })
            .ToListAsync();

        return Ok(students);
    }

    [HttpGet("students/{studentId:guid}")]
    public async Task<ActionResult<object>> StudentDetail(Guid studentId)
    {
        var teacher = await GetTeacherOrNull();
        if (teacher is null) return NotFound();

        var student = await db.StudentProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == studentId && s.TeacherId == teacher.Id);

        if (student is null) return NotFound();

        var assignments = await db.Assignments
            .Where(a => a.TeacherId == teacher.Id && a.StudentId == student.Id)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new { a.Id, a.Subject, a.Topic, a.Status, a.CreatedAt })
            .ToListAsync();

        var submissions = await db.Submissions
            .Include(s => s.Assignment)
            .Where(s => s.StudentId == student.Id && s.Assignment.TeacherId == teacher.Id)
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new { s.Id, s.AssignmentId, s.Status, s.SubmittedAt, s.TeacherGrade })
            .ToListAsync();

        return Ok(new
        {
            student = new { student.Id, email = student.User.Email, student.GradeLevel },
            assignments,
            submissions
        });
    }

    [HttpPost("assignments")]
    public async Task<ActionResult<object>> CreateAssignment([FromBody] CreateAssignmentRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Subject) ||
            string.IsNullOrWhiteSpace(req.Topic) ||
            string.IsNullOrWhiteSpace(req.TaskText))
        {
            return BadRequest("Missing fields.");
        }

        var teacher = await GetTeacherOrNull();
        if (teacher is null) return NotFound();

        var a = new Assignment
        {
            TeacherId = teacher.Id,
            Subject = req.Subject.Trim(),
            Topic = req.Topic.Trim(),
            GradeLevel = req.GradeLevel,
            Difficulty = string.IsNullOrWhiteSpace(req.Difficulty) ? "Medium" : req.Difficulty.Trim(),
            TaskText = req.TaskText.Trim(),
            Status = AssignmentStatus.CREATED
        };

        db.Assignments.Add(a);
        await db.SaveChangesAsync();

        return Ok(new { a.Id, a.Status, a.CreatedAt });
    }

    [HttpGet("assignments")]
    public async Task<ActionResult<object>> GetAssignments([FromQuery] string? status)
    {
        var teacher = await GetTeacherOrNull();
        if (teacher is null) return NotFound();

        var query = db.Assignments
            .Where(a => a.TeacherId == teacher.Id)
            .OrderByDescending(a => a.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<AssignmentStatus>(status, true, out var st))
        {
            query = query.Where(a => a.Status == st);
        }

        var list = await query
            .Select(a => new
            {
                a.Id,
                a.Subject,
                a.Topic,
                a.GradeLevel,
                a.Difficulty,
                a.Status,
                a.StudentId,
                a.CreatedAt
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost("assignments/{id:guid}/assign")]
    public async Task<ActionResult<object>> Assign(Guid id, [FromBody] AssignRequest req)
    {
        var teacher = await GetTeacherOrNull();
        if (teacher is null) return NotFound();

        var assignment = await db.Assignments
            .FirstOrDefaultAsync(a => a.Id == id && a.TeacherId == teacher.Id);

        if (assignment is null) return NotFound();

        if (assignment.Status != AssignmentStatus.CREATED)
            return BadRequest("Only CREATED assignments can be assigned.");

        var student = await db.StudentProfiles
            .FirstOrDefaultAsync(s => s.Id == req.StudentId && s.TeacherId == teacher.Id);

        if (student is null)
            return BadRequest("Student not found for this teacher.");

        assignment.StudentId = student.Id;
        assignment.Status = AssignmentStatus.ASSIGNED;

        await db.SaveChangesAsync();

        return Ok(new { assignment.Id, assignment.Status, assignment.StudentId });
    }

    [HttpGet("submissions")]
    public async Task<ActionResult<object>> Submissions()
    {
        var teacher = await GetTeacherOrNull();
        if (teacher is null) return NotFound();

        var list = await db.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.Student).ThenInclude(st => st.User)
            .Where(s => s.Assignment.TeacherId == teacher.Id)
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new
            {
                s.Id,
                assignmentId = s.AssignmentId,
                studentId = s.StudentId,
                studentEmail = s.Student.User.Email,
                subject = s.Assignment.Subject,
                topic = s.Assignment.Topic,
                s.Status,
                s.SubmittedAt,
                s.TeacherGrade
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("submissions/{id:guid}")]
    public async Task<ActionResult<object>> SubmissionDetail(Guid id)
    {
        var teacher = await GetTeacherOrNull();
        if (teacher is null) return NotFound();

        var s = await db.Submissions
            .Include(x => x.Assignment)
            .Include(x => x.Student).ThenInclude(st => st.User)
            .FirstOrDefaultAsync(x => x.Id == id && x.Assignment.TeacherId == teacher.Id);

        if (s is null) return NotFound();

        return Ok(new
        {
            s.Id,
            s.AssignmentId,
            studentEmail = s.Student.User.Email,
            assignmentText = s.Assignment.TaskText,
            s.AnswerText,
            s.SubmittedAt,
            s.AiScore,
            s.AiFeedback,
            s.TeacherGrade,
            s.TeacherComment,
            s.Status
        });
    }

    [HttpPost("submissions/{id:guid}/review")]
    public async Task<ActionResult<object>> Review(Guid id, [FromBody] ReviewSubmissionRequest req)
    {
        if (req.TeacherGrade < 0 || req.TeacherGrade > 100)
            return BadRequest("TeacherGrade must be 0-100.");

        var teacher = await GetTeacherOrNull();
        if (teacher is null) return NotFound();

        var s = await db.Submissions
            .Include(x => x.Assignment)
            .FirstOrDefaultAsync(x => x.Id == id && x.Assignment.TeacherId == teacher.Id);

        if (s is null) return NotFound();

        if (s.Status != SubmissionStatus.SUBMITTED)
            return BadRequest("Only SUBMITTED submissions can be reviewed.");

        s.TeacherGrade = req.TeacherGrade;
        s.TeacherComment = string.IsNullOrWhiteSpace(req.TeacherComment) ? null : req.TeacherComment.Trim();
        s.Status = SubmissionStatus.REVIEWED;

        // Keep assignment in sync
        s.Assignment.Status = AssignmentStatus.REVIEWED;

        await db.SaveChangesAsync();

        return Ok(new { s.Id, s.Status, s.TeacherGrade });
    }

    private async Task<TeacherProfile?> GetTeacherOrNull()
    {
        var userId = User.GetUserId();
        return await db.TeacherProfiles.FirstOrDefaultAsync(t => t.UserId == userId);
    }
}