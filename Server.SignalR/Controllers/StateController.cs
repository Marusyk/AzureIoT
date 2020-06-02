using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Server.SignalR.Controllers
{
    [Route("[controller]")]
    public class StateController : ControllerBase
    {

        private readonly ILogger<StateController> _logger;
        private readonly INotificationManager _notificationManager;

        public StateController(ILogger<StateController> logger, INotificationManager notificationManager)
        {
            _logger = logger;
            _notificationManager = notificationManager;
        }

        [HttpGet("{position}")]
        public async Task<IActionResult> ChangeCurrentPosition(int position)
        {
            await _notificationManager.Send("Elevator1", EventType.ElevatorState, new { position = position, health = "OK" });
            return Ok();
        }
    }
}
