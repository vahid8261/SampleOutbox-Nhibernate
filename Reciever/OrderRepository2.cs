﻿using System;
using System.Data;

namespace Reciever
{
    public class OrderRepository2 : BaseRepository<Order2, Guid>, IOrderRepository2
    {
        public OrderRepository2(IDbContextProvider ctxProvider) : base(ctxProvider)
        {
        }

        public OrderRepository2(IDbConnection dbConnection) : base(dbConnection)
        {
        }
    }

    public interface IOrderRepository2 : IBaseRepository<Order2, Guid>
    {
    }

    public class Order2 : IBaseEntity<Guid>
    {
        public Guid OrderId { get; set; }
        public string Value { get; set; }
    }
}