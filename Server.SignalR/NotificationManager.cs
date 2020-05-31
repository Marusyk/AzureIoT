using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Server.SignalR.Hubs;
using System.Threading.Tasks;

namespace Server.SignalR
{
    public class NotificationManager : INotificationManager
    {
        private readonly IHubContext<TouchlessHub> _hubContext;

        public NotificationManager(IHubContext<TouchlessHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Send(string elevatorId, EventType eventType, object payload)
        {
            var message = JsonConvert.SerializeObject(new
            {
                EventType = eventType.ToString().ToLowerInvariant(),
                Data = payload
            });
            await _hubContext.Clients.Group(elevatorId).SendAsync(TouchlessHub.ClientEventHandlerName, message);
        }
    }
}
