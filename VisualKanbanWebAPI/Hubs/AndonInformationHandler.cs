using Application.Events;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace VisualKanbanWebAPI.Hubs
{
    public class AndonInformationHandler : INotificationHandler<AndonInformationNotification>
    {
        
        private readonly IHubContext<SignalRHub> _hubContext;

        public AndonInformationHandler(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task Handle(AndonInformationNotification notification, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.All.SendAsync("UploadAndon", notification.AndonData);
        }

    }
}
