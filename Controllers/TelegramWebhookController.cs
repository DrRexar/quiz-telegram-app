using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using QuizTelegramApp.Services;

namespace QuizTelegramApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramWebhookController : ControllerBase
    {
        private readonly ITelegramBotService _botService;
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramWebhookController> _logger;

        public TelegramWebhookController(
            ITelegramBotService botService,
            ITelegramBotClient botClient,
            ILogger<TelegramWebhookController> logger)
        {
            _botService = botService;
            _botClient = botClient;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            try
            {
                _logger.LogInformation("=== Начало обработки вебхука в контроллере ===");
                _logger.LogInformation("Тип обновления: {UpdateType}", update.Type);
                
                if (update.Message != null)
                {
                    _logger.LogInformation("Сообщение от: {FromId}, Текст: {Text}", 
                        update.Message.From?.Id, update.Message.Text);
                }

                await _botService.HandleUpdateAsync(_botClient, update, HttpContext.RequestAborted);
                
                _logger.LogInformation("=== Успешная обработка вебхука в контроллере ===");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке вебхука в контроллере");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
} 