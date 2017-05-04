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
    public class OrderSubmittedHandler : IHandleMessages<OrderSubmitted>
    {
        private IOrderRepository _orderrepository;
        static ILog log = LogManager.GetLogger<OrderSubmittedHandler>();
        private Random ChaosGenerator = new Random();

        string connectionString =
            @"Data Source = (localdb)\MSSQLLocalDB;Integrated Security = True; Persist Security Info=False;Initial Catalog = nservicebus";

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

            //var storagesession = context.SynchronizedStorageSession;
            //var ctxhelper = new ContextHelper()
            //{
            //    dbConnection =  storagesession.Session().Connection,
            //    dbTransaction = ExtractTransactionFromSession(context.SynchronizedStorageSession.Session())
            //};

            //_orderrepository = new OrderRepository(ctxhelper);
            //var orderrepository = new OrderRepository(storagesession.Session().Connection);
            //var orderrepository = new OrderRepository(new SqlConnection(connectionString));

            ////dbtransaction need to be injected to repository. Ideally using ExtractTransactionFromSession
            ////but we want dependency injector to do this. 
            await _orderrepository.Add(orderAccepted);

            if (ChaosGenerator.Next(2) == 0)
            {
                throw new Exception("Boom!");
            }


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