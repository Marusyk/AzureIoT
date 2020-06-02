using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Mobile.SignalR.Controllers
{
    [Route("[controller]")]
    public class CallController : ControllerBase
    {
        private readonly HubConnection _hubConnection;

        public CallController(ILogger<CallController> logger, HubConnection hubConnection)
        {
            _hubConnection = hubConnection;
        }

        [HttpGet("{floorId}")]
        public async Task<IActionResult> Get(string floorId)
        {
            await _hubConnection.InvokeAsync("FloorCall", floorId);
            return Ok();
        }

        [HttpGet("{floorId}/{destinationFloor}")]
        public async Task<IActionResult> Get(string floorId, int destinationFloor)
        {
            await _hubConnection.InvokeAsync("CabinCall", floorId, destinationFloor);
            return Ok();
        }
    }
}
