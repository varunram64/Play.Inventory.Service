using System;

namespace Play.Inventory.Service
{
    public record GrantItemsDTO(Guid UserId, Guid CategoryItemId, int Quantity);
    public record InventoryItemDTO(Guid CategoryItemId, string Name, string Description, int Quantity, DateTimeOffset AcquiredDate);
    public record CatalogItemDTO(Guid id, string Name, string Description);
}
