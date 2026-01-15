using Microsoft.EntityFrameworkCore;

namespace EduFlow.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<TeacherProfile> TeacherProfiles => Set<TeacherProfile>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>();
        });

        modelBuilder.Entity<TeacherProfile>(e =>
        {
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasIndex(x => x.InviteCode).IsUnique();
        });

        modelBuilder.Entity<StudentProfile>(e =>
        {
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasIndex(x => new { x.TeacherId, x.GradeLevel });
        });

        modelBuilder.Entity<Assignment>(e =>
        {
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => new { x.TeacherId, x.Status, x.CreatedAt });
            e.HasIndex(x => new { x.StudentId, x.Status, x.CreatedAt });
        });

        modelBuilder.Entity<Submission>(e =>
        {
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => new { x.StudentId, x.SubmittedAt });
            e.HasIndex(x => new { x.AssignmentId, x.SubmittedAt });
        });
    }
}