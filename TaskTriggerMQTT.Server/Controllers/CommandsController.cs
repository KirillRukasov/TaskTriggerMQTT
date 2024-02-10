using Microsoft.AspNetCore.Mvc;
using TaskTriggerMQTT.Server.Services;

namespace TaskTriggerMQTT.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommandsController : ControllerBase
    {
        private readonly RabbitMQService _rabbitMQService;

        public CommandsController(RabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        [HttpPost("send")]
        public IActionResult SendCommand([FromBody] CommandModel command)
        {
            _rabbitMQService.SendMessageAsync(command.ButtonNumber.ToString());
            return Ok($"Command {command.ButtonNumber} is sent.");
        }

        [HttpGet("sendtest")]
        public IActionResult SendTest()
        {
            _rabbitMQService.SendTestMessageAsync();
            return Ok("Test message sent.");
        }
    }

    public class CommandModel
    {
        public int ButtonNumber { get; set; }
    }
}
