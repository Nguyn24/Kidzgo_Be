using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.ExportDeliveredRewardRedemptions;

public sealed class ExportDeliveredRewardRedemptionsQueryHandler(IDbContext context)
    : IQueryHandler<ExportDeliveredRewardRedemptionsQuery, ExportDeliveredRewardRedemptionsResponse>
{
    private const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public async Task<Result<ExportDeliveredRewardRedemptionsResponse>> Handle(
        ExportDeliveredRewardRedemptionsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Year.HasValue != query.Month.HasValue)
        {
            return Result.Failure<ExportDeliveredRewardRedemptionsResponse>(
                Error.Validation("Gamification.ExportMonthInvalid", "Year and month must be specified together"));
        }

        var vietnamNow = VietnamTime.NowInVietnam();
        var year = query.Year ?? vietnamNow.Year;
        var month = query.Month ?? vietnamNow.Month;

        if (year < 2000 || year > 2100)
        {
            return Result.Failure<ExportDeliveredRewardRedemptionsResponse>(
                Error.Validation("Year", "Year must be between 2000 and 2100"));
        }

        if (month < 1 || month > 12)
        {
            return Result.Failure<ExportDeliveredRewardRedemptionsResponse>(
                Error.Validation("Month", "Month must be between 1 and 12"));
        }

        var startUtc = VietnamTime.TreatAsVietnamLocal(new DateTime(year, month, 1));
        var endUtc = VietnamTime.TreatAsVietnamLocal(new DateTime(year, month, 1).AddMonths(1));

        var redemptionsQuery = context.RewardRedemptions
            .AsNoTracking()
            .Where(redemption =>
                redemption.DeliveredAt.HasValue &&
                redemption.DeliveredAt.Value >= startUtc &&
                redemption.DeliveredAt.Value < endUtc &&
                (redemption.Status == RedemptionStatus.Delivered ||
                 redemption.Status == RedemptionStatus.Received));

        if (query.ItemId.HasValue)
        {
            redemptionsQuery = redemptionsQuery.Where(redemption => redemption.ItemId == query.ItemId.Value);
        }

        if (query.BranchId.HasValue)
        {
            redemptionsQuery = redemptionsQuery.Where(redemption =>
                redemption.StudentProfile.ClassEnrollments.Any(enrollment =>
                    enrollment.Status == EnrollmentStatus.Active &&
                    enrollment.Class.BranchId == query.BranchId.Value));
        }

        var rows = await redemptionsQuery
            .OrderBy(redemption => redemption.DeliveredAt)
            .ThenBy(redemption => redemption.ItemName)
            .ThenBy(redemption => redemption.StudentProfile.DisplayName)
            .Select(redemption => new DeliveredRewardRedemptionExportRow
            {
                RedemptionId = redemption.Id,
                StudentProfileId = redemption.StudentProfileId,
                StudentName = redemption.StudentProfile.DisplayName ??
                              redemption.StudentProfile.Name ??
                              redemption.StudentProfile.User.Name ??
                              redemption.StudentProfile.User.Email,
                StudentEmail = redemption.StudentProfile.User.Email,
                StudentPhone = redemption.StudentProfile.User.PhoneNumber,
                BranchName = redemption.StudentProfile.ClassEnrollments
                    .Where(enrollment => enrollment.Status == EnrollmentStatus.Active)
                    .OrderByDescending(enrollment => enrollment.EnrollDate)
                    .Select(enrollment => enrollment.Class.Branch.Name)
                    .FirstOrDefault(),
                ClassName = redemption.StudentProfile.ClassEnrollments
                    .Where(enrollment => enrollment.Status == EnrollmentStatus.Active)
                    .OrderByDescending(enrollment => enrollment.EnrollDate)
                    .Select(enrollment => enrollment.Class.Title)
                    .FirstOrDefault(),
                ItemId = redemption.ItemId,
                ItemName = redemption.ItemName,
                Quantity = redemption.Quantity,
                StarsDeducted = redemption.StarsDeducted,
                Status = redemption.Status,
                OrderedAt = redemption.CreatedAt,
                ApprovedAt = redemption.HandledAt,
                HandledByName = redemption.HandledByUser != null
                    ? redemption.HandledByUser.Name ?? redemption.HandledByUser.Email
                    : null,
                DeliveredAt = redemption.DeliveredAt,
                ReceivedAt = redemption.ReceivedAt,
                CancelReason = redemption.CancelReason
            })
            .ToListAsync(cancellationToken);

        var content = CreateWorkbook(rows, year, month);
        var fileName = $"delivered-reward-redemptions-{year:D4}-{month:D2}.xlsx";

        return Result.Success(new ExportDeliveredRewardRedemptionsResponse
        {
            FileName = fileName,
            ContentType = ContentType,
            Content = content,
            RowCount = rows.Count
        });
    }

    private static byte[] CreateWorkbook(
        IReadOnlyCollection<DeliveredRewardRedemptionExportRow> rows,
        int year,
        int month)
    {
        using var stream = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var detailPart = workbookPart.AddNewPart<WorksheetPart>();
            WriteDetailWorksheet(detailPart, rows);

            var summaryPart = workbookPart.AddNewPart<WorksheetPart>();
            WriteSummaryWorksheet(summaryPart, rows, year, month);

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(detailPart),
                SheetId = 1,
                Name = "Details"
            });
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(summaryPart),
                SheetId = 2,
                Name = "Summary"
            });

            workbookPart.Workbook.Save();
        }

        return stream.ToArray();
    }

    private static void WriteDetailWorksheet(
        WorksheetPart worksheetPart,
        IReadOnlyCollection<DeliveredRewardRedemptionExportRow> rows)
    {
        using var writer = OpenXmlWriter.Create(worksheetPart);
        writer.WriteStartElement(new Worksheet());
        WriteDefaultColumns(writer, 18);
        writer.WriteStartElement(new SheetData());

        WriteRow(writer, 1, [
            "No",
            "RedemptionId",
            "StudentProfileId",
            "StudentName",
            "StudentEmail",
            "StudentPhone",
            "Branch",
            "Class",
            "ItemId",
            "ItemName",
            "Quantity",
            "StarsDeducted",
            "Status",
            "OrderedAt",
            "ApprovedAt",
            "HandledBy",
            "DeliveredAt",
            "ReceivedAt"
        ]);

        var rowIndex = 2U;
        var no = 1;
        foreach (var row in rows)
        {
            WriteRow(writer, rowIndex, [
                no.ToString(CultureInfo.InvariantCulture),
                row.RedemptionId.ToString(),
                row.StudentProfileId.ToString(),
                row.StudentName,
                row.StudentEmail,
                row.StudentPhone,
                row.BranchName,
                row.ClassName,
                row.ItemId.ToString(),
                row.ItemName,
                row.Quantity.ToString(CultureInfo.InvariantCulture),
                row.StarsDeducted?.ToString(CultureInfo.InvariantCulture),
                row.Status.ToString(),
                FormatDateTime(row.OrderedAt),
                FormatDateTime(row.ApprovedAt),
                row.HandledByName,
                FormatDateTime(row.DeliveredAt),
                FormatDateTime(row.ReceivedAt)
            ]);
            rowIndex++;
            no++;
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteSummaryWorksheet(
        WorksheetPart worksheetPart,
        IReadOnlyCollection<DeliveredRewardRedemptionExportRow> rows,
        int year,
        int month)
    {
        using var writer = OpenXmlWriter.Create(worksheetPart);
        writer.WriteStartElement(new Worksheet());
        WriteDefaultColumns(writer, 5);
        writer.WriteStartElement(new SheetData());

        WriteRow(writer, 1, ["ReportMonth", $"{year:D4}-{month:D2}"]);
        WriteRow(writer, 2, ["DeliveredRedemptions", rows.Count.ToString(CultureInfo.InvariantCulture)]);
        WriteRow(writer, 3, ["DeliveredQuantity", rows.Sum(row => row.Quantity).ToString(CultureInfo.InvariantCulture)]);
        WriteRow(writer, 4, ["StarsDeducted", rows.Sum(row => row.StarsDeducted ?? 0).ToString(CultureInfo.InvariantCulture)]);
        WriteRow(writer, 6, ["ItemName", "RedemptionCount", "DeliveredQuantity", "StarsDeducted"]);

        var rowIndex = 7U;
        foreach (var itemGroup in rows
                     .GroupBy(row => row.ItemName)
                     .OrderBy(group => group.Key))
        {
            WriteRow(writer, rowIndex, [
                itemGroup.Key,
                itemGroup.Count().ToString(CultureInfo.InvariantCulture),
                itemGroup.Sum(row => row.Quantity).ToString(CultureInfo.InvariantCulture),
                itemGroup.Sum(row => row.StarsDeducted ?? 0).ToString(CultureInfo.InvariantCulture)
            ]);
            rowIndex++;
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteDefaultColumns(OpenXmlWriter writer, uint maxColumn)
    {
        writer.WriteStartElement(new Columns());
        writer.WriteElement(new Column
        {
            Min = 1,
            Max = maxColumn,
            Width = 22,
            CustomWidth = true
        });
        writer.WriteEndElement();
    }

    private static void WriteRow(OpenXmlWriter writer, uint rowIndex, IReadOnlyList<string?> values)
    {
        writer.WriteStartElement(new Row { RowIndex = rowIndex });

        for (var index = 0; index < values.Count; index++)
        {
            WriteTextCell(
                writer,
                GetCellReference(index + 1, rowIndex),
                values[index] ?? string.Empty);
        }

        writer.WriteEndElement();
    }

    private static void WriteTextCell(OpenXmlWriter writer, string cellReference, string value)
    {
        writer.WriteStartElement(new Cell
        {
            CellReference = cellReference,
            DataType = CellValues.InlineString
        });
        writer.WriteElement(new InlineString(
            new Text(value)
            {
                Space = SpaceProcessingModeValues.Preserve
            }));
        writer.WriteEndElement();
    }

    private static string GetCellReference(int columnIndex, uint rowIndex)
    {
        var dividend = columnIndex;
        var columnName = string.Empty;

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return $"{columnName}{rowIndex}";
    }

    private static string? FormatDateTime(DateTime? value)
        => value.HasValue ? FormatDateTime(value.Value) : null;

    private static string FormatDateTime(DateTime value)
        => VietnamTime.FormatInVietnam(value, "yyyy-MM-dd HH:mm");

    private sealed class DeliveredRewardRedemptionExportRow
    {
        public Guid RedemptionId { get; init; }
        public Guid StudentProfileId { get; init; }
        public string StudentName { get; init; } = string.Empty;
        public string? StudentEmail { get; init; }
        public string? StudentPhone { get; init; }
        public string? BranchName { get; init; }
        public string? ClassName { get; init; }
        public Guid ItemId { get; init; }
        public string ItemName { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public int? StarsDeducted { get; init; }
        public RedemptionStatus Status { get; init; }
        public DateTime OrderedAt { get; init; }
        public DateTime? ApprovedAt { get; init; }
        public string? HandledByName { get; init; }
        public DateTime? DeliveredAt { get; init; }
        public DateTime? ReceivedAt { get; init; }
        public string? CancelReason { get; init; }
    }
}
