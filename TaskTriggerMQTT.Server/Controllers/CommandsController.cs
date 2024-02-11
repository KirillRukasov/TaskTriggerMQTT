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
        public async Task<IActionResult> SendCommand([FromBody] CommandModel command)
        {
            await _rabbitMQService.ConnectAsync();
            _rabbitMQService.SendMessageAsync(command.ButtonNumber.ToString());
            return Ok($"Command {command.ButtonNumber} is sent.");
        }

        [HttpPost("send_and_wait")]
        public async Task<IActionResult> SendCommandAndWaitForResponse([FromBody] CommandModel command)
        {
            await _rabbitMQService.ConnectAsync();
            try
            {
                await _rabbitMQService.SendMessageAsync(command.ButtonNumber.ToString());

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var response = await _rabbitMQService.WaitForResponseAsync(cts.Token);

                return Ok($"Received response: {response}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("sendtest")]
        public async Task<IActionResult> SendTest()
        {
            await _rabbitMQService.ConnectAsync();
            _rabbitMQService.SendTestMessageAsync();
            return Ok("Test message sent.");
        }
    }

    public class CommandModel
    {
        public int ButtonNumber { get; set; }
    }
}
