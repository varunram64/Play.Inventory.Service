using MassTransit;
using System.Threading.Tasks;
using static Play.Catalog.Contracts.Constracts;
using Play.Common.IRepository;
using Play.Inventory.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
    {
        private readonly IMongoRepository<CatalogItem> repository;

        public CatalogItemDeletedConsumer(IMongoRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
        {
            var message = context.Message;
            var item = await repository.Get(message.ItemId);

            if (item == null)
                return;

            await repository.Delete(item.Id);
        }
    }
}
