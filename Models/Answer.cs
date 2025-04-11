using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizTelegramApp.Models
{
    [Table("Answers")]
    public class Answer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        [Required]
        public bool IsCorrect { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    }
} 