using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EventBus;
using EventBus.Abstractions;
using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventBusServiceBus
{
    public class EventBusImpServiceBus : IEventBus, IDisposable
    {
        private readonly string _topicName = "eshop_event_bus";
        private const string INTEGRATION_EVENT_SUFFIX = "IntegrationEvent";
        private readonly string _subscriptionName;

        private readonly IServiceBusPersisterConnection _serviceBusPersisterConnection;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly ILogger<EventBusImpServiceBus> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceBusSender _sender;
        private readonly ServiceBusProcessor _processor;
        public EventBusImpServiceBus(
            IServiceBusPersisterConnection serviceBusPersisterConnection,
            ILogger<EventBusImpServiceBus> logger,
            IEventBusSubscriptionsManager subsManager,
            IServiceProvider serviceProvider,
            string subscriptionName)
        {
            _serviceBusPersisterConnection = serviceBusPersisterConnection;
            _logger = logger;
            _subsManager = subsManager;
            _serviceProvider = serviceProvider;
            _sender = _serviceBusPersisterConnection.TopicClient.CreateSender(_topicName);
            _subscriptionName = subscriptionName;
            ServiceBusProcessorOptions options = new ServiceBusProcessorOptions { MaxConcurrentCalls = 10, AutoCompleteMessages = false };
            _processor = _serviceBusPersisterConnection.TopicClient.CreateProcessor(_topicName, _subscriptionName, options);

            RemoveDefaultRule();
            RegisterSubscriptionClientMessageHandlerAsync().GetAwaiter().GetResult();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFFIX, "");
            var jsonMessage = JsonSerializer.Serialize(@event, @event.GetType());
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var message = new ServiceBusMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = new BinaryData(body),
                Subject = eventName,
            };

            _sender.SendMessageAsync(message)
                .GetAwaiter()
                .GetResult();
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFFIX, "");

            var containsKey = _subsManager.HasSubscriptionsForEvent<T>();
            if (!containsKey)
            {
                try
                {
                    _serviceBusPersisterConnection.AdministrationClient.CreateRuleAsync(_topicName, _subscriptionName, new CreateRuleOptions
                    {
                        Filter = new CorrelationRuleFilter() { Subject = eventName },
                        Name = eventName
                    }).GetAwaiter().GetResult();
                }
                catch (ServiceBusException)
                {
                    _logger.LogWarning("The messaging entity {eventName} already exists.", eventName);
                }
            }

            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

            _subsManager.AddSubscription<T, TH>();
        }

        public void SubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            throw new NotImplementedException();
        }

        private async Task RegisterSubscriptionClientMessageHandlerAsync()
        {
            _processor.ProcessMessageAsync +=
                async (args) =>
                {
                    var eventName = $"{args.Message.Subject}{INTEGRATION_EVENT_SUFFIX}";
                    string messageData = args.Message.Body.ToString();

                    // Complete the message so that it is not received again.
                    if (await ProcessEvent(eventName, messageData))
                    {
                        await args.CompleteMessageAsync(args.Message);
                    }
                };

            _processor.ProcessErrorAsync += ErrorHandler;
            await _processor.StartProcessingAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            var ex = args.Exception;
            var context = args.ErrorSource;

            _logger.LogError(ex, "ERROR handling message: {ExceptionMessage} - Context: {@ExceptionContext}", ex.Message, context);

            return Task.CompletedTask;
        }

        private async Task<bool> ProcessEvent(string eventName, string message)
        {
            var processed = false;
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                foreach (var subscription in subscriptions)
                {
                    if (subscription.IsDynamic)
                    {
                        if (scope.ServiceProvider.GetService(subscription.HandlerType) is not IDynamicIntegrationEventHandler handler) continue;

                        using dynamic eventData = JsonDocument.Parse(message);
                        await handler.Handle(eventData);
                    }
                    else
                    {
                        IEnumerable<IntegrationEvent> integrationEvents = new List<IntegrationEvent>();
                        var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                        if (handler == null) continue;
                        var eventType = _subsManager.GetEventTypeByName(eventName);
                        var integrationEvent = JsonSerializer.Deserialize(message, eventType);
                        integrationEvents.Append(integrationEvent);
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new[] {(IEnumerable<IntegrationEvent>)integrationEvents });
                    }
                }
            }
            processed = true;
            return processed;
        }

        private void RemoveDefaultRule()
        {
            try
            {
                _serviceBusPersisterConnection
                    .AdministrationClient
                    .DeleteRuleAsync(_topicName, _subscriptionName, RuleProperties.DefaultRuleName)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
            {
                _logger.LogWarning("The messaging entity {DefaultRuleName} Could not be found.", RuleProperties.DefaultRuleName);
            }
        }
    }
}
