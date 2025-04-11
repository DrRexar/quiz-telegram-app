using Microsoft.AspNetCore.Mvc;
using QuizTelegramApp.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Text;
using Newtonsoft.Json;

namespace QuizTelegramApp.Controllers;

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
            await _botService.HandleUpdateAsync(_botClient, update, HttpContext.RequestAborted);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке webhook");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "ok", time = DateTime.UtcNow });
    }
} 