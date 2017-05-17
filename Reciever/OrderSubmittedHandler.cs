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
        private IOrderRepository _orderrepository;
        static ILog log = LogManager.GetLogger<OrderSubmittedHandler>();
        private Random ChaosGenerator = new Random();

        public OrderSubmittedHandler(IOrderRepository orderrepository)
        {
            _orderrepository = orderrepository;
        }


        public async Task Handle(OrderSubmitted message, IMessageHandlerContext context)
        {
            log.Info($"Order {context.MessageId} worth {message.Value} submitted");

            #region StoreUserData
            var orderAccepted = new Order()
            {
                OrderId = new Guid(context.MessageId),
                Value = message.Value
            };

            if (FeatureToggle.OutBoxEnabled)
                await _orderrepository.Add(orderAccepted);
            else
            {
                using (var tran = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
                {

                    await _orderrepository.Add(orderAccepted);
                    tran.Complete();
                }
        }

            if (ChaosGenerator.Next(2) == 0)
            {
                throw new Exception("Boom!");
            }
            #endregion
        }
    }
}