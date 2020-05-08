using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Elevator.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElevatorController : ControllerBase
    {
        private readonly ILogger<ElevatorController> _logger;
        private readonly ServiceClient _serviceClient;

        public ElevatorController(ServiceClient serviceClient, ILogger<ElevatorController> logger)
        {
            _serviceClient = serviceClient;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(CallCommand command)
        {
            var methodInvocation = new CloudToDeviceMethod("FloorCall", TimeSpan.FromSeconds(30));
            methodInvocation.SetPayloadJson(command.Floor.ToString());
            var response = await _serviceClient.InvokeDeviceMethodAsync(command.DeviceName, methodInvocation);
            return StatusCode(response.Status, response.GetPayloadAsJson());
        }
    }

    public class CallCommand
    {
        [Required]
        public int? Floor { get; set; }

        [Required]
        public string DeviceName { get; set; }
    }
}
