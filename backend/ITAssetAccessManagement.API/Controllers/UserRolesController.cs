using ITAssetAccessManagement.Application.DTOs.Users;
using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccessManagement.API.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/roles")]
[Authorize(Roles = "Admin")]
public sealed class UserRolesController : BaseApiController
{
    private readonly ApplicationDbContext _dbContext;

    public UserRolesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserRoles(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.Id == userId,
                cancellationToken);

        if (!userExists)
        {
            return NotFound(new
            {
                Message = "User not found."
            });
        }

        var roles = await _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == userId)
            .OrderBy(userRole => userRole.Role.Name)
            .Select(userRole => new UserRoleResponse
            {
                RoleId = userRole.RoleId,
                RoleName = userRole.Role.Name,
                AssignedAt = userRole.AssignedAt,
                AssignedByUserId = userRole.AssignedByUserId
            })
            .ToListAsync(cancellationToken);

        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> AssignRole(
        Guid userId,
        AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Users
            .AnyAsync(
                user => user.Id == userId,
                cancellationToken);

        if (!userExists)
        {
            return NotFound(new
            {
                Message = "User not found."
            });
        }

        var role = await _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                role => role.Id == request.RoleId,
                cancellationToken);

        if (role is null)
        {
            return NotFound(new
            {
                Message = "Role not found."
            });
        }

        var assignmentExists = await _dbContext.UserRoles
            .AnyAsync(
                userRole =>
                    userRole.UserId == userId &&
                    userRole.RoleId == request.RoleId,
                cancellationToken);

        if (assignmentExists)
        {
            return Conflict(new
            {
                Message = "The user already has this role."
            });
        }

        var assignedByUserId = GetCurrentUserId();

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = request.RoleId,
            AssignedByUserId = assignedByUserId,
            AssignedAt = DateTime.UtcNow
        };

        _dbContext.UserRoles.Add(userRole);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new UserRoleResponse
        {
            RoleId = role.Id,
            RoleName = role.Name,
            AssignedAt = userRole.AssignedAt,
            AssignedByUserId = userRole.AssignedByUserId
        };

        return Ok(response);
    }

    [HttpDelete("{roleId:guid}")]
    public async Task<IActionResult> RemoveRole(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var userRole = await _dbContext.UserRoles
            .Include(userRole => userRole.Role)
            .FirstOrDefaultAsync(
                userRole =>
                    userRole.UserId == userId &&
                    userRole.RoleId == roleId,
                cancellationToken);

        if (userRole is null)
        {
            return NotFound(new
            {
                Message = "The user does not have this role."
            });
        }

        var userRoleCount = await _dbContext.UserRoles
            .CountAsync(
                item => item.UserId == userId,
                cancellationToken);

        if (userRoleCount == 1)
        {
            return BadRequest(new
            {
                Message = "A user's last role cannot be removed."
            });
        }

        _dbContext.UserRoles.Remove(userRole);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}