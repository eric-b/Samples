using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace Demo.Frontend
{
    public sealed class Trigger(IOptions<TriggerOptions> options, ServiceBusClient serviceBus) : IAsyncDisposable
    {
        private readonly ServiceBusSender _sender = serviceBus.CreateSender(options.Value.QueueName);
        
        public async Task Send(CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(new ServiceBusMessage("trigger")
            {
                TimeToLive = TimeSpan.FromSeconds(10)
            }, cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return _sender.DisposeAsync();
        }
    }
}
