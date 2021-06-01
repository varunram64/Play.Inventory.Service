using System;

namespace Play.Inventory.Service
{
    public record GrantItemsDTO(Guid UserId, Guid CategoryItemId, int Quantity);
    public record InventoryItemDTO(Guid CategoryItemId, int Quantity, DateTimeOffset AcquiredDate);
}
