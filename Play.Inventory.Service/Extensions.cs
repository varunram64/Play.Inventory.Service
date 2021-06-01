using Play.Inventory.Entities;

namespace Play.Inventory.Service
{
    public static class Extensions
    {
        public static InventoryItemDTO AsDTO(this InventoryItem item)
        {
            return new InventoryItemDTO(item.CategoryItemId, item.Quantity, item.AcquiredDate);
        }
    }
}
