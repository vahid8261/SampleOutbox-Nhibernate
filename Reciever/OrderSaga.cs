using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Sagas;
using Shared;

namespace Reciever
{

    public class OrderSaga :
    Saga<OrderSagaData>,
    IAmStartedByMessages<OrderSubmitted>,
    IHandleMessages<CompleteOrder>
    {

        static ILog log = LogManager.GetLogger<OrderSubmittedHandler>();
        public Task Handle(CompleteOrder message, IMessageHandlerContext context)
        {
            log.Info($"Saga for Order {context.MessageId} with orderid {message.OrderId} is Completed");
            MarkAsComplete();
            return Task.FromResult(0);
        }

        public Task Handle(OrderSubmitted message, IMessageHandlerContext context)
        {
            log.Info($"saga for Order {context.MessageId} with orderid {message.OrderId} is started");

            Data.OrderId = message.OrderId;
            context.SendLocal(new SaveOrder()
            {
                OrderId = message.OrderId,
                Value = message.Value
            });

            return Task.FromResult(0);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<OrderSagaData> mapper)
        {
            mapper.ConfigureMapping<OrderSubmitted>(_ => _.OrderId)
                .ToSaga(_ => _.OrderId);
            mapper.ConfigureMapping<CompleteOrder>(_ => _.OrderId)
                .ToSaga(_ => _.OrderId);
        }

        public class SagaNotFoundHandler :
            IHandleSagaNotFound
        {
            public Task Handle(object message, IMessageProcessingContext context)
            {
                throw new Exception("No saga is found");
            }
        }
    }
}
