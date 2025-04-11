using Microsoft.EntityFrameworkCore;
using QuizTelegramApp.Data;
using QuizTelegramApp.Services;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

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

builder.Services.AddHostedService<TelegramBotService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapControllers();

// Маршрутизация для Telegram webhook
app.MapPost("/api/webhook", async (HttpContext context) =>
{
    var botService = context.RequestServices.GetRequiredService<ITelegramBotService>();
    var botClient = context.RequestServices.GetRequiredService<ITelegramBotClient>();
    
    using var reader = new StreamReader(context.Request.Body);
    var requestBody = await reader.ReadToEndAsync();
    var update = JsonConvert.DeserializeObject<Update>(requestBody);
    
    if (update != null)
    {
        await botService.HandleUpdateAsync(botClient, update, context.RequestAborted);
    }
    
    return Results.Ok();
});

// Маршрутизация для веб-приложения
app.MapFallbackToPage("/_Host");

app.Run();
