using ITAssetAccessManagement.Application.DTOs.Roles;
using ITAssetAccessManagement.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITAssetAccessManagement.Domain.Entities;

namespace ITAssetAccessManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public sealed class RolesController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public RolesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles(
        CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole
            })
            .ToListAsync(cancellationToken);

        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(
        CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var roleName = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(roleName))
        {
            return BadRequest(new
            {
                Message = "Role name is required."
            });
        }

        var roleExists = await _dbContext.Roles
            .AnyAsync(
                role => role.Name.ToLower() == roleName.ToLower(),
                cancellationToken);

        if (roleExists)
        {
            return Conflict(new
            {
                Message = "A role with this name already exists."
            });
        }

        var now = DateTime.UtcNow;

        var role = new Role
        {
            Name = roleName,
            Description = request.Description?.Trim(),
            IsSystemRole = false,
            CreatedAt = now,
            UpdatedAt = now
        };
        _dbContext.Roles.Add(role);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole
        };

        return CreatedAtAction(
            nameof(GetRoleById),
            new { id = role.Id },
            response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRoleById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles
            .AsNoTracking()
            .Where(role => role.Id == id)
            .Select(role => new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (role is null)
        {
            return NotFound(new
            {
                Message = "Role not found."
            });
        }

        return Ok(role);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(
                role => role.Id == id,
                cancellationToken);

        if (role is null)
        {
            return NotFound(new
            {
                Message = "Role not found."
            });
        }

        var roleName = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(roleName))
        {
            return BadRequest(new
            {
                Message = "Role name is required."
            });
        }

        var duplicateExists = await _dbContext.Roles
            .AnyAsync(
                otherRole =>
                    otherRole.Id != id &&
                    otherRole.Name.ToLower() == roleName.ToLower(),
                cancellationToken);

        if (duplicateExists)
        {
            return Conflict(new
            {
                Message = "A role with this name already exists."
            });
        }

        role.Name = roleName;
        role.Description = request.Description?.Trim();
        role.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole
        };

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRole(
        Guid id,
        CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(
                role => role.Id == id,
                cancellationToken);

        if (role is null)
        {
            return NotFound(new
            {
                Message = "Role not found."
            });
        }

        if (role.IsSystemRole)
        {
            return BadRequest(new
            {
                Message = "System roles cannot be deleted."
            });
        }

        var isAssignedToAnyUser = await _dbContext.UserRoles
            .AnyAsync(
                userRole => userRole.RoleId == id,
                cancellationToken);

        if (isAssignedToAnyUser)
        {
            return Conflict(new
            {
                Message = "This role is assigned to one or more users and cannot be deleted."
            });
        }

        _dbContext.Roles.Remove(role);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}