using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
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

        string connectionString =
            @"Data Source = (localdb)\MSSQLLocalDB;Integrated Security = True; Persist Security Info=False;Initial Catalog = nservicebus";

        private IKernel _kernel;

        public OrderSubmittedHandler(IOrderRepository orderrepository)
        {
            _orderrepository = orderrepository;
        }

        //public OrderSubmittedHandler(IKernel kernel)
        //{
        //    _kernel = kernel;
        //}

        public async Task Handle(OrderSubmitted message, IMessageHandlerContext context)
        {
             log.Info($"Order {context.MessageId} worth {message.Value} submitted");

            #region StoreUserData
            var orderAccepted = new Order()
            {
                OrderId = new Guid(context.MessageId),
                Value = message.Value
            };

         //  _orderrepository = _kernel.Get<IOrderRepository>();
            await _orderrepository.Add(orderAccepted);

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