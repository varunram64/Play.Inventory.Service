using Play.Inventory.Entities;

namespace Play.Inventory.Service
{
    public static class Extensions
    {
        public static InventoryItemDTO AsDTO(this InventoryItem item, string Name, string Description)
        {
            return new InventoryItemDTO(item.CatelogItemId, Name, Description, item.Quantity, item.AcquiredDate);
        }
    }
}
