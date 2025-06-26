using Application.Events;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace VisualKanbanWebAPI.Hubs
{
    public class ProductInfromationChangedHandler: INotificationHandler<ProductInfromationNotification>
    {
        private readonly IHubContext<DeviceHub> _hubContext;

        public ProductInfromationChangedHandler(IHubContext<DeviceHub> hubContext)
        {
            _hubContext = hubContext;
        }
     
        public async Task Handle(ProductInfromationNotification notification, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.All.SendAsync(notification.Type, notification.SerialNumber);
        }
    }
}
