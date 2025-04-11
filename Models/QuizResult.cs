using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizTelegramApp.Models;

[Table("QuizResults")]
public class QuizResult
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    [Required]
    public int Score { get; set; }

    [Required]
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
} 