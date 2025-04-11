using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizTelegramApp.Data;
using QuizTelegramApp.Models;

namespace QuizTelegramApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QuizController> _logger;

    public QuizController(ApplicationDbContext context, ILogger<QuizController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Quiz>>> GetQuizzes()
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.QuestionOptions)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Quiz>> GetQuiz(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.QuestionOptions)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
        {
            return NotFound();
        }

        return quiz;
    }

    [HttpPost]
    public async Task<ActionResult<Quiz>> CreateQuiz(Quiz quiz)
    {
        quiz.CreatedAt = DateTime.UtcNow;
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, quiz);
    }

    [HttpPost("{quizId}/questions")]
    public async Task<ActionResult<Question>> AddQuestion(int quizId, Question question)
    {
        var quiz = await _context.Quizzes.FindAsync(quizId);
            
        if (quiz == null)
        {
            return NotFound("Квиз не найден");
        }

        question.QuizId = quizId;
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
    }

    [HttpGet("questions/{id}")]
    public async Task<ActionResult<Question>> GetQuestion(int id)
    {
        var question = await _context.Questions
            .Include(q => q.QuestionOptions)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
        {
            return NotFound();
        }

        return question;
    }

    [HttpPut("questions/{id}")]
    public async Task<IActionResult> UpdateQuestion(int id, Question question)
    {
        if (id != question.Id)
        {
            return BadRequest();
        }

        var existingQuestion = await _context.Questions
            .Include(q => q.QuestionOptions)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (existingQuestion == null)
        {
            return NotFound();
        }

        // Обновляем основные свойства
        existingQuestion.Text = question.Text;
        existingQuestion.CorrectAnswer = question.CorrectAnswer;
        existingQuestion.Points = question.Points;

        // Обновляем опции
        existingQuestion.QuestionOptions.Clear();
        foreach (var option in question.QuestionOptions)
        {
            existingQuestion.QuestionOptions.Add(new QuestionOption { Text = option.Text });
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("questions/{id}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
        {
            return NotFound();
        }

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuiz(int id, Quiz quiz)
    {
        if (id != quiz.Id)
        {
            return BadRequest();
        }

        _context.Entry(quiz).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!QuizExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        var quiz = await _context.Quizzes.FindAsync(id);
        if (quiz == null)
        {
            return NotFound();
        }

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool QuizExists(int id)
    {
        return _context.Quizzes.Any(e => e.Id == id);
    }
} 