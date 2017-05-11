using System;
using System.ComponentModel;
using System.Data;

namespace Reciever
{
    public class NSBContextProvider : IContextProvider
    {
        public IDbTransaction DbTransaction { get; set; }
        public IDbConnection DbConnection { get; set; }

        public Ref Ref { get; set; }

        public void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            this.DbTransaction = ((ContextHelper)sender).GetDbTransaction();
            this.DbConnection = ((ContextHelper)sender).getDbConnection();
            this.Ref = ((ContextHelper)sender).Ref;
        }
    }

    public interface IContextProvider
    {
        IDbTransaction DbTransaction { get; set; }
        IDbConnection DbConnection { get; set; }
        Ref Ref { get; set; }
        void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);
    }
}