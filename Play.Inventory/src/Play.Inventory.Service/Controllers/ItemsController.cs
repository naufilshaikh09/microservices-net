using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Contracts;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    private const string AdminRole = "Admin";
    private readonly IRepository<CatalogItem> catalogItemsRepository;
    private readonly IRepository<InventoryItem> inventoryItemsRepository;
    private readonly IPublishEndpoint publishEndpoint;

    public ItemsController(
        IRepository<InventoryItem> inventoryItemsRepository,
        IRepository<CatalogItem> catalogItemsRepository,
        IPublishEndpoint publishEndpoint)
    {
        this.inventoryItemsRepository = inventoryItemsRepository;
        this.catalogItemsRepository = catalogItemsRepository;
        this.publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty) return BadRequest();

        var currentUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (Guid.Parse(currentUserId) != userId)
            if (!User.IsInRole(AdminRole))
                return Forbid();

        var inventoryItemEntities = await inventoryItemsRepository.GetAllAsync(item => item.UserId == userId);
        var itemIds = inventoryItemEntities.Select(item => item.CatalogItemId);
        var catalogItemEntities = await catalogItemsRepository.GetAllAsync(item => itemIds.Contains(item.Id));

        var items = inventoryItemEntities.Select(inventoryItem =>
        {
            var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
            return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        });

        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = AdminRole)]
    public async Task<ActionResult> PostAsync(GrantItemsDto dto)
    {
        var inventoryItem = await inventoryItemsRepository.GetAsync(item =>
            item.UserId == dto.UserId && item.CatalogItemId == dto.CatalogItemId);

        if (inventoryItem == null)
        {
            inventoryItem = new InventoryItem
            {
                UserId = dto.UserId,
                CatalogItemId = dto.CatalogItemId,
                Quantity = dto.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };

            await inventoryItemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += dto.Quantity;
            await inventoryItemsRepository.UpdateAsync(inventoryItem);
        }

        await publishEndpoint.Publish(new InventoryItemUpdated(
            inventoryItem.UserId,
            inventoryItem.CatalogItemId,
            inventoryItem.Quantity));

        return Ok();
    }
}