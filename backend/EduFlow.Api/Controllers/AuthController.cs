using BCrypt.Net;
using EduFlow.Api.Auth;
using EduFlow.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, JwtService jwt) : ControllerBase
{
    [HttpPost("teacher/register")]
    public async Task<ActionResult<AuthResponse>> TeacherRegister([FromBody] TeacherRegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || req.Password.Length < 6)
            return BadRequest("Invalid email or password (min 6 chars).");

        if (await db.Users.AnyAsync(u => u.Email == email))
            return Conflict("Email already registered.");

        var inviteCode = await GenerateUniqueInviteCode();

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = UserRole.TEACHER
        };

        var teacher = new TeacherProfile
        {
            User = user,
            InviteCode = inviteCode,
            DisplayName = string.IsNullOrWhiteSpace(req.DisplayName) ? null : req.DisplayName.Trim()
        };

        db.Users.Add(user);
        db.TeacherProfiles.Add(teacher);
        await db.SaveChangesAsync();

        var token = jwt.CreateToken(user);

        return Ok(new AuthResponse(
            token, user.Id, user.Email, user.Role.ToString(),
            InviteCode: teacher.InviteCode,
            TeacherId: teacher.Id,
            StudentId: null
        ));
    }

    [HttpPost("teacher/login")]
    public async Task<ActionResult<AuthResponse>> TeacherLogin([FromBody] LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await db.Users
            .Include(u => u.TeacherProfile)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || user.Role != UserRole.TEACHER) return Unauthorized("Invalid credentials.");
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)) return Unauthorized("Invalid credentials.");

        var token = jwt.CreateToken(user);
        return Ok(new AuthResponse(
            token, user.Id, user.Email, user.Role.ToString(),
            InviteCode: user.TeacherProfile!.InviteCode,
            TeacherId: user.TeacherProfile!.Id,
            StudentId: null
        ));
    }

    [HttpPost("student/register")]
    public async Task<ActionResult<AuthResponse>> StudentRegister([FromBody] StudentRegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || req.Password.Length < 6)
            return BadRequest("Invalid email or password (min 6 chars).");

        var teacher = await db.TeacherProfiles.FirstOrDefaultAsync(t => t.InviteCode == req.InviteCode.Trim());
        if (teacher is null) return BadRequest("Invalid invite code.");

        if (await db.Users.AnyAsync(u => u.Email == email))
            return Conflict("Email already registered.");

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = UserRole.STUDENT
        };

        var student = new StudentProfile
        {
            User = user,
            TeacherId = teacher.Id,
            GradeLevel = req.GradeLevel
        };

        db.Users.Add(user);
        db.StudentProfiles.Add(student);
        await db.SaveChangesAsync();

        var token = jwt.CreateToken(user);
        return Ok(new AuthResponse(
            token, user.Id, user.Email, user.Role.ToString(),
            InviteCode: null,
            TeacherId: teacher.Id,
            StudentId: student.Id
        ));
    }

    [HttpPost("student/login")]
    public async Task<ActionResult<AuthResponse>> StudentLogin([FromBody] LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await db.Users
            .Include(u => u.StudentProfile)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || user.Role != UserRole.STUDENT) return Unauthorized("Invalid credentials.");
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)) return Unauthorized("Invalid credentials.");

        var token = jwt.CreateToken(user);
        return Ok(new AuthResponse(
            token, user.Id, user.Email, user.Role.ToString(),
            InviteCode: null,
            TeacherId: user.StudentProfile!.TeacherId,
            StudentId: user.StudentProfile!.Id
        ));
    }

    private async Task<string> GenerateUniqueInviteCode()
    {
        // 8 chars, easy to type
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rng = Random.Shared;

        for (var attempts = 0; attempts < 20; attempts++)
        {
            var code = new string(Enumerable.Range(0, 8).Select(_ => alphabet[rng.Next(alphabet.Length)]).ToArray());
            var exists = await db.TeacherProfiles.AnyAsync(t => t.InviteCode == code);
            if (!exists) return code;
        }

        // fallback (very unlikely)
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }
}