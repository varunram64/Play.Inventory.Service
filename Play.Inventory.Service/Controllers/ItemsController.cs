using Microsoft.AspNetCore.Mvc;
using Play.Common;
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
        private readonly IRepository<InventoryItem> _mongoRepository;
        private readonly CatalogClient _catalogClient;

        public ItemsController(IRepository<InventoryItem> mongoRepository, CatalogClient catalogClient)
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

            var items = (await _mongoRepository.GetAllAsync(item => item.UserId == userId))
                        .Select(x => {
                            var catalogItem = catalogItems.FirstOrDefault(y => y.id == x.CategoryItemId);
                            return x.AsDTO(catalogItem.Name, catalogItem.Description);
                        }).ToList();

            if (items == null || (items != null && items.Count == 0))
                return NoContent();

            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult<InventoryItemDTO>> PostAsync(GrantItemsDTO grantItemsDTO)
        {
            var catalogItems = await _catalogClient.GetCatalogItemsAsync();
            var inventoryItem = await _mongoRepository.GetAsync(item => item.UserId == grantItemsDTO.UserId && item.CategoryItemId == grantItemsDTO.CategoryItemId);
            if(inventoryItem != null)
            {
                inventoryItem.Quantity += grantItemsDTO.Quantity;
                await _mongoRepository.UpdateAsync(inventoryItem);
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

                await _mongoRepository.CreateAsync(inventoryItem);
            }

            var catalogItem = catalogItems.FirstOrDefault(y => y.id == inventoryItem.CategoryItemId);
            return Ok(inventoryItem.AsDTO(catalogItem.Name, catalogItem.Description));
        }
    }
}
