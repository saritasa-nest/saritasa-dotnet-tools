using System;
using SandBox.Events;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Events;

namespace SandBox
{
    /// <summary>
    /// Events handlers.
    /// </summary>
    [EventHandlers]
    public class EventsHandlers
    {
        public IProductsRepository ProductsRepository { get; set; }

        public void HandleOnUpdateProductEvent(UpdateProductEvent ev)
        {
            var product = ProductsRepository.Get(ev.Id);
            Console.WriteLine("EventsHandlers: HandleUpdateProductEvent");
        }
    }
}
