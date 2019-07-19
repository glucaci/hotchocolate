using System.Threading.Tasks;
using HotChocolate.Language;
using Xunit;

namespace HotChocolate.Subscriptions.Redis
{
    public class RedisTests
    {
        private readonly IEventRegistry _registry;
        private readonly IEventSender _sender;

        public RedisTests()
        {
            var redisEventRegistry = new RedisEventRegistry(
                RedisConnection.Create("localhost:6379"), // To be resolved
                new JsonPayloadSerializer());

            _sender = redisEventRegistry;
            _registry = redisEventRegistry;
        }

        [Fact(Skip = "Add underlining service")]
        public async Task SubscribeOneConsumer_SendMessage_ConsumerReceivesMessage()
        {
            // arrange
            var eventDescription = new EventDescription("foo");

            // act
            IEventStream consumer = await _registry
                .SubscribeAsync(eventDescription);
            var outgoing = new EventMessage(eventDescription, "bar");
            await _sender.SendAsync(outgoing);

            // assert
            IEventMessage incoming = await consumer.ReadAsync();
            Assert.Equal(outgoing.Payload, incoming.Payload);
        }

        [Fact(Skip = "Add underlining service")]
        public async Task SubscribeOneConsumer_Complete_StreamIsCompleted()
        {
            // arrange
            var eventDescription = new EventDescription("foo");

            // act
            IEventStream consumer = await _registry
                .SubscribeAsync(eventDescription);
            await consumer.CompleteAsync();

            // assert
            Assert.True(consumer.IsCompleted);
        }

        [Fact(Skip = "Add underlining service")]
        public async Task SubscribeTwoConsumer_SendOneMessage_BothConsumerReceivesMessage()
        {
            // arrange
            var eventDescription = new EventDescription("foo");

            // act
            IEventStream consumerOne = await _registry
                .SubscribeAsync(eventDescription);
            IEventStream consumerTwo = await _registry
                .SubscribeAsync(eventDescription);
            var outgoing = new EventMessage(eventDescription, "bar");
            await _sender.SendAsync(outgoing);

            // assert
            IEventMessage incomingOne = await consumerOne.ReadAsync();
            IEventMessage incomingTwo = await consumerTwo.ReadAsync();
            Assert.Equal(outgoing.Payload, incomingOne.Payload);
            Assert.Equal(outgoing.Payload, incomingTwo.Payload);
        }

        [Fact(Skip = "Add underlining service")]
        public async Task SubscribeTwoConsumer_SendTwoMessage_BothConsumerReceivesIndependentMessage()
        {
            // arrange
            var eventDescriptionOne = new EventDescription(
                "foo", new ArgumentNode("nodeOne", "x"));
            var eventDescriptionTwo = new EventDescription(
                "foo", new ArgumentNode("nodeTwo", "x"));

            // act
            IEventStream consumerOne = await _registry
                .SubscribeAsync(eventDescriptionOne);
            var outgoingOne = new EventMessage(eventDescriptionOne, "bar");
            await _sender.SendAsync(outgoingOne);

            IEventStream consumerTwo = await _registry
                .SubscribeAsync(eventDescriptionTwo);
            var outgoingTwo = new EventMessage(eventDescriptionTwo, "bar");
            await _sender.SendAsync(outgoingTwo);

            // assert
            IEventMessage incomingOne = await consumerOne.ReadAsync();
            IEventMessage incomingTwo = await consumerTwo.ReadAsync();
            Assert.Equal(outgoingOne.Payload, incomingOne.Payload);
            Assert.Equal(outgoingTwo.Payload, incomingTwo.Payload);
            Assert.NotEqual(incomingOne.Event, incomingTwo.Event);
        }
    }
}