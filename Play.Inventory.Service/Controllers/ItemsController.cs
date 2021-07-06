using Microsoft.AspNetCore.Mvc;
using Play.Common.IRepository;
using Play.Inventory.Entities;
using Play.Inventory.Service.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Inventory.Service.Controllers
{
    [Route("items")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IMongoRepository<InventoryItem> _mongoRepository;
        private readonly CatalogClient _catalogClient;

        public ItemsController(IMongoRepository<InventoryItem> mongoRepository, CatalogClient catalogClient)
        {
            _mongoRepository = mongoRepository;
            _catalogClient = catalogClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDTO>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest();

            var catalogItems = await _catalogClient.GetCatalogItemsAsync();

            var items = (await _mongoRepository.GetAll(item => item.UserId == userId))
                        .Select(x =>
                        {
                            var catalogItem = catalogItems.FirstOrDefault(y => y.id == x.CategoryItemId);
                            if (catalogItem != null)
                                return x.AsDTO(catalogItem.Name, catalogItem.Description);
                            return null;
                        }).ToList();

            if (items != null && items.Count > 0)
                items = items.Where(x => x != null).ToList();

            if (items == null || (items != null && items.Count == 0))
                return NoContent();

            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult<InventoryItemDTO>> PostAsync(GrantItemsDTO grantItemsDTO)
        {
            var catalogItems = await _catalogClient.GetCatalogItemsAsync();
            var inventoryItem = await _mongoRepository.Get(item => item.UserId == grantItemsDTO.UserId && item.CategoryItemId == grantItemsDTO.CategoryItemId);
            if (inventoryItem != null)
            {
                inventoryItem.Quantity += grantItemsDTO.Quantity;
                await _mongoRepository.Update(inventoryItem);
            }
            else
            {
                inventoryItem = new InventoryItem
                {
                    CategoryItemId = grantItemsDTO.CategoryItemId,
                    UserId = grantItemsDTO.UserId,
                    Quantity = grantItemsDTO.Quantity,
                    AcquiredDate = DateTimeOffset.Now
                };

                await _mongoRepository.Create(inventoryItem);
            }

            var catalogItem = catalogItems.FirstOrDefault(y => y.id == inventoryItem.CategoryItemId);
            return Ok(inventoryItem.AsDTO(catalogItem.Name, catalogItem.Description));
        }
    }
}
