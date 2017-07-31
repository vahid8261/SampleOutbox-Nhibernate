using System;
using NServiceBus;

namespace Shared
{
    public class OrderSubmitted : ICommand
    {
        public Guid OrderId { get; set; }
        public string Value { get; set; }
    }
}