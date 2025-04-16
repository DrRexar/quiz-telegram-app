using Microsoft.EntityFrameworkCore;
using QuizTelegramApp.Data;
using QuizTelegramApp.Services;
using Microsoft.OpenApi.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text.Json;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы в контейнер
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Quiz Telegram Bot API", Version = "v1" });
});

// Настройка логирования
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Настройка базы данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(60);
    });
});

// Регистрация сервисов
builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();
builder.Services.AddScoped<IDbContextFactory, DbContextFactory>();

// Настройка HttpClient для Telegram API
builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var token = builder.Configuration["TelegramBot:Token"] ?? throw new InvalidOperationException("Telegram bot token is not configured");
        var options = new TelegramBotClientOptions(token);
        return new TelegramBotClient(options, httpClient);
    });

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Настройка конфигурации
var configuration = app.Services.GetRequiredService<IConfiguration>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Настройка middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Настройка обработки вебхуков
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Проверяем, что запрос идет к нашему вебхуку
        if (context.Request.Path.StartsWithSegments("/api/webhook"))
        {
            logger.LogInformation("=== Начало обработки вебхука ===");
            
            // Получаем IP-адрес из заголовка X-Forwarded-For
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            var remoteIp = context.Connection.RemoteIpAddress;
            
            logger.LogInformation($"Remote IP: {remoteIp}, X-Forwarded-For: {forwardedFor}");
            
            // Проверяем IP-адрес
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var telegramIps = new[]
                {
                    "149.154.160.0/20",
                    "91.108.4.0/22",
                    "91.108.56.0/22"
                };

                var isFromTelegram = telegramIps.Any(ipRange => 
                    IsInRange(forwardedFor, ipRange));

                if (!isFromTelegram)
                {
                    logger.LogWarning($"Запрос не от Telegram: {forwardedFor}");
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }
            else
            {
                logger.LogWarning("Отсутствует заголовок X-Forwarded-For");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            // Читаем тело запроса
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            logger.LogInformation($"Тело запроса: {body}");

            try
            {
                // Десериализуем обновление
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var update = JsonSerializer.Deserialize<Update>(body, options);
                if (update != null)
                {
                    logger.LogInformation($"Тип обновления: {update.Type}");
                    
                    if (update.Message != null)
                    {
                        logger.LogInformation($"Сообщение от {update.Message.From?.Username} (ID: {update.Message.From?.Id}): {update.Message.Text}");
                    }
                }

                // Обрабатываем обновление
                var botService = context.RequestServices.GetRequiredService<ITelegramBotService>();
                var botClient = context.RequestServices.GetRequiredService<ITelegramBotClient>();
                await botService.HandleUpdateAsync(botClient, update, CancellationToken.None);

                context.Response.StatusCode = StatusCodes.Status200OK;
                logger.LogInformation("=== Успешная обработка вебхука ===");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при обработке вебхука");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
            return;
        }
        
        await next();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка при обработке вебхука");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Internal server error");
    }
});

// Настройка базы данных
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        logger.LogInformation("База данных успешно мигрирована");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка при миграции базы данных");
    }
}

// Запуск приложения
logger.LogInformation("Запуск приложения...");
app.Run();

public partial class Program
{
    public static void Main(string[] args)
    {
        // ... existing code ...
    }

    private static bool IsInRange(string ipAddress, string ipRange)
    {
        var parts = ipRange.Split('/');
        if (parts.Length != 2) return false;

        var ip = IPAddress.Parse(parts[0]);
        var maskBits = int.Parse(parts[1]);
        
        var ipBytes = ip.GetAddressBytes();
        var ipAddressBytes = IPAddress.Parse(ipAddress).GetAddressBytes();
        
        if (ipBytes.Length != ipAddressBytes.Length) return false;
        
        var maskBytes = new byte[ipBytes.Length];
        for (int i = 0; i < ipBytes.Length; i++)
        {
            if (maskBits >= 8)
            {
                maskBytes[i] = 0xFF;
                maskBits -= 8;
            }
            else if (maskBits > 0)
            {
                maskBytes[i] = (byte)(0xFF << (8 - maskBits));
                maskBits = 0;
            }
        }
        
        for (int i = 0; i < ipBytes.Length; i++)
        {
            if ((ipBytes[i] & maskBytes[i]) != (ipAddressBytes[i] & maskBytes[i]))
            {
                return false;
            }
        }
        
        return true;
    }
}
