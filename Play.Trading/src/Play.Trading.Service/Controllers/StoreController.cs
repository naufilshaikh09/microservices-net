using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Trading.Service.Dtos;
using Play.Trading.Service.Entities;

namespace Play.Trading.Service.Controllers;

[ApiController]
[Route("store")]
[Authorize]
public class StoreController : ControllerBase
{
    private readonly IRepository<CatalogItem> _catalogRepository;
    private readonly IRepository<InventoryItem> _inventoryRepository;
    private readonly IRepository<ApplicationUser> _userRepository;

    public StoreController(
        IRepository<CatalogItem> catalogRepository,
        IRepository<ApplicationUser> userRepository,
        IRepository<InventoryItem> inventoryRepository)
    {
        _catalogRepository = catalogRepository;
        _userRepository = userRepository;
        _inventoryRepository = inventoryRepository;
    }

    [HttpGet]
    public async Task<ActionResult<StoreDto>> GetAsync()
    {
        var userId = User.FindFirstValue("sub");

        var catalogItems = await _catalogRepository.GetAllAsync();
        var inventoryItems = await _inventoryRepository.GetAllAsync(
            item => item.UserId == Guid.Parse(userId));

        var user = await _userRepository.GetAsync(Guid.Parse(userId));

        var storeDto = new StoreDto(
            catalogItems.Select(catalogItem =>
                new StoreItemDto(
                    catalogItem.Id,
                    catalogItem.Name,
                    catalogItem.Description,
                    catalogItem.Price,
                    inventoryItems.FirstOrDefault(
                        inventoryItem => inventoryItem.CatalogItemId == catalogItem.Id)?.Quantity ?? 0
                )
            ),
            user?.Gil ?? 0);

        return Ok(storeDto);
    }
}