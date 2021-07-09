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
        private readonly IMongoRepository<InventoryItem> _inventoryItemRepository;
        //private readonly CatalogClient _catalogClient;
        private readonly IMongoRepository<CatalogItem> _catalogItemsRepository;

        public ItemsController(IMongoRepository<InventoryItem> inventoryItemRepository, IMongoRepository<CatalogItem> catalogItemsRepository) //CatalogClient catalogClient
        {
            _inventoryItemRepository = inventoryItemRepository;
            //_catalogClient = catalogClient;
            _catalogItemsRepository = catalogItemsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDTO>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest();

            //var catalogItems = await _catalogClient.GetCatalogItemsAsync();

            //var items = (await _inventoryItemRepository.GetAll(item => item.UserId == userId))
            //            .Select(x =>
            //            {
            //                var catalogItem = catalogItems.FirstOrDefault(y => y.Id == x.CatelogItemId);
            //                if (catalogItem != null)
            //                    return x.AsDTO(catalogItem.Name, catalogItem.Description);
            //                return null;
            //            }).ToList();

            //if (items != null && items.Count > 0)
            //    items = items.Where(x => x != null).ToList();

            var items = (await _inventoryItemRepository.GetAll(item => item.UserId == userId)).ToList();
            var itemIDs = items.Select(x => x.CatelogItemId);
            var catalogItems = await _catalogItemsRepository.GetAll(item => itemIDs.Contains(item.Id));

            var inventoryItemDTOs = items.Select(inventoryItem => {
                var catalogItem = catalogItems.FirstOrDefault(y => y.Id == inventoryItem.CatelogItemId);
                return inventoryItem.AsDTO(catalogItem.Name, catalogItem.Description);
            }).ToList();

            if (inventoryItemDTOs == null || (inventoryItemDTOs != null && inventoryItemDTOs.Count == 0))
                return NoContent();

            return Ok(inventoryItemDTOs);
        }

        [HttpPost]
        public async Task<ActionResult<InventoryItemDTO>> PostAsync(GrantItemsDTO grantItemsDTO)
        {
            var catalogItems = await _catalogItemsRepository.GetAll();
            var inventoryItem = await _inventoryItemRepository.Get(item => item.UserId == grantItemsDTO.UserId && item.CatelogItemId == grantItemsDTO.CategoryItemId);
            if (inventoryItem != null)
            {
                inventoryItem.Quantity += grantItemsDTO.Quantity;
                await _inventoryItemRepository.Update(inventoryItem);
            }
            else
            {
                inventoryItem = new InventoryItem
                {
                    CatelogItemId = grantItemsDTO.CategoryItemId,
                    UserId = grantItemsDTO.UserId,
                    Quantity = grantItemsDTO.Quantity,
                    AcquiredDate = DateTimeOffset.Now
                };

                await _inventoryItemRepository.Create(inventoryItem);
            }

            var catalogItem = catalogItems.FirstOrDefault(y => y.Id == inventoryItem.CatelogItemId);
            return Ok(inventoryItem.AsDTO(catalogItem.Name, catalogItem.Description));
        }
    }
}
