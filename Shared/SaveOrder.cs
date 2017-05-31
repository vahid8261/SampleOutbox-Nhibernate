using System;
using NServiceBus;

namespace Reciever
{
    public class SaveOrder : ICommand
    {
        public Guid OrderId { get; set; }
        public string Value { get; set; }
    }
}