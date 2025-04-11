using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

public interface ITelegramBotService
{
    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    Task HandleStartCommand(long chatId);
    Task HandleQuizzesCommand(long chatId);
    Task HandleLeaderboardCommand(long chatId);
    Task HandleQuizSelection(long chatId, int quizId);
    Task HandleAnswer(long chatId, int quizId, int questionId, string answer, string username);
} 