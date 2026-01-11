using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.Exams.SaveExamSubmissionAnswer;

public sealed class SaveExamSubmissionAnswerCommandHandler(
    IDbContext context
) : ICommandHandler<SaveExamSubmissionAnswerCommand, SaveExamSubmissionAnswerResponse>
{
    public async Task<Result<SaveExamSubmissionAnswerResponse>> Handle(
        SaveExamSubmissionAnswerCommand command,
        CancellationToken cancellationToken)
    {
        // Check if submission exists and is in progress
        var submission = await context.ExamSubmissions
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure<SaveExamSubmissionAnswerResponse>(
                ExamSubmissionErrors.NotFound(command.SubmissionId));
        }

        if (submission.Status != ExamSubmissionStatus.InProgress)
        {
            return Result.Failure<SaveExamSubmissionAnswerResponse>(
                ExamSubmissionErrors.AlreadySubmitted);
        }

        // Check if question exists
        var question = await context.ExamQuestions
            .FirstOrDefaultAsync(q => q.Id == command.QuestionId && q.ExamId == submission.ExamId, cancellationToken);

        if (question is null)
        {
            return Result.Failure<SaveExamSubmissionAnswerResponse>(
                ExamSubmissionErrors.QuestionNotFound(command.QuestionId));
        }

        var now = DateTime.UtcNow;

        // Check if answer already exists
        var existingAnswer = await context.ExamSubmissionAnswers
            .FirstOrDefaultAsync(a => a.SubmissionId == command.SubmissionId && 
                                     a.QuestionId == command.QuestionId, 
                                     cancellationToken);

        if (existingAnswer != null)
        {
            // Update existing answer
            existingAnswer.Answer = command.Answer;
            existingAnswer.AnsweredAt = now;

            // Auto-grade for multiple choice
            if (question.QuestionType == QuestionType.MultipleChoice)
            {
                existingAnswer.IsCorrect = string.Equals(
                    existingAnswer.Answer?.Trim(), 
                    question.CorrectAnswer?.Trim(), 
                    StringComparison.OrdinalIgnoreCase);
                
                if (existingAnswer.IsCorrect)
                {
                    existingAnswer.PointsAwarded = question.Points;
                }
                else
                {
                    existingAnswer.PointsAwarded = 0;
                }
            }
        }
        else
        {
            // Create new answer
            var answer = new ExamSubmissionAnswer
            {
                Id = Guid.NewGuid(),
                SubmissionId = command.SubmissionId,
                QuestionId = command.QuestionId,
                Answer = command.Answer,
                AnsweredAt = now
            };

            // Auto-grade for multiple choice
            if (question.QuestionType == QuestionType.MultipleChoice)
            {
                answer.IsCorrect = string.Equals(
                    answer.Answer?.Trim(), 
                    question.CorrectAnswer?.Trim(), 
                    StringComparison.OrdinalIgnoreCase);
                
                if (answer.IsCorrect)
                {
                    answer.PointsAwarded = question.Points;
                }
                else
                {
                    answer.PointsAwarded = 0;
                }
            }

            context.ExamSubmissionAnswers.Add(answer);
            existingAnswer = answer;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new SaveExamSubmissionAnswerResponse
        {
            Id = existingAnswer.Id,
            SubmissionId = existingAnswer.SubmissionId,
            QuestionId = existingAnswer.QuestionId,
            Answer = existingAnswer.Answer,
            AnsweredAt = existingAnswer.AnsweredAt
        };
    }
}


