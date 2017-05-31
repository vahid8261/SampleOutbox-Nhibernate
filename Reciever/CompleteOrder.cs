using System;

namespace Reciever
{
    public class CompleteOrder : NServiceBus.ICommand
    {
        public Guid OrderId { get; set; }
    }
}