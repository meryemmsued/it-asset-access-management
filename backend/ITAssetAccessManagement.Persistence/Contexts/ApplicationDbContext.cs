using ITAssetAccessManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccessManagement.Persistence.Contexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Team>()
            .HasOne(team => team.Department)
            .WithMany(department => department.Teams)
            .HasForeignKey(team => team.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Team>()
            .HasOne(team => team.TeamLeadUser)
            .WithMany()
            .HasForeignKey(team => team.TeamLeadUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(user => user.Department)
            .WithMany(department => department.Users)
            .HasForeignKey(user => user.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(user => user.Team)
            .WithMany(team => team.Users)
            .HasForeignKey(user => user.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(user => user.Manager)
            .WithMany(user => user.ManagedUsers)
            .HasForeignKey(user => user.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<UserRole>()
            .HasKey(userRole => new
            {
                userRole.UserId,
                userRole.RoleId
            });

        modelBuilder.Entity<UserRole>()
            .HasOne(userRole => userRole.User)
            .WithMany(user => user.UserRoles)
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(userRole => userRole.Role)
            .WithMany(role => role.UserRoles)
            .HasForeignKey(userRole => userRole.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(userRole => userRole.AssignedByUser)
            .WithMany()
            .HasForeignKey(userRole => userRole.AssignedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<RolePermission>()
            .HasKey(rolePermission => new
            {
                rolePermission.RoleId,
                rolePermission.PermissionId
            });

        modelBuilder.Entity<RolePermission>()
            .HasOne(rolePermission => rolePermission.Role)
            .WithMany(role => role.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rolePermission => rolePermission.Permission)
            .WithMany(permission => permission.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(refreshToken => refreshToken.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(refreshToken => refreshToken.ReplacedByToken)
            .WithMany()
            .HasForeignKey(refreshToken => refreshToken.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<LoginAttempt>()
            .HasOne(loginAttempt => loginAttempt.User)
            .WithMany(user => user.LoginAttempts)
            .HasForeignKey(loginAttempt => loginAttempt.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AuditLog>()
            .Property(auditLog => auditLog.OldValues)
            .HasColumnType("jsonb");

        modelBuilder.Entity<AuditLog>()
            .Property(auditLog => auditLog.NewValues)
            .HasColumnType("jsonb");

        modelBuilder.Entity<AuditLog>()
            .HasOne(auditLog => auditLog.User)
            .WithMany(user => user.AuditLogs)
            .HasForeignKey(auditLog => auditLog.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}