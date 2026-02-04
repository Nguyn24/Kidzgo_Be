using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Exercises.CreateExercise;
using Kidzgo.Application.Exercises.GetExerciseById;
using Kidzgo.Application.Exercises.GetExercises;
using Kidzgo.Application.Exercises.SoftDeleteExercise;
using Kidzgo.Application.Exercises.UpdateExercise;
using Kidzgo.Application.Exercises.Questions.CreateExerciseQuestion;
using Kidzgo.Application.Exercises.Questions.DeleteExerciseQuestion;
using Kidzgo.Application.Exercises.Questions.UpdateExerciseQuestion;
using Kidzgo.Application.Exercises.Submissions.AutoGradeMultipleChoice;
using Kidzgo.Application.Exercises.Submissions.GetAnswerDetail;
using Kidzgo.Application.Exercises.Submissions.GradeTextInput;
using Kidzgo.Application.Exercises.Submissions.SetAnswerFeedback;
using Kidzgo.Application.Exercises.Submissions.SubmitExerciseSubmission;
using Kidzgo.Domain.Exams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/exercises")]
[ApiController]
public class ExerciseController : ControllerBase
{
    private readonly ISender _mediator;

    public ExerciseController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-134: Tạo Exercise (READING/LISTENING/WRITING)
    [HttpPost]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> CreateExercise(
        [FromBody] CreateExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateExerciseCommand
        {
            ClassId = request.ClassId,
            MissionId = request.MissionId,
            Title = request.Title,
            Description = request.Description,
            ExerciseType = request.ExerciseType,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/exercises/{r.Id}");
    }

    /// UC-135: Xem danh sách Exercises
    [HttpGet]
    [Authorize]
    public async Task<IResult> GetExercises(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? missionId,
        [FromQuery] ExerciseType? exerciseType,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExercisesQuery
        {
            ClassId = classId,
            MissionId = missionId,
            ExerciseType = exerciseType,
            IsActive = isActive,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-136: Xem chi tiết Exercise
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IResult> GetExerciseById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetExerciseByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-137: Cập nhật Exercise
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> UpdateExercise(
        Guid id,
        [FromBody] UpdateExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExerciseCommand
        {
            Id = id,
            ClassId = request.ClassId,
            MissionId = request.MissionId,
            Title = request.Title,
            Description = request.Description,
            ExerciseType = request.ExerciseType,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-138: Xóa mềm Exercise
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> SoftDeleteExercise(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeleteExerciseCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-139: Tạo Exercise Question (MULTIPLE_CHOICE/TEXT_INPUT)
    [HttpPost("{exerciseId:guid}/questions")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> CreateExerciseQuestion(
        Guid exerciseId,
        [FromBody] CreateExerciseQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateExerciseQuestionCommand
        {
            ExerciseId = exerciseId,
            OrderIndex = request.OrderIndex,
            QuestionText = request.QuestionText,
            QuestionType = request.QuestionType,
            Options = request.Options,
            CorrectAnswer = request.CorrectAnswer,
            Points = request.Points,
            Explanation = request.Explanation
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/exercises/{exerciseId}/questions/{r.Id}");
    }

    /// UC-140/142/143/144: Cập nhật Exercise Question (bao gồm correct_answer/points/options)
    [HttpPut("{exerciseId:guid}/questions/{questionId:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> UpdateExerciseQuestion(
        Guid exerciseId,
        Guid questionId,
        [FromBody] UpdateExerciseQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExerciseQuestionCommand
        {
            QuestionId = questionId,
            OrderIndex = request.OrderIndex,
            QuestionText = request.QuestionText,
            QuestionType = request.QuestionType,
            Options = request.Options,
            CorrectAnswer = request.CorrectAnswer,
            Points = request.Points,
            Explanation = request.Explanation
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-141: Xóa Exercise Question
    [HttpDelete("{exerciseId:guid}/questions/{questionId:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> DeleteExerciseQuestion(
        Guid exerciseId,
        Guid questionId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteExerciseQuestionCommand { QuestionId = questionId };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-147: Tự động chấm Multiple Choice
    [HttpPut("submissions/{submissionId:guid}/auto-grade-multiple-choice")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> AutoGradeMultipleChoice(
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var command = new AutoGradeMultipleChoiceCommand { SubmissionId = submissionId };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-148: Teacher chấm Text Input (Writing)
    [HttpPut("submissions/{submissionId:guid}/grade-text-input")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GradeTextInput(
        Guid submissionId,
        [FromBody] GradeTextInputRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GradeTextInputCommand
        {
            SubmissionId = submissionId,
            AnswerGrades = request.AnswerGrades.Select(a => new TextAnswerGradeItem
            {
                QuestionId = a.QuestionId,
                PointsAwarded = a.PointsAwarded,
                TeacherFeedback = a.TeacherFeedback
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-150: Xem chi tiết từng câu trả lời
    [HttpGet("submissions/{submissionId:guid}/answers/{questionId:guid}")]
    [Authorize]
    public async Task<IResult> GetAnswerDetail(
        Guid submissionId,
        Guid questionId,
        CancellationToken cancellationToken)
    {
        var query = new GetAnswerDetailQuery
        {
            SubmissionId = submissionId,
            QuestionId = questionId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-151: Nhập feedback cho câu trả lời
    [HttpPut("submissions/{submissionId:guid}/answers/{questionId:guid}/feedback")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> SetAnswerFeedback(
        Guid submissionId,
        Guid questionId,
        [FromBody] SetExerciseAnswerFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetAnswerFeedbackCommand
        {
            SubmissionId = submissionId,
            QuestionId = questionId,
            TeacherFeedback = request.TeacherFeedback
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-147: Auto grade Multiple Choice triggered on submit
    [HttpPost("submissions/{submissionId:guid}/submit")]
    [Authorize]
    public async Task<IResult> SubmitExerciseSubmission(
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var command = new SubmitExerciseSubmissionCommand { SubmissionId = submissionId };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}


