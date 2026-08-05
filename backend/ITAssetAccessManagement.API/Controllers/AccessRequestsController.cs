using ITAssetAccessManagement.Application.DTOs.AccessRequests;
using ITAssetAccessManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetAccessManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AccessRequestsController : BaseApiController
{
    private readonly IAccessRequestService _service;

    public AccessRequestsController(
        IAccessRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Team Lead")]
    public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 10)
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        var result =
            await _service.GetVisibleRequestsAsync(
                currentUserId,
                isAdmin,
                page,
                pageSize);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        var currentUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        var canView = await _service.CanViewRequestAsync(
            id,
            currentUserId,
            isAdmin);

        if (!canView)
            return Forbid();

        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests(
        int page = 1,
        int pageSize = 10)
    {
        var userId = GetCurrentUserId();

        var result = await _service.GetByUserAsync(
            userId,
            page,
            pageSize);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAccessRequestRequest request)
    {
        var userId = GetCurrentUserId();

        var result = await _service.CreateAsync(
            request,
            userId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Team Lead")]
    public async Task<IActionResult> Approve(
        Guid id,
        ApproveAccessRequestRequest request)
    {
        var approverId = GetCurrentUserId();

        var success = await _service.ApproveAsync(
            id,
            request,
            approverId);

        if (!success)
            return BadRequest();

        return Ok();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Team Lead")]
    public async Task<IActionResult> Reject(
        Guid id,
        RejectAccessRequestRequest request)
    {
        var approverId = GetCurrentUserId();

        var success = await _service.RejectAsync(
            id,
            request,
            approverId);

        if (!success)
            return BadRequest();

        return Ok();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetCurrentUserId();

        var success = await _service.CancelAsync(
            id,
            userId);

        if (!success)
            return BadRequest();

        return Ok();
    }

}