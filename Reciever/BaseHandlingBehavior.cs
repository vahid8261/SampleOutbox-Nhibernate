using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using NHibernate;
using NServiceBus;
using NServiceBus.Pipeline;

namespace Reciever
{
    public class BaseHandlingBehavior : Behavior<IInvokeHandlerContext>
    {
        public override async  Task Invoke(IInvokeHandlerContext context, Func<Task> next)
        {
            var ctx = context.Builder.Build<ContextHelper>();
            ctx.dbTransaction = ExtractTransactionFromSession(context.SynchronizedStorageSession.Session());
            ctx.dbConnection =  context.SynchronizedStorageSession.Session().Connection;
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
                return helper.Transaction;
            }
        }
    }
}
