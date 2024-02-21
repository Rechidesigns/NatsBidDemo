using NATS.Client;
using NATS.Client.JetStream;

namespace BidService
{
    public class NatsConnector
    {
        private readonly IConnection _connection;

        public NatsConnector(string natsUrl)
        {
            _connection = new ConnectionFactory().CreateConnection(natsUrl);
        }
        public IJetStream GetJetStreamContext() => _connection.CreateJetStreamContext();

        public void Subscribe(string subject, string durableName, EventHandler<MsgHandlerEventArgs> messageHandler)
        {
            var jetStreamContext = _connection.CreateJetStreamContext();
            var consumerConfig = ConsumerConfiguration.Builder().WithDurable(durableName).Build();
            var pushSubscribeOptions = PushSubscribeOptions.Builder().WithConfiguration(consumerConfig).Build().Bind;

            // Assuming an overload that correctly matches your library version:
            jetStreamContext.PushSubscribeAsync(subject, messageHandler, pushSubscribeOptions);
        }
    }
}
