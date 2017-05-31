using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using NHibernate;
using NServiceBus;
using NServiceBus.Logging;
using Shared;

namespace Reciever
{
    public class SaveOrderHandler : IHandleMessages<SaveOrder>
    {
        private IOrderRepository2 _orderrepository2;
        static ILog log = LogManager.GetLogger<SaveOrder>();
        private Random ChaosGenerator = new Random();
        
        public SaveOrderHandler(IOrderRepository2 orderrepository2)
        {
            _orderrepository2 = orderrepository2;
        }

        public async Task Handle(SaveOrder message, IMessageHandlerContext context)
        {
            log.Info($"Order {message.OrderId} worth {message.Value} submitted");

            #region StoreUserData
            var orderAccepted = new Order2()
            {
                OrderId = message.OrderId,
                Value = message.Value
            };

            if (FeatureToggle.OutBoxEnabled)
                await _orderrepository2.Add(orderAccepted);
            else
            {
                using (var tran = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _orderrepository2.Add(orderAccepted);
                    tran.Complete();
                }
            }

            await context.SendLocal(new CompleteOrder
            {
                OrderId = message.OrderId
            });
            // throw new Exception("Boom!");
            //if (ChaosGenerator.Next(2) == 0)
            //{
            //    throw new Exception("Boom!");
            //}

            #endregion
        }
    }
}