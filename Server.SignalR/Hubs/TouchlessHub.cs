using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Server.SignalR.Hubs
{
    public class TouchlessHub : Hub
    {
        private readonly ILogger<TouchlessHub> _logger;

        public TouchlessHub(ILogger<TouchlessHub> logger)
        {
            _logger = logger;
        }

        public static string ClientEventHandlerName = "OnEvent";

        public async Task FloorCall(string floorId)
        {
            _logger.LogInformation("Floor call. Id {FloorId}", floorId);
            // TODO: logic here
            string groupName = "Elevator1"; // TODO
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task CabinCall(string floorId, int destinationFloor)
        {
            _logger.LogInformation("Cabin call. Id {FloorId} to {Floor}", floorId, destinationFloor);
            // TODO: logic here
            string groupName = "Elevator1"; // TODO
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected. Connection: {ConnectionId}; User: {UserIdentifier}", Context.ConnectionId, Context.UserIdentifier);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation("Client disconnected. Connection: {ConnectionId}; User: {UserIdentifier}", Context.ConnectionId, Context.UserIdentifier);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
