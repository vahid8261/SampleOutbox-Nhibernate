using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NHibernate;
using Ninject;
using NServiceBus;
using NServiceBus.Pipeline;

namespace Reciever
{
    public class BaseHandlingBehavior : Behavior<IInvokeHandlerContext>
    {
        public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
        {
            var contextHelper = context.Builder.Build<ContextHelper>();
            contextHelper.SetDbTransaction(ExtractTransactionFromSession(context.SynchronizedStorageSession.Session()));
            contextHelper.setDbConnection(context.SynchronizedStorageSession.Session().Connection);
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

    public class ContextHelper : INotifyPropertyChanged
    {
        public ContextHelper()
        {
            var rnd = new Random();
            Ref = new Ref()
            {
                RefNumber = rnd.Next(1000)
            };
        }

        private IDbConnection _dbConnection;
        public IDbConnection getDbConnection()
        {
            return _dbConnection;
        }
        public void setDbConnection(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            OnPropertyChanged("DbTransaction");
        }

        private IDbTransaction _dbTransaction;
        public IDbTransaction GetDbTransaction()
        {
            return _dbTransaction;
        }
        public void SetDbTransaction(IDbTransaction dbTransaction)
        {
            _dbTransaction = dbTransaction;
        }

        //public DbConnection DbConnection { get; set; }
        //public DbTransaction DbTransaction { get; set; }

        public Ref Ref;
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Ref
    {
        public int RefNumber { get; set; }
    }
}
