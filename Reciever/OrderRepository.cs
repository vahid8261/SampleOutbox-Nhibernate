using System;
using System.Data;

namespace Reciever
{
    public class OrderRepository : BaseRepository<Order, Guid>
    {
        public OrderRepository(IDbConnection connection, IDbTransaction dbTransaction) : base(connection, dbTransaction)
        {
        }
        public OrderRepository(IDbConnection connection) : base(connection)
        {
        }
    }
    public class Order : IBaseEntity<Guid>
    {
        public Guid OrderId { get; set; }
        public string Value { get; set; }

}
}