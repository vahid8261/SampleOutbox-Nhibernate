using System;
using System.Data;
using System.Threading.Tasks;
using NHibernate;
using NServiceBus;
using NServiceBus.Pipeline;

namespace Reciever
{
    public class BaseHandlingBehavior : Behavior<IInvokeHandlerContext>
    {
            public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
        {
            var contextHelper = context.Builder.Build<DbContextHelper>();

            contextHelper.DbConnection = context.SynchronizedStorageSession.Session().Connection;
            contextHelper.DbTransaction = ExtractTransactionFromSession(context.SynchronizedStorageSession.Session());
            
            await next().ConfigureAwait(false);
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base(typeof(BaseHandlingBehavior).Name, typeof(BaseHandlingBehavior), "Database context behavior yo!")
            {
            }
        }

        static IDbTransaction ExtractTransactionFromSession(ISession storageContext)
        {
            using (var helper = storageContext.Connection.CreateCommand())
            {
                storageContext.Transaction.Enlist(helper);
                return  helper.Transaction;
            }
        }
    }

    public class DbContextHelper
    {
        private static long counter;
        public DbContextHelper()
        {
            //var rnd = new Random();
            Ref = ++counter;
        }
        

        public IDbConnection getDbConnection()
        {
            return DbConnection;
        }

        public IDbConnection DbConnection { get; internal set; }

        public IDbTransaction DbTransaction { get; internal set; }

        public long Ref;
    }
}
