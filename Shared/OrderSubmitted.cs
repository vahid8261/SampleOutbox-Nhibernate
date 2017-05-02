using System;
using NServiceBus;

namespace Shared
{
    public class OrderSubmitted : IEvent
    {
        public Guid OrderId { get; set; }
        public string Value { get; set; }
    }
}