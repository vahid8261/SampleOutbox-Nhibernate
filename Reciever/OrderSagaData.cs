using System;
using NServiceBus;

namespace Reciever
{
    public class OrderSagaData :
        ContainSagaData
    {
        public virtual Guid OrderId { get; set; }
    }
}