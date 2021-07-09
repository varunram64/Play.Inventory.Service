using MassTransit;
using System.Threading.Tasks;
using static Play.Catalog.Contracts.Constracts;
using Play.Common.IRepository;
using Play.Inventory.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
    {
        private readonly IMongoRepository<CatalogItem> repository;

        public CatalogItemCreatedConsumer(IMongoRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            var message = context.Message;
            var item = await repository.Get(message.ItemId);

            if (item != null)
                return;

            item = new CatalogItem
            { 
                Id = message.ItemId,
                Name= message.Name,
                Description = message.Description
            };

            await repository.Create(item);
        }
    }
}
