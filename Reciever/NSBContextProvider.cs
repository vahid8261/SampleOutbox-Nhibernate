using System;
using System.ComponentModel;
using System.Data;

namespace Reciever
{
    public class NsbDbContextProvider : IDbContextProvider
    {
        private readonly DbContextHelper _dbContextHelper;

        public NsbDbContextProvider(DbContextHelper dbContextHelper)
        {
            _dbContextHelper = dbContextHelper;
        }

        public IDbConnection DbConnection => _dbContextHelper.DbConnection;

        public IDbTransaction DbTransaction => _dbContextHelper.DbTransaction;
    }


    public interface IDbContextProvider
    {
        IDbTransaction DbTransaction { get; }

        IDbConnection DbConnection { get; }
    }
}