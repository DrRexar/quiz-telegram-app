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

// Список IP-адресов Telegram
var telegramIps = new[]
{
    IPAddress.Parse("149.154.160.0"),
    IPAddress.Parse("91.108.4.0"),
    IPAddress.Parse("91.108.56.0")
};

// Настройка обработки вебхуков
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Проверяем, что запрос идет к нашему вебхуку
        if (context.Request.Path.StartsWithSegments("/api/telegramwebhook"))
        {
            logger.LogInformation("=== Начало обработки вебхука ===");
            
            // Получаем реальный IP-адрес клиента
            var remoteIp = context.Connection.RemoteIpAddress;
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            logger.LogInformation("Remote IP: {RemoteIp}, X-Forwarded-For: {ForwardedFor}", remoteIp, forwardedFor);

            // Проверяем IP-адрес
            if (!telegramIps.Any(ip => ip.GetAddressBytes().SequenceEqual(remoteIp.GetAddressBytes())))
            {
                logger.LogWarning("Запрос не от Telegram: {RemoteIp}", remoteIp);
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
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
