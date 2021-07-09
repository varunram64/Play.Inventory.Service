using MassTransit;
using System.Threading.Tasks;
using static Play.Catalog.Contracts.Constracts;
using Play.Common.IRepository;
using Play.Inventory.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
    {
        private readonly IMongoRepository<CatalogItem> repository;

        public CatalogItemUpdatedConsumer(IMongoRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            var message = context.Message;
            var item = await repository.Get(message.ItemId);

            if (item == null)
            {
                item = new CatalogItem
                {
                    Id = message.ItemId,
                    Name = message.Name,
                    Description = message.Description
                };

                await repository.Create(item);
            }
            else
            {
                item = new CatalogItem
                {
                    Id = message.ItemId,
                    Name = message.Name,
                    Description = message.Description
                };

                await repository.Update(item);
            }
        }
    }
}
