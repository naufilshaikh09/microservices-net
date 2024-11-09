using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemController : ControllerBase
{
    private const string AdminRole = "Admin";
    private readonly IPublishEndpoint publishEndpoint;
    private readonly IRepository<Item> repository;

    public ItemController(IRepository<Item> repository, IPublishEndpoint publishEndpoint)
    {
        this.repository = repository;
        this.publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    [Authorize(Policies.Read)]
    public async Task<IEnumerable<ItemDto>> GetAsync()
    {
        var items = await repository.GetAllAsync();
        return items.Select(item => item.AsDto());
    }

    [HttpGet("{id}")]
    [Authorize(Policies.Read)]
    public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
    {
        var item = await repository.GetAsync(id);

        if (item == null) return NotFound();

        return item.AsDto();
    }

    [HttpPost]
    [Authorize(Policies.Write)]
    public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
    {
        var item = new Item
        {
            Name = createItemDto.Name,
            Description = createItemDto.Description,
            Price = createItemDto.Price,
            CreatedDate = DateTimeOffset.UtcNow
        };

        await repository.CreateAsync(item);

        await publishEndpoint.Publish(new CatalogItemCreated(
            item.Id,
            item.Name,
            item.Description,
            item.Price));

        return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    [Authorize(Policies.Write)]
    public async Task<ActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
    {
        var existingItem = await repository.GetAsync(id);

        if (existingItem == null) return NotFound();

        existingItem.Name = updateItemDto.Name;
        existingItem.Description = updateItemDto.Description;
        existingItem.Price = updateItemDto.Price;

        await repository.UpdateAsync(existingItem);

        await publishEndpoint.Publish(new CatalogItemUpdated(
            existingItem.Id,
            existingItem.Name,
            existingItem.Description,
            existingItem.Price));

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policies.Write)]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        var item = await repository.GetAsync(id);

        if (item == null) return NotFound();

        await repository.RemoveAsync(item.Id);

        await publishEndpoint.Publish(new CatalogItemDeleted(item.Id));

        return NoContent();
    }
}