using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> repository;
        private readonly CatalogClient catalogClient;

        public ItemsController(IRepository<InventoryItem> repository, CatalogClient catalogClient)
        {
            this.repository = repository;
            this.catalogClient = catalogClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var catalogItems = await catalogClient.GetCatalogItemsAsync();
            var inventoryItems = await repository.GetAllAsync(item => item.UserId == userId);

            var items = inventoryItems.Select(inventoryItem =>
            {
                var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);

                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });

            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDto dto)
        {
            var inventoryItem = await repository.GetAsync(item =>
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

                await repository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += dto.Quantity;
                await repository.UpdateAsync(inventoryItem);
            }

            return Ok();
        }
    }
}