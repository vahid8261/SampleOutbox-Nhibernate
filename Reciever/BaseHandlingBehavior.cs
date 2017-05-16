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
        //private readonly IKernel _kernel;

        //BaseHandlingBehavior(IKernel kernel)
        //{
        //    _kernel = kernel;
        //}

            public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
        {
            var contextHelper = context.Builder.Build<ContextHelper>();
            //var contextHelper = _kernel.Get<ContextHelper>();
            contextHelper.DbTransaction = ExtractTransactionFromSession(context.SynchronizedStorageSession.Session());
            contextHelper.DbConnection = context.SynchronizedStorageSession.Session().Connection;
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
        private static long counter;
        public ContextHelper()
        {
            
            //var rnd = new Random();
            Ref = ++counter;
        }

        private IDbConnection _dbConnection;
        public IDbConnection DbConnection
        {
            get { return _dbConnection; }
            set
            {
                _dbConnection = value;
                OnPropertyChanged("DbConnection");
            }
        }
        public IDbTransaction DbTransaction { get; set; }


        public long Ref;
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
