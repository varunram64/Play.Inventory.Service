using Microsoft.AspNetCore.Mvc;
using Play.Common.IRepository;
using Play.Inventory.Entities;
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

        public ItemsController(IMongoRepository<InventoryItem> mongoRepository)
        {
            _mongoRepository = mongoRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDTO>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest();

            var items = (await _mongoRepository.GetAll(item => item.UserId == userId))
                        .Select(x => x.AsDTO());
            if (items == null || (items != null && items.Count() == 0))
                return NotFound();

            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult<InventoryItemDTO>> PostAsync(GrantItemsDTO grantItemsDTO)
        {
            var inventoryItem = await _mongoRepository.Get(item => item.UserId == grantItemsDTO.UserId && item.CategoryItemId == grantItemsDTO.CategoryItemId);
            if(inventoryItem != null)
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

            return Ok(inventoryItem.AsDTO());
        }
    }
}
