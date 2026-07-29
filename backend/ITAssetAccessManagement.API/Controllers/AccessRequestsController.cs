using System.Security.Claims;
using ITAssetAccessManagement.Application.DTOs.AccessRequests;
using ITAssetAccessManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetAccessManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AccessRequestsController : ControllerBase
{
    private readonly IAccessRequestService _service;

    public AccessRequestsController(IAccessRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests()
    {
        var userId = GetCurrentUserId();

        var result = await _service.GetByUserAsync(userId);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAccessRequestRequest request)
    {
        var userId = GetCurrentUserId();

        var result = await _service.CreateAsync(request, userId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid id,
        ApproveAccessRequestRequest request)
    {
        var approverId = GetCurrentUserId();

        var success =
            await _service.ApproveAsync(id, request, approverId);

        if (!success)
            return BadRequest();

        return Ok();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        RejectAccessRequestRequest request)
    {
        var approverId = GetCurrentUserId();

        var success =
            await _service.RejectAsync(id, request, approverId);

        if (!success)
            return BadRequest();

        return Ok();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetCurrentUserId();

        var success =
            await _service.CancelAsync(id, userId);

        if (!success)
            return BadRequest();

        return Ok();
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException();

        return Guid.Parse(userId);
    }
}