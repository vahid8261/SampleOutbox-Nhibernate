using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NHibernate;
using NServiceBus;
using NServiceBus.Logging;
using Shared;

namespace Reciever
{
    public class OrderSubmittedHandler2 : IHandleMessages<OrderSubmitted>
    {
        private IOrderRepository2 _orderrepository;
        static ILog log = LogManager.GetLogger<OrderSubmittedHandler2>();
        private Random ChaosGenerator = new Random();

        string connectionString =
            @"Data Source = (localdb)\MSSQLLocalDB;Integrated Security = True; Persist Security Info=False;Initial Catalog = nservicebus";

        public OrderSubmittedHandler2(IOrderRepository2 orderrepository)
        {
            _orderrepository = orderrepository;
        }

        public async Task Handle(OrderSubmitted message, IMessageHandlerContext context)
        {
             log.Info($"Order {context.MessageId} worth {message.Value} submitted");

            #region StoreUserData
            var orderAccepted = new Order2()
            {
                OrderId = new Guid(context.MessageId),
                Value = message.Value
            };

            await _orderrepository.Add(orderAccepted);


           // throw new Exception("Boom!");
            //if (ChaosGenerator.Next(2) == 0)
            //{
            //    throw new Exception("Boom!");
            //}


            #endregion
        }

        static IDbTransaction ExtractTransactionFromSession(ISession storageContext)
        {
            using (var helper = storageContext.Connection.CreateCommand())
            {
                storageContext.Transaction.Enlist(helper);
                return helper.Transaction;
            }
        }
    }
}