using ITAssetAccessManagement.Application.DTOs.Assets;
using ITAssetAccessManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetAccessManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AssetsController : BaseApiController
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 10)
    {
        var assets =
            await _assetService.GetAllAsync(
                page,
                pageSize);

        return Ok(assets);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var asset = await _assetService.GetByIdAsync(id);

        if (asset is null)
            return NotFound();

        return Ok(asset);
    }

    [Authorize(Roles = "Admin,IT")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAssetRequest request)
    {
        var userId = GetCurrentUserId();

        var asset = await _assetService.CreateAsync(request, userId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = asset.Id },
            asset);
    }

    [Authorize(Roles = "Admin,IT")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateAssetRequest request)
    {
        var asset = await _assetService.UpdateAsync(id, request);

        if (asset is null)
            return NotFound();

        return Ok(asset);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _assetService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [Authorize(Roles = "Admin,IT")]
    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> Assign(
        Guid id,
        AssignAssetRequest request)
    {
        var userId = GetCurrentUserId();

        var success = await _assetService.AssignAsync(
            id,
            request,
            userId);

        if (!success)
            return BadRequest();

        return Ok();
    }

    [Authorize(Roles = "Admin,IT")]
    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> Return(
        Guid id,
        ReturnAssetRequest request)
    {
        var success = await _assetService.ReturnAsync(id, request);

        if (!success)
            return BadRequest();

        return Ok();
    }

    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> History(Guid id)
    {
        var history = await _assetService.GetStatusHistoryAsync(id);

        return Ok(history);
    }
}