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
        var userId = User.GetUserId();
        var teacher = await db.TeacherProfiles.FirstOrDefaultAsync(t => t.UserId == userId);
        if (teacher is null) return NotFound();

        var students = await db.StudentProfiles
            .Include(s => s.User)
            .Where(s => s.TeacherId == teacher.Id)
            .OrderBy(s => s.User.Email)
            .Select(s => new { s.Id, email = s.User.Email, s.GradeLevel })
            .ToListAsync();

        return Ok(students);
    }

    [HttpPost("assignments")]
    public async Task<ActionResult<object>> CreateAssignment([FromBody] CreateAssignmentRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Subject) || string.IsNullOrWhiteSpace(req.Topic) || string.IsNullOrWhiteSpace(req.TaskText))
            return BadRequest("Missing fields.");

        var userId = User.GetUserId();
        var teacher = await db.TeacherProfiles.FirstOrDefaultAsync(t => t.UserId == userId