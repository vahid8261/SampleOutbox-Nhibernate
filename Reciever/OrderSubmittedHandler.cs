using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using NHibernate;
using Ninject;
using NServiceBus;
using NServiceBus.Logging;
using Shared;

namespace Reciever
{
    public class OrderSubmittedHandler : IHandleMessages<OrderSubmitted>
    {
        static ILog log = LogManager.GetLogger<OrderSubmittedHandler>();
        private Random ChaosGenerator = new Random();

        public OrderSubmittedHandler()
        {
        }

        public async Task Handle(OrderSubmitted message, IMessageHandlerContext context)
        {
            log.Info($"Order {context.MessageId} with orderid {message.OrderId} submitted");

        }
    }
}