using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace QuizTelegramApp.Models;

[Table("Questions")]
public class Question
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;

    [Required]
    public string Options { get; set; } = string.Empty;

    [Required]
    public string CorrectAnswer { get; set; } = string.Empty;

    [Required]
    [Range(1, 100)]
    public int Points { get; set; }

    public int QuizId { get; set; }

    [JsonIgnore]
    [ForeignKey("QuizId")]
    public Quiz Quiz { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    [JsonIgnore]
    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();
} 