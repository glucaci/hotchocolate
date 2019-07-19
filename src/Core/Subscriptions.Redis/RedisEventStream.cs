using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace HotChocolate.Subscriptions.Redis
{
    public class RedisEventStream : IEventStream
    {
        private readonly ChannelMessageQueue _channel;
        private readonly IPayloadSerializer _serializer;
        private bool _isCompleted = false;

        public RedisEventStream(
            ChannelMessageQueue channel,
            IPayloadSerializer serializer)
        {
            _channel = channel;
            _serializer = serializer;
        }

        public bool IsCompleted => _isCompleted;

        public async Task<IEventMessage> ReadAsync()
        {
            ChannelMessage message = await _channel
                .ReadAsync();

            var payload = await _serializer
                .DeserializeAsync(message.Message);

            return new EventMessage(
                message.Channel, payload);
        }

        public async Task<IEventMessage> ReadAsync(
            CancellationToken cancellationToken)
        {
            ChannelMessage message = await _channel
                .ReadAsync(cancellationToken);

            var payload = await _serializer
                .DeserializeAsync(message.Message);

            return new EventMessage(
                message.Channel, payload);
        }

        public async Task CompleteAsync()
        {
            if (!IsCompleted)
            {
                await _channel.UnsubscribeAsync();
                _isCompleted = true;
            }
        }

        public void Dispose()
        {
            if (!IsCompleted)
            {
                _channel.Unsubscribe();
                _isCompleted = true;
            }
        }
    }
}