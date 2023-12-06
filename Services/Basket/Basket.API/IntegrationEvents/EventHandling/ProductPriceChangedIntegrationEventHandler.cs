using Basket.API.IntegrationEvents.Events;
using EventBus.Abstractions;
using EventBus.Events;
using System.Text.Json;

namespace Basket.API.IntegrationEvents.EventHandling;

public class ProductPriceChangedIntegrationEventHandler : IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>
{
    private readonly ILogger<ProductPriceChangedIntegrationEventHandler> _logger;

    public ProductPriceChangedIntegrationEventHandler(
        ILogger<ProductPriceChangedIntegrationEventHandler> logger
)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(ProductPriceChangedIntegrationEvent @event)
    {
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

        //using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        //{

        //    var userIds = _repository.GetUsers();

        //    foreach (var id in userIds)
        //    {
        //        var basket = await _repository.GetBasketAsync(id);

        //        await UpdatePriceInBasketItems(@event.ProductId, @event.NewPrice, @event.OldPrice, basket);
        //    }
        //}
    }

    //private async Task UpdatePriceInBasketItems(int productId, decimal newPrice, decimal oldPrice, CustomerBasket basket)
    //{
    //    var itemsToUpdate = basket?.Items?.Where(x => x.ProductId == productId).ToList();

    //    if (itemsToUpdate != null)
    //    {
    //        _logger.LogInformation("----- ProductPriceChangedIntegrationEventHandler - Updating items in basket for user: {BuyerId} ({@Items})", basket.BuyerId, itemsToUpdate);

    //        foreach (var item in itemsToUpdate)
    //        {
    //            if (item.UnitPrice == oldPrice)
    //            {
    //                var originalPrice = item.UnitPrice;
    //                item.UnitPrice = newPrice;
    //                item.OldUnitPrice = originalPrice;
    //            }
    //        }
    //        await _repository.UpdateBasketAsync(basket);
    //    }
    //}
}
