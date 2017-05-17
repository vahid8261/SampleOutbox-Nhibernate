using System;
using System.Data;

namespace Reciever
{
    public class OrderRepository : BaseRepository<Order, Guid>, IOrderRepository
    {
        public OrderRepository(IDbContextProvider ctxProvider) : base(ctxProvider)
        {
        }

        public OrderRepository(IDbConnection dbConnection): base(dbConnection)
        {
        }
    }

    public interface IOrderRepository : IBaseRepository<Order, Guid>
    {
    }

    public class Order : IBaseEntity<Guid>
    {
        public Guid OrderId { get; set; }
        public string Value { get; set; }
    }
}