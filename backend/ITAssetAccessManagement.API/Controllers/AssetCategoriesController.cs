using ITAssetAccessManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetAccessManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AssetCategoriesController : ControllerBase
{
    private readonly IAssetCategoryService _service;

    public AssetCategoriesController(IAssetCategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

        return Ok(result);
    }
}