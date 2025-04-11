using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizTelegramApp.Models;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    public long TelegramId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    public List<Answer> Answers { get; set; } = new();
    public List<QuizResult> QuizResults { get; set; } = new();
} 