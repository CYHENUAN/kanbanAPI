using Application.Events;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace VisualKanbanWebAPI.Hubs
{
    public class DeviceStatusChangedHandler : INotificationHandler<DeviceStatusChangedNotification>
    {
        private readonly IHubContext<SignalRHub> _hubContext;

        public DeviceStatusChangedHandler(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Handle(DeviceStatusChangedNotification notification, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.All.SendAsync("DeviceStatusUpdated", notification.Device);
        }
    }
}
