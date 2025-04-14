using Microsoft.EntityFrameworkCore;
using QuizTelegramApp.Data;
using QuizTelegramApp.Services;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы в контейнер
builder.Services.AddControllers();
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
builder.Services.AddScoped<TelegramBotService>();
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
app.MapPost("/api/webhook", async (HttpContext context, TelegramBotService botService, ITelegramBotClient botClient) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("=== Начало обработки вебхука ===");
        logger.LogInformation("Remote IP: {RemoteIpAddress}", context.Connection.RemoteIpAddress);
        logger.LogInformation("Headers: {Headers}", context.Request.Headers);

        // Проверяем, что запрос пришел от Telegram
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        if (!userAgent.Contains("TelegramBot"))
        {
            logger.LogWarning("Получен запрос не от Telegram: {UserAgent}", userAgent);
            return Results.BadRequest("Unauthorized");
        }

        // Читаем тело запроса
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        logger.LogInformation("Request body: {Body}", body);

        // Десериализуем обновление
        var update = JsonSerializer.Deserialize<Update>(body);
        if (update == null)
        {
            logger.LogWarning("Не удалось десериализовать обновление");
            return Results.BadRequest("Invalid update");
        }

        logger.LogInformation("Тип обновления: {UpdateType}", update.Type);
        if (update.Message != null)
        {
            logger.LogInformation("Сообщение от: {FromId}, Текст: {Text}", 
                update.Message.From?.Id, update.Message.Text);
        }

        // Обрабатываем обновление
        await botService.HandleUpdateAsync(botClient, update, context.RequestAborted);

        logger.LogInformation("=== Успешная обработка вебхука ===");
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка при обработке вебхука");
        return Results.Problem("Internal server error");
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
