using Application.Events;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace VisualKanbanWebAPI.Hubs
{
    public class ProductInfromationChangedHandler : INotificationHandler<ProductInfromationNotification>
    {
        private readonly IHubContext<SignalRHub> _hubContext;

        public ProductInfromationChangedHandler(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Handle(ProductInfromationNotification notification, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(notification.Type))
            {
                await _hubContext.Clients.All.SendAsync(notification.Type, notification.OutPutQuantity, cancellationToken);
            }
            else
            {
                throw new ArgumentNullException(nameof(notification.Type), "Notification type cannot be null.");
            }
        }
    }
}
