namespace QuizTelegramApp.Models;

public class UserState
{
    public int? CurrentQuizId { get; set; }
    public int? CurrentQuestionId { get; set; }
    public int Score { get; set; }
    public DateTime? QuizStartTime { get; set; }
    public List<Answer> CurrentAnswers { get; set; } = new();
} 