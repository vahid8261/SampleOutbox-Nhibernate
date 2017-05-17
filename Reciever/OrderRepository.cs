using System;
using System.Data;

namespace Reciever
{
    public class OrderRepository : BaseRepository<Order, Guid>, IOrderRepository
    {
        public OrderRepository(IContextProvider ctxProvider) : base(ctxProvider)
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