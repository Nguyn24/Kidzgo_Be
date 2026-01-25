using Kidzgo.API.Extensions;
using Kidzgo.Application.Attendance.GetStudentAttendanceHistory;
using Kidzgo.Application.Exams.GetStudentExamResults;
using Kidzgo.Application.Invoices.GetParentInvoices;
using Kidzgo.Application.MakeupCredits.GetParentStudentsWithMakeupOrLeave;
using Kidzgo.Application.Media.GetMedia;
using Kidzgo.Application.Notifications.GetNotifications;
using Kidzgo.Application.Notifications.GetParentNotifications;
using Kidzgo.Application.Sessions.GetStudentTimetable;
using Kidzgo.Application.Users.GetParentOverview;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Media;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/parent")]
[ApiController]
[Authorize]
public class ParentController : ControllerBase
{
    private readonly ISender _mediator;

    public ParentController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// Dashboard tổng quan của phụ huynh (studentId lấy từ token)
    [HttpGet("overview")]
    public async Task<IResult> GetOverview(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? sessionId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = new GetParentOverviewQuery
        {
            ClassId = classId,
            SessionId = sessionId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Danh sách học viên của phụ huynh đang có đơn xin nghỉ hoặc đang có makeup credit
    /// <param name="searchTerm">Search by student display name</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet("students-with-makeup-or-leave")]
    public async Task<IResult> GetStudentsWithMakeupOrLeave(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetParentStudentsWithMakeupOrLeaveQuery
        {
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Lịch học của học viên (studentId lấy từ token)
    [HttpGet("timetable")]
    public async Task<IResult> GetTimetable(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentTimetableQuery
        {
            From = from,
            To = to
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Lịch sử điểm danh của học viên (studentId lấy từ token)
    [HttpGet("attendance")]
    public async Task<IResult> GetAttendance(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentAttendanceHistoryQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Kết quả kiểm tra của học viên (studentId lấy từ token)
    [HttpGet("exam-results")]
    public async Task<IResult> GetExamResults(
        [FromQuery] string? examType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        Kidzgo.Domain.Exams.ExamType? parsedExamType = null;
        if (!string.IsNullOrWhiteSpace(examType) && 
            Enum.TryParse<Kidzgo.Domain.Exams.ExamType>(examType, true, out var parsed))
        {
            parsedExamType = parsed;
        }

        var query = new GetStudentExamResultsQuery
        {
            ExamType = parsedExamType,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Hóa đơn của phụ huynh (tự động lọc theo parent hiện tại)
    [HttpGet("invoices")]
    public async Task<IResult> GetInvoices(
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        InvoiceStatus? invoiceStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && 
            Enum.TryParse<InvoiceStatus>(status, true, out var parsed))
        {
            invoiceStatus = parsed;
        }

        var query = new GetParentInvoicesByCurrentUserQuery
        {
            Status = invoiceStatus,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Tài liệu, album, video của học viên (studentId lấy từ token)
    [HttpGet("media")]
    public async Task<IResult> GetMedia(
        [FromQuery] Guid? classId,
        [FromQuery] string? monthTag,
        [FromQuery] string? type,
        [FromQuery] string? contentType,
        [FromQuery] string? visibility,
        [FromQuery] string? approvalStatus,
        [FromQuery] bool? isPublished,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        MediaType? parsedType = null;
        if (!string.IsNullOrWhiteSpace(type) && 
            Enum.TryParse<MediaType>(type, true, out var parsedTypeValue))
        {
            parsedType = parsedTypeValue;
        }

        MediaContentType? parsedContentType = null;
        if (!string.IsNullOrWhiteSpace(contentType) && 
            Enum.TryParse<MediaContentType>(contentType, true, out var parsedContentTypeValue))
        {
            parsedContentType = parsedContentTypeValue;
        }

        Visibility? parsedVisibility = null;
        if (!string.IsNullOrWhiteSpace(visibility) && 
            Enum.TryParse<Visibility>(visibility, true, out var parsedVisibilityValue))
        {
            parsedVisibility = parsedVisibilityValue;
        }

        ApprovalStatus? parsedApprovalStatus = null;
        if (!string.IsNullOrWhiteSpace(approvalStatus) && 
            Enum.TryParse<ApprovalStatus>(approvalStatus, true, out var parsedApprovalStatusValue))
        {
            parsedApprovalStatus = parsedApprovalStatusValue;
        }

        var query = new GetMediaQuery(
            BranchId: null,
            ClassId: classId,
            MonthTag: monthTag,
            Type: parsedType,
            ContentType: parsedContentType,
            Visibility: parsedVisibility,
            ApprovalStatus: parsedApprovalStatus,
            IsPublished: isPublished,
            PageNumber: pageNumber,
            PageSize: pageSize
        );

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Thông báo của phụ huynh (tự động lọc theo parent và các học sinh liên kết)
    [HttpGet("notifications")]
    public async Task<IResult> GetNotifications(
        [FromQuery] bool? unreadOnly,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetParentNotificationsQuery
        {
            UnreadOnly = unreadOnly,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

