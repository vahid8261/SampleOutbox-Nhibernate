using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace Reciever
{
    public abstract class BaseRepository<T, TR> : IBaseRepository<T, TR> where T : class, IBaseEntity<TR>
    {
        protected readonly IDbConnection _connection ;
        private readonly IDbTransaction _dbTransaction ;

        private readonly ContextHelper _ctxHelper;

        //protected BaseRepository(IDbTransaction dbTransaction)
        //{
        //    _connection = dbTransaction.Connection;
        //    _dbTransaction = dbTransaction;
        //}

        protected BaseRepository(ContextHelper ctxHelper)
        {
            _ctxHelper = ctxHelper;
            _connection = _ctxHelper.dbConnection;
            _dbTransaction = _ctxHelper.dbTransaction;
        }

        //protected BaseRepository(IDbConnection connection)
        //{
        //    _connection = connection;
        //}

        public virtual async Task<int> Add(T item)
        {
            return await _ctxHelper.dbConnection.InsertAsync(item, _ctxHelper.dbTransaction);
            //return await _connection.InsertAsync(item);
        }

        public virtual async Task<bool> Update(T item)
        {
            return await _connection.UpdateAsync(item, _dbTransaction);
        }

        public virtual async Task<bool> Remove(T item)
        {
            return await _connection.DeleteAsync(item, _dbTransaction);
        }

        public virtual async Task<T> GetById(TR id)
        {
            return await _connection.GetAsync<T>(id, _dbTransaction);
        }
    }

    public interface IBaseRepository<T, in TR> where T : IBaseEntity<TR>
    {
        Task<int> Add(T item);
        Task<bool> Remove(T item);
        Task<bool> Update(T item);
        Task<T> GetById(TR id);
    }

    public interface IBaseEntity<T>
    {
        T OrderId { get; set; }
    }
}
