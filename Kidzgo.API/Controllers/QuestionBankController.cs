using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.QuestionBank;
using Kidzgo.Application.QuestionBank.CreateQuestionBankItems;
using Kidzgo.Application.QuestionBank.GetQuestionBank;
using Kidzgo.Application.QuestionBank.ImportQuestionBank;
using Kidzgo.Domain.Homework;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/question-bank")]
[ApiController]
[Authorize]
public class QuestionBankController : ControllerBase
{
    private readonly ISender _mediator;

    public QuestionBankController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create questions in question bank (manual input)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> CreateQuestionBankItems(
        [FromBody] CreateQuestionBankItemsRequest request,
        CancellationToken cancellationToken)
    {
        var items = new List<CreateQuestionBankItemDto>();

        for (int i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];

            if (!Enum.TryParse<HomeworkQuestionType>(item.QuestionType, ignoreCase: true, out var questionType))
            {
                return Results.BadRequest($"Invalid question type: {item.QuestionType}");
            }

            if (!Enum.TryParse<QuestionLevel>(item.Level, ignoreCase: true, out var level))
            {
                return Results.BadRequest($"Invalid level: {item.Level}");
            }

            items.Add(new CreateQuestionBankItemDto
            {
                QuestionText = item.QuestionText,
                QuestionType = questionType,
                Options = item.Options,
                CorrectAnswer = item.CorrectAnswer,
                Points = item.Points,
                Explanation = item.Explanation,
                Topic = item.Topic,
                Skill = item.Skill,
                GrammarTags = item.GrammarTags,
                VocabularyTags = item.VocabularyTags,
                Level = level
            });
        }

        var command = new CreateQuestionBankItemsCommand
        {
            ProgramId = request.ProgramId,
            Items = items
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// List question bank items by program
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetQuestionBankItems(
        [FromQuery] Guid? programId,
        [FromQuery] string? level,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        QuestionLevel? parsedLevel = null;
        if (!string.IsNullOrWhiteSpace(level))
        {
            if (!Enum.TryParse<QuestionLevel>(level, ignoreCase: true, out var tmpLevel))
            {
                return Results.BadRequest($"Invalid level: {level}");
            }
            parsedLevel = tmpLevel;
        }

        var query = new GetQuestionBankItemsQuery
        {
            ProgramId = programId,
            Level = parsedLevel,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Import question bank from CSV/Excel/Word/PDF file
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [RequestSizeLimit(20_971_520)]
    public async Task<IResult> ImportQuestionBank(
        [FromQuery] Guid programId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var command = new ImportQuestionBankFromFileCommand
        {
            ProgramId = programId,
            FileName = file.FileName,
            FileStream = file.OpenReadStream()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Generate draft question bank items with AI Creator
    /// </summary>
    [HttpPost("ai-generate")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GenerateQuestionBankItems(
        [FromBody] GenerateQuestionBankItemsRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<HomeworkQuestionType>(request.QuestionType, ignoreCase: true, out var questionType))
        {
            return Results.BadRequest($"Invalid question type: {request.QuestionType}");
        }

        if (!Enum.TryParse<QuestionLevel>(request.Level, ignoreCase: true, out var level))
        {
            return Results.BadRequest($"Invalid level: {request.Level}");
        }

        var taskStyle = string.IsNullOrWhiteSpace(request.TaskStyle)
            ? "standard"
            : request.TaskStyle.Trim().ToLowerInvariant();

        if (taskStyle is not ("standard" or "translation"))
        {
            return Results.BadRequest($"Invalid task style: {request.TaskStyle}");
        }

        var query = new GenerateAiQuestionBankItemsQuery
        {
            ProgramId = request.ProgramId,
            Topic = request.Topic,
            QuestionType = questionType,
            QuestionCount = request.QuestionCount,
            Level = level,
            Skill = request.Skill,
            TaskStyle = taskStyle,
            GrammarTags = request.GrammarTags,
            VocabularyTags = request.VocabularyTags,
            Instructions = request.Instructions,
            Language = request.Language,
            PointsPerQuestion = request.PointsPerQuestion
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}
