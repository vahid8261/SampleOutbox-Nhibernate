using System;
using System.ComponentModel;
using System.Data;

namespace Reciever
{
    public class NSBContextProvider : IContextProvider
    {
        public IDbTransaction DbTransaction { get; set; }
        public IDbConnection DbConnection { get; set; }

        public void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            this.DbTransaction = ((ContextHelper)sender).DbTransaction;
            this.DbConnection = ((ContextHelper)sender).DbConnection;
        }

        public NSBContextProvider(ContextHelper ctx)
        {
            ctx.PropertyChanged += PropertyChangedEventHandler;
        }

    }


    public interface IContextProvider
    {
        IDbTransaction DbTransaction { get; set; }
        IDbConnection DbConnection { get; set; }
        void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);
    }
}