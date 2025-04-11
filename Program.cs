using Microsoft.EntityFrameworkCore;
using QuizTelegramApp.Data;
using QuizTelegramApp.Services;
using QuizTelegramApp.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Настраиваем порт из переменной окружения
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new DefaultNamingStrategy()
        };
        options.SerializerSettings.StringEscapeHandling = StringEscapeHandling.Default;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

// Configure database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрируем DbContextOptions как singleton
builder.Services.AddSingleton<DbContextOptions<ApplicationDbContext>>(sp =>
{
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    return optionsBuilder.Options;
});

// Регистрируем фабрику как singleton
builder.Services.AddSingleton<IDbContextFactory, DbContextFactory>();

// Configure Telegram bot
builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var token = builder.Configuration["TelegramBot:Token"] ?? throw new InvalidOperationException("Telegram bot token is not configured");
        var options = new TelegramBotClientOptions(token);
        return new TelegramBotClient(options, httpClient);
    });

// Регистрируем TelegramBotService как IHostedService и ITelegramBotService
var descriptor = new ServiceDescriptor(
    typeof(ITelegramBotService),
    sp => sp.GetRequiredService<IHostedService>() as TelegramBotService ?? 
          throw new InvalidOperationException("Failed to resolve TelegramBotService"),
    ServiceLifetime.Singleton
);

builder.Services.AddHostedService<TelegramBotService>();
builder.Services.Add(descriptor);

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database_health_check");

var app = builder.Build();

// Добавляем тестовый квиз при запуске
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!dbContext.Quizzes.Any())
    {
        var testQuiz = new Quiz
        {
            Title = "Тестовый квиз",
            Description = "Это тестовый квиз для проверки работы приложения",
            Questions = new List<Question>
            {
                new Question
                {
                    Text = "Какая столица России?",
                    Options = System.Text.Json.JsonSerializer.Serialize(new[] { "Москва", "Санкт-Петербург", "Новосибирск", "Екатеринбург" }),
                    CorrectAnswer = "Москва"
                },
                new Question
                {
                    Text = "Сколько планет в Солнечной системе?",
                    Options = System.Text.Json.JsonSerializer.Serialize(new[] { "7", "8", "9", "10" }),
                    CorrectAnswer = "8"
                }
            }
        };
        dbContext.Quizzes.Add(testQuiz);
        dbContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// Добавляем эндпоинт для корневого пути
app.MapGet("/", () => Results.Ok(new { status = "healthy" }));

// Добавляем эндпоинт для проверки работоспособности
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.MapBlazorHub();
app.MapControllers();

// Маршрутизация для Telegram webhook
app.MapPost("/api/webhook", async (HttpContext context) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var botService = context.RequestServices.GetRequiredService<ITelegramBotService>();
    var botClient = context.RequestServices.GetRequiredService<ITelegramBotClient>();
    
    try 
    {
        logger.LogInformation("Получен webhook запрос от {RemoteIpAddress}", context.Connection.RemoteIpAddress);
        logger.LogInformation("Headers: {Headers}", string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}")));
        
        using var reader = new StreamReader(context.Request.Body);
        var requestBody = await reader.ReadToEndAsync();
        logger.LogInformation("Получен webhook: {RequestBody}", requestBody);
        
        var update = JsonConvert.DeserializeObject<Update>(requestBody);
        logger.LogInformation("Десериализовано обновление: {Update}", JsonConvert.SerializeObject(update));
        
        if (update != null)
        {
            await botService.HandleUpdateAsync(botClient, update, context.RequestAborted);
        }
        else
        {
            logger.LogWarning("Не удалось десериализовать обновление");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка при обработке webhook");
    }
    
    return Results.Ok();
});

// Эндпоинт для проверки webhook
app.MapGet("/api/webhook-info", async (HttpContext context) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var botClient = context.RequestServices.GetRequiredService<ITelegramBotClient>();
    
    try
    {
        var webhookInfo = await botClient.GetWebhookInfoAsync();
        logger.LogInformation("Информация о webhook: {WebhookInfo}", JsonConvert.SerializeObject(webhookInfo));
        return Results.Json(webhookInfo);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка при получении информации о webhook");
        return Results.Problem("Ошибка при получении информации о webhook");
    }
});

// Маршрутизация для веб-приложения
app.MapFallbackToPage("/_Host");

app.Run();
