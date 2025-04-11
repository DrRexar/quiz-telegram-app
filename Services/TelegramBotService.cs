using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuizTelegramApp.Data;
using QuizTelegramApp.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;

namespace QuizTelegramApp.Services;

public class TelegramBotService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IDbContextFactory _dbContextFactory;
    private readonly ILogger<TelegramBotService> _logger;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<long, UserState> _userStates = new();
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 1000;

    public TelegramBotService(
        ITelegramBotClient botClient,
        IDbContextFactory dbContextFactory,
        ILogger<TelegramBotService> logger,
        HttpClient httpClient)
    {
        _botClient = botClient;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    private ApplicationDbContext CreateDbContext()
    {
        return _dbContextFactory.CreateDbContext();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var me = await _botClient.GetMeAsync(stoppingToken);
            _logger.LogInformation("Бот {BotName} успешно запущен", me.Username);

            // Удаляем webhook перед использованием getUpdates
            await _botClient.DeleteWebhookAsync(cancellationToken: stoppingToken);

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                cancellationToken: stoppingToken
            );

            await Task.Delay(-1, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при запуске бота");
            throw;
        }
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.CallbackQuery != null)
            {
                await HandleCallbackQueryAsync(update.CallbackQuery);
                return;
            }

            if (update.Message?.Text == null)
            {
                _logger.LogWarning("Получено обновление без текста сообщения");
                return;
            }

            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            _logger.LogInformation("Обработка сообщения: {Message} от пользователя {UserId}", messageText, chatId);

            switch (messageText)
            {
                case "/start":
                    await HandleStartCommand(chatId);
                    break;

                case "/quizzes":
                    await HandleQuizzesCommand(chatId);
                    break;

                case "/leaderboard":
                    await HandleLeaderboardCommand(chatId);
                    break;

                case "/app":
                    await HandleAppCommand(chatId);
                    break;

                default:
                    if (messageText.StartsWith("/quiz_"))
                    {
                        var quizId = int.Parse(messageText.Split('_')[1]);
                        await HandleQuizSelection(chatId, quizId);
                    }
                    else if (messageText == "completed")
                    {
                        await SendMessageWithRetry(chatId, "Этот квиз уже пройден. Выберите другой квиз.");
                    }
                    else
                    {
                        await SendMessageWithRetry(chatId, "Неизвестная команда. Используйте /start для начала работы.");
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке обновления");
            if (update.Message?.Chat.Id != null)
            {
                await SendMessageWithRetry(update.Message.Chat.Id, "Произошла ошибка при обработке вашего запроса. Пожалуйста, попробуйте позже.");
            }
        }
    }

    private async Task SendMessageWithRetry(long chatId, string text, IReplyMarkup? replyMarkup = null)
    {
        for (int i = 0; i < MaxRetries; i++)
        {
            try
            {
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    replyMarkup: replyMarkup,
                    parseMode: ParseMode.Html
                );
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Попытка {Attempt} из {MaxRetries} отправить сообщение не удалась", i + 1, MaxRetries);
                if (i < MaxRetries - 1)
                {
                    await Task.Delay(RetryDelayMs * (i + 1));
                }
                else
                {
                    _logger.LogError(ex, "Не удалось отправить сообщение после {MaxRetries} попыток", MaxRetries);
                    throw;
                }
            }
        }
    }

    private async Task HandleStartCommand(long chatId)
    {
        var message = "Добро пожаловать в Quiz App! 🎯\n\n" +
                     "Доступные команды:\n" +
                     "/quizzes - Список доступных квизов\n" +
                     "/leaderboard - Таблица лидеров\n" +
                     "/app - Открыть веб-приложение";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithWebApp(
                    "📱 Открыть приложение",
                    new WebAppInfo { Url = "https://your-mini-app-url.com" })
            }
        });

        await SendMessageWithRetry(chatId, message, keyboard);
    }

    private async Task HandleQuizzesCommand(long chatId)
    {
        try 
        {
            using var context = CreateDbContext();
            var quizzes = await context.Quizzes.ToListAsync();
            
            if (!quizzes.Any())
            {
                await SendMessageWithRetry(chatId, "К сожалению, пока нет доступных квизов.");
                return;
            }

            var user = await context.Users.FirstOrDefaultAsync(u => u.TelegramId == chatId);
            var completedQuizIds = new HashSet<int>();
            
            if (user != null)
            {
                completedQuizIds = new HashSet<int>(await context.QuizResults
                    .Where(r => r.UserId == user.Id)
                    .Select(r => r.QuizId)
                    .ToListAsync());
            }

            var quizButtons = quizzes.Select(q => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{q.Title} {(completedQuizIds.Contains(q.Id) ? "✅" : "")}",
                    completedQuizIds.Contains(q.Id) ? "completed" : $"/quiz_{q.Id}")
            }).ToList();

            // Добавляем кнопку возврата в главное меню
            quizButtons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("🏠 Главное меню", "main_menu")
            });

            var keyboard = new InlineKeyboardMarkup(quizButtons);

            await SendMessageWithRetry(
                chatId,
                "Выберите квиз:\n(✅ - пройденные квизы)",
                replyMarkup: keyboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке команды /quizzes");
            await SendMessageWithRetry(chatId, "Произошла ошибка при получении списка квизов. Пожалуйста, попробуйте позже.");
        }
    }

    private async Task HandleLeaderboardCommand(long chatId)
    {
        try
        {
            using var context = CreateDbContext();
            var quizzes = await context.Quizzes.ToListAsync();

            if (!quizzes.Any())
            {
                await SendMessageWithRetry(chatId, "Пока нет доступных квизов.");
                return;
            }

            var quizButtons = quizzes.Select(q => new[]
            {
                InlineKeyboardButton.WithCallbackData(q.Title, $"leaderboard_{q.Id}")
            }).ToList();

            // Добавляем кнопку возврата в главное меню
            quizButtons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("🏠 Главное меню", "main_menu")
            });

            var keyboard = new InlineKeyboardMarkup(quizButtons);

            await SendMessageWithRetry(
                chatId,
                "Выберите квиз для просмотра таблицы лидеров:",
                replyMarkup: keyboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка квизов для таблицы лидеров: {Error}", ex.Message);
            await SendMessageWithRetry(chatId, "Произошла ошибка при получении списка квизов. Пожалуйста, попробуйте позже.");
        }
    }

    private async Task ShowQuizLeaderboard(long chatId, int quizId)
    {
        try
        {
            using var context = CreateDbContext();
            var quiz = await context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);
            if (quiz == null)
            {
                await SendMessageWithRetry(chatId, "Квиз не найден.");
                return;
            }

            var allResults = await context.QuizResults
                .Include(r => r.User)
                .Where(r => r.QuizId == quizId)
                .ToListAsync();

            var results = allResults
                .GroupBy(r => r.UserId)
                .Select(g => g.OrderByDescending(r => r.Score).First())
                .OrderByDescending(r => r.Score)
                .Take(10)
                .ToList();

            if (!results.Any())
            {
                await SendMessageWithRetry(chatId, $"Пока нет результатов для квиза \"{quiz.Title}\".");
                return;
            }

            var message = $"🏆 Таблица лидеров: {quiz.Title}\n\n" +
                         string.Join("\n", results.Select((r, i) =>
                             $"{i + 1}. {r.User?.Username ?? "Аноним"} - {r.Score} баллов"));

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("◀️ Назад к списку квизов", "back_to_leaderboards") },
                new[] { InlineKeyboardButton.WithCallbackData("🏠 Главное меню", "main_menu") }
            });

            await SendMessageWithRetry(chatId, message, keyboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении таблицы лидеров для квиза: {Error}", ex.Message);
            await SendMessageWithRetry(chatId, "Произошла ошибка при получении таблицы лидеров. Пожалуйста, попробуйте позже.");
        }
    }

    private async Task HandleQuizSelection(long chatId, int quizId)
    {
        try
        {
            using var context = CreateDbContext();
            
            // Проверяем, проходил ли пользователь этот квиз
            var user = await context.Users.FirstOrDefaultAsync(u => u.TelegramId == chatId);
            if (user != null)
            {
                var hasAttempt = await context.QuizResults
                    .AnyAsync(r => r.UserId == user.Id && r.QuizId == quizId);
                
                if (hasAttempt)
                {
                    await SendMessageWithRetry(chatId, "Вы уже проходили этот квиз. Выберите другой квиз с помощью команды /quizzes");
                    return;
                }
            }

            var quiz = await context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                _logger.LogWarning("Квиз не найден. ID: {QuizId}", quizId);
                await SendMessageWithRetry(chatId, "Квиз не найден.");
                return;
            }

            var firstQuestion = quiz.Questions.OrderBy(q => q.Id).FirstOrDefault();
            if (firstQuestion == null)
            {
                _logger.LogWarning("В квизе нет вопросов. ID: {QuizId}", quizId);
                await SendMessageWithRetry(chatId, "В этом квизе пока нет вопросов.");
                return;
            }

            await HandleQuestion(chatId, firstQuestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке выбора квиза");
            await SendMessageWithRetry(chatId, "Произошла ошибка при загрузке квиза. Пожалуйста, попробуйте позже.");
        }
    }

    private async Task HandleAnswer(long chatId, int quizId, int questionId, string answer, string username)
    {
        try
        {
            using var context = CreateDbContext();
            var quiz = await context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                _logger.LogWarning("Квиз не найден. ID: {QuizId}", quizId);
                await SendMessageWithRetry(chatId, "Квиз не найден.");
                return;
            }

            var currentQuestion = quiz.Questions.FirstOrDefault(q => q.Id == questionId);
            if (currentQuestion == null)
            {
                _logger.LogWarning("Вопрос не найден. ID: {QuestionId}", questionId);
                await SendMessageWithRetry(chatId, "Вопрос не найден.");
                return;
            }

            // Сохраняем ответ
            var user = await context.Users.FirstOrDefaultAsync(u => u.TelegramId == chatId);
            if (user == null)
            {
                user = new Models.User 
                { 
                    Username = username,
                    TelegramId = chatId
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            var answerEntity = new Models.Answer
            {
                QuestionId = questionId,
                UserId = user.Id,
                Text = answer,
                IsCorrect = answer == currentQuestion.CorrectAnswer,
                Question = currentQuestion,
                User = user
            };
            context.Answers.Add(answerEntity);
            await context.SaveChangesAsync();

            // Обновляем состояние пользователя
            if (!_userStates.TryGetValue(chatId, out var state))
            {
                state = new UserState
                {
                    CurrentQuizId = quizId,
                    CurrentQuestionId = questionId,
                    Score = 0
                };
            }

            if (answer == currentQuestion.CorrectAnswer)
            {
                state.Score++;
            }

            _userStates[chatId] = state;

            // Отправляем сообщение о правильности ответа
            var isCorrect = answer == currentQuestion.CorrectAnswer;
            await SendMessageWithRetry(
                chatId,
                isCorrect ? "✅ Правильно!" : $"❌ Неправильно. Правильный ответ: {currentQuestion.CorrectAnswer ?? "Неизвестно"}"
            );

            // Находим следующий вопрос
            var questions = quiz.Questions.OrderBy(q => q.Id).ToList();
            var currentIndex = questions.FindIndex(q => q.Id == questionId);
            var nextQuestion = currentIndex < questions.Count - 1 ? questions[currentIndex + 1] : null;

            if (nextQuestion != null)
            {
                // Обновляем состояние для следующего вопроса
                state.CurrentQuestionId = nextQuestion.Id;
                _userStates[chatId] = state;

                // Отправляем следующий вопрос
                await HandleQuestion(chatId, nextQuestion);
            }
            else
            {
                // Квиз завершен, показываем результаты
                await FinishQuizAsync(chatId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке ответа: {Error}", ex.Message);
            await SendMessageWithRetry(chatId, "Произошла ошибка при обработке ответа. Пожалуйста, попробуйте позже.");
        }
    }

    private async Task SendLeaderboard(long chatId, int quizId)
    {
        using var context = CreateDbContext();
        var results = await context.QuizResults
            .Include(r => r.User)
            .Include(r => r.Quiz)
            .Where(r => r.QuizId == quizId)
            .OrderByDescending(r => r.Score)
            .Take(10)
            .ToListAsync();

        if (!results.Any())
        {
            await _botClient.SendTextMessageAsync(chatId, "Пока нет результатов для этого квиза.");
            return;
        }

        var message = "🏆 Топ-10 результатов:\n\n";
        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            message += $"{i + 1}. {result.User?.Username ?? "Аноним"}: {result.Score}/{result.Quiz?.Questions?.Count ?? 0}\n";
        }

        await _botClient.SendTextMessageAsync(chatId, message);
    }

    private async Task SaveQuizResult(Models.User user, Quiz quiz, int score)
    {
        try
        {
            using var context = CreateDbContext();
            var result = new QuizResult
            {
                UserId = user.Id,
                User = user,
                QuizId = quiz.Id,
                Quiz = quiz,
                Score = score,
                CompletedAt = DateTime.UtcNow
            };

            await context.QuizResults.AddAsync(result);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении результата");
            throw;
        }
    }

    private async Task HandleQuestion(long chatId, Question question)
    {
        try
        {
            if (question == null)
            {
                _logger.LogWarning("Получен null вопрос для chatId: {ChatId}", chatId);
                await SendMessageWithRetry(chatId, "Ошибка: вопрос не найден.");
                return;
            }

            // Загружаем вопрос с вариантами ответов
            using var context = CreateDbContext();
            var loadedQuestion = await context.Questions
                .Include(q => q.QuestionOptions)
                .FirstOrDefaultAsync(q => q.Id == question.Id);

            if (loadedQuestion == null)
            {
                _logger.LogWarning("Вопрос не найден в базе данных. ID: {QuestionId}", question.Id);
                await SendMessageWithRetry(chatId, "Ошибка: вопрос не найден в базе данных.");
                return;
            }

            // Получаем варианты ответов
            string[] options;
            if (loadedQuestion.QuestionOptions != null && loadedQuestion.QuestionOptions.Any())
            {
                options = loadedQuestion.QuestionOptions
                    .Where(o => !string.IsNullOrEmpty(o.Text))
                    .Select(o => o.Text)
                    .ToArray();
            }
            else if (!string.IsNullOrEmpty(loadedQuestion.Options))
            {
                try
                {
                    options = System.Text.Json.JsonSerializer.Deserialize<string[]>(loadedQuestion.Options) ?? Array.Empty<string>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при десериализации вариантов ответов: {Error}", ex.Message);
                    options = loadedQuestion.Options
                        .Split(',')
                        .Select(o => o.Trim())
                        .Where(o => !string.IsNullOrEmpty(o))
                        .ToArray();
                }
            }
            else
            {
                _logger.LogWarning("У вопроса нет вариантов ответа. ID: {QuestionId}", loadedQuestion.Id);
                await SendMessageWithRetry(chatId, "Ошибка: у вопроса нет вариантов ответа.");
                return;
            }

            if (options == null || !options.Any())
            {
                _logger.LogWarning("Получен пустой массив вариантов ответов. ID: {QuestionId}", loadedQuestion.Id);
                await SendMessageWithRetry(chatId, "Ошибка: у вопроса нет вариантов ответа.");
                return;
            }

            // Создаем кнопки для вариантов ответов
            var keyboard = new InlineKeyboardMarkup(
                options.Select(o => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        o,
                        $"answer_{loadedQuestion.QuizId}_{loadedQuestion.Id}_{o}")
                }));

            // Отправляем вопрос с вариантами ответов
            await SendMessageWithRetry(
                chatId: chatId,
                text: loadedQuestion.Text ?? "Вопрос без текста",
                replyMarkup: keyboard
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке вопроса: {Error}", ex.Message);
            await SendMessageWithRetry(chatId, "Произошла ошибка при загрузке вопроса. Пожалуйста, попробуйте позже.");
        }
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
    {
        if (callbackQuery?.Data == null || callbackQuery.Message?.Chat == null)
        {
            _logger.LogWarning("Получен некорректный callback query");
            return;
        }

        var chatId = callbackQuery.Message.Chat.Id;
        var messageId = callbackQuery.Message.MessageId;

        try
        {
            if (callbackQuery.Data == "main_menu")
            {
                await HandleStartCommand(chatId);
            }
            else if (callbackQuery.Data.StartsWith("leaderboard_"))
            {
                var quizId = int.Parse(callbackQuery.Data.Split('_')[1]);
                await ShowQuizLeaderboard(chatId, quizId);
            }
            else if (callbackQuery.Data == "back_to_leaderboards")
            {
                await HandleLeaderboardCommand(chatId);
            }
            else if (callbackQuery.Data.StartsWith("answer_"))
            {
                var parts = callbackQuery.Data.Split('_');
                if (parts.Length != 4)
                {
                    _logger.LogWarning("Некорректный формат callback data: {Data}", callbackQuery.Data);
                    await SendMessageWithRetry(chatId, "Произошла ошибка при обработке ответа. Пожалуйста, попробуйте позже.");
                    return;
                }

                var quizId = int.Parse(parts[1]);
                var questionId = int.Parse(parts[2]);
                var answer = parts[3];
                var username = callbackQuery.From?.Username ?? "Anonymous";
                await HandleAnswer(chatId, quizId, questionId, answer, username);
            }
            else if (callbackQuery.Data.StartsWith("/quiz_"))
            {
                var quizId = int.Parse(callbackQuery.Data.Split('_')[1]);
                await StartQuizAsync(chatId, quizId);
            }

            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке callback query: {Error}", ex.Message);
            await SendMessageWithRetry(chatId, "Произошла ошибка при обработке вашего запроса. Пожалуйста, попробуйте позже.");
        }
    }

    private async Task StartQuizAsync(long chatId, int quizId)
    {
        try
        {
            using var context = CreateDbContext();
            var quiz = await context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                _logger.LogWarning("Квиз не найден. ID: {QuizId}", quizId);
                await SendMessageWithRetry(chatId, "Квиз не найден.");
                return;
            }

            if (quiz.Questions == null || !quiz.Questions.Any())
            {
                _logger.LogWarning("В квизе нет вопросов. ID: {QuizId}", quizId);
                await SendMessageWithRetry(chatId, "В этом квизе пока нет вопросов.");
                return;
            }

            var firstQuestion = quiz.Questions.OrderBy(q => q.Id).FirstOrDefault();
            if (firstQuestion == null)
            {
                _logger.LogWarning("Не удалось получить первый вопрос. ID: {QuizId}", quizId);
                await SendMessageWithRetry(chatId, "Ошибка при загрузке первого вопроса.");
                return;
            }

            // Инициализируем состояние пользователя
            _userStates[chatId] = new UserState
            {
                CurrentQuizId = quizId,
                CurrentQuestionId = firstQuestion.Id,
                Score = 0
            };

            await HandleQuestion(chatId, firstQuestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при запуске квиза: {Error}", ex.Message);
            await SendMessageWithRetry(chatId, "Произошла ошибка при запуске квиза. Пожалуйста, попробуйте позже.");
        }
    }

    private async Task HandleAnswerAsync(long chatId, int answerId)
    {
        try
        {
            if (!_userStates.TryGetValue(chatId, out var state))
            {
                await SendMessageWithRetry(chatId, "Сначала выберите квиз.");
                return;
            }

            using var context = CreateDbContext();
            var answer = await context.Answers
                .Include(a => a.Question)
                .FirstOrDefaultAsync(a => a.Id == answerId);

            if (answer == null)
            {
                await SendMessageWithRetry(chatId, "Ответ не найден.");
                return;
            }

            if (answer.QuestionId != state.CurrentQuestionId)
            {
                await SendMessageWithRetry(chatId, "Этот ответ не относится к текущему вопросу.");
                return;
            }

            state.CurrentAnswers.Add(answer);
            if (answer.IsCorrect)
            {
                state.Score++;
            }

            await SendMessageWithRetry(
                chatId,
                answer.IsCorrect ? "✅ Правильно!" : $"❌ Неправильно. Правильный ответ: {answer.Question?.CorrectAnswer ?? "Неизвестно"}"
            );

            await SendNextQuestionAsync(chatId, answer.QuestionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке ответа");
            await SendMessageWithRetry(chatId, "Произошла ошибка при обработке ответа. Пожалуйста, попробуйте позже.");
        }
    }

    private async Task SendNextQuestionAsync(long chatId, int currentQuestionId)
    {
        try
        {
            if (!_userStates.TryGetValue(chatId, out var state))
            {
                await SendMessageWithRetry(chatId, "Сначала выберите квиз.");
                return;
            }

            using var context = CreateDbContext();
            var quiz = await context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == state.CurrentQuizId);

            if (quiz == null)
            {
                await SendMessageWithRetry(chatId, "Квиз не найден.");
                return;
            }

            var questions = quiz.Questions.ToList();
            var currentIndex = questions.FindIndex(q => q.Id == currentQuestionId);
            var nextQuestion = currentIndex < questions.Count - 1 ? questions[currentIndex + 1] : null;

            if (nextQuestion != null)
            {
                state.CurrentQuestionId = nextQuestion.Id;
                await HandleQuestion(chatId, nextQuestion);
            }
            else
            {
                await FinishQuizAsync(chatId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке следующего вопроса");
            await SendMessageWithRetry(chatId, "Произошла ошибка при загрузке следующего вопроса. Пожалуйста, попробуйте позже.");
        }
    }

    private async Task FinishQuizAsync(long chatId)
    {
        try
        {
            if (!_userStates.TryGetValue(chatId, out var state))
            {
                await SendMessageWithRetry(chatId, "Сначала выберите квиз.");
                return;
            }

            using var context = CreateDbContext();
            var quiz = await context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == state.CurrentQuizId);

            if (quiz == null)
            {
                await SendMessageWithRetry(chatId, "Квиз не найден.");
                return;
            }

            var user = await context.Users.FirstOrDefaultAsync(u => u.TelegramId == chatId);
            if (user == null)
            {
                user = new Models.User { TelegramId = chatId };
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            var result = new QuizResult
            {
                UserId = user.Id,
                QuizId = quiz.Id,
                Score = state.Score,
                CompletedAt = DateTime.UtcNow
            };

            context.QuizResults.Add(result);
            await context.SaveChangesAsync();

            var message = $"🎉 Квиз завершен!\n\n" +
                         $"Ваш результат: {state.Score} из {quiz.Questions.Count} правильных ответов\n" +
                         $"Процент правильных ответов: {(state.Score * 100.0 / quiz.Questions.Count):F1}%";

            // Создаем клавиатуру с кнопкой возврата в главное меню
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("🏠 Главное меню", "main_menu") }
            });

            await SendMessageWithRetry(chatId, message, keyboard);
            _userStates.Remove(chatId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при завершении квиза");
            await SendMessageWithRetry(chatId, "Произошла ошибка при завершении квиза. Пожалуйста, попробуйте позже.");
        }
    }

    private async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(errorMessage);
        await Task.Delay(1000, cancellationToken);
    }

    private async Task HandleAppCommand(long chatId)
    {
        try
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithWebApp(
                        "📱 Открыть приложение",
                        new WebAppInfo { Url = "https://amvera-drrexar-run-quizbott.amvera.io" })
                }
            });

            await SendMessageWithRetry(
                chatId,
                "Нажмите на кнопку ниже, чтобы открыть веб-приложение:",
                keyboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке команды /app");
            await SendMessageWithRetry(chatId, "Произошла ошибка при открытии приложения. Пожалуйста, попробуйте позже.");
        }
    }
} 