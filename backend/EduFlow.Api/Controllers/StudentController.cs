using EduFlow.Api.Auth;
using EduFlow.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Api.Controllers;

public record SubmitRequest(string AnswerText);

[ApiController]
[Route("api/student")]
[Authorize(Roles = "STUDENT")]
public class StudentController(AppDbContext db) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<object>> Me()
    {
        var userId = User.GetUserId();

        var student = await db.StudentProfiles
            .Include(s => s.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student is null) return NotFound();

        return Ok(new
        {
            studentId = student.Id,
            email = student.User.Email,
            gradeLevel = student.GradeLevel,
            teacherId = student.TeacherId
        });
    }

    [HttpGet("assignments")]
    public async Task<ActionResult<object>> Assignments()
    {
        var student = await GetStudentOrNull();
        if (student is null) return NotFound();

        var list = await db.Assignments
            .Where(a => a.StudentId == student.Id)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.Subject,
                a.Topic,
                a.GradeLevel,
                a.Difficulty,
                a.Status,
                a.CreatedAt
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("assignments/{id:guid}")]
    public async Task<ActionResult<object>> AssignmentDetail(Guid id)
    {
        var student = await GetStudentOrNull();
        if (student is null) return NotFound();

        var a = await db.Assignments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.StudentId == student.Id);

        if (a is null) return NotFound();

        var submission = await db.Submissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == a.Id && s.StudentId == student.Id)
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new
            {
                s.Id,
                s.Status,
                s.SubmittedAt,
                s.TeacherGrade,
                s.TeacherComment,
                s.AiScore,
                s.AiFeedback
            })
            .FirstOrDefaultAsync();

        return Ok(new
        {
            a.Id,
            a.Subject,
            a.Topic,
            a.TaskText,
            a.Status,
            a.CreatedAt,
            submission
        });
    }

    [HttpPost("assignments/{id:guid}/submit")]
    public async Task<ActionResult<object>> Submit(Guid id, [FromBody] SubmitRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.AnswerText))
            return BadRequest("AnswerText required.");

        var student = await GetStudentOrNull();
        if (student is null) return NotFound();

        var assignment = await db.Assignments
            .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == student.Id);

        if (assignment is null) return NotFound();

        if (assignment.Status != AssignmentStatus.ASSIGNED)
            return BadRequest("Only ASSIGNED assignments can be submitted.");

        var existing = await db.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.StudentId == student.Id);

        if (existing is not null)
            return Conflict("Already submitted.");

        var submission = new Submission
        {
            AssignmentId = assignment.Id,
            StudentId = student.Id,
            AnswerText = req.AnswerText.Trim(),
            Status = SubmissionStatus.SUBMITTED,
            SubmittedAt = DateTime.UtcNow
        };

        db.Submissions.Add(submission);

        assignment.Status = AssignmentStatus.SUBMITTED;

        await db.SaveChangesAsync();

        return Ok(new { submission.Id, submission.Status, submission.SubmittedAt });
    }

    private async Task<StudentProfile?> GetStudentOrNull()
    {
        var userId = User.GetUserId();
        return await db.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
    }
}