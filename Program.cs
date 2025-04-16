using Microsoft.EntityFrameworkCore;
using QuizTelegramApp.Data;
using QuizTelegramApp.Services;
using Microsoft.OpenApi.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;
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
app.MapPost("/webhook", async (HttpContext context, ITelegramBotService botService) =>
{
    logger.LogInformation("\n=== Начало обработки вебхука ===");
    
    var remoteIp = context.Connection.RemoteIpAddress;
    var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
    logger.LogInformation($"Remote IP: {remoteIp}, X-Forwarded-For: {forwardedFor}");

    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    logger.LogInformation($"Тело запроса: {body}");

    try
    {
        var update = Update.FromJson(body);
        
        if (update == null)
        {
            logger.LogError("Не удалось десериализовать обновление");
            return Results.BadRequest("Invalid update");
        }

        logger.LogInformation($"Тип обновления: {update.Type}");
        await botService.HandleUpdateAsync(update);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка при обработке вебхука");
        return Results.BadRequest($"Error: {ex.Message}");
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

public class UnixDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var unixTime = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
        }
        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var unixTime = new DateTimeOffset(value).ToUnixTimeSeconds();
        writer.WriteNumberValue(unixTime);
    }
}
