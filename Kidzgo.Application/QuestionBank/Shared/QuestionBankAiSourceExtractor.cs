using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ExcelDataReader;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework.Errors;
using UglyToad.PdfPig;

namespace Kidzgo.Application.QuestionBank.Shared;

internal static partial class QuestionBankAiSourceExtractor
{
    private const int MaxExtractedTextLength = 50_000;

    public static Result<QuestionBankAiSourceText> Extract(Stream stream, string fileName)
    {
        if (stream.CanSeek && stream.Length == 0)
        {
            return Result.Failure<QuestionBankAiSourceText>(
                HomeworkErrors.InvalidQuestionBankFile("Source file is empty"));
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        try
        {
            var text = extension switch
            {
                ".txt" or ".md" or ".csv" or ".json" or ".xml" => ReadText(stream),
                ".html" or ".htm" => StripHtml(ReadText(stream)),
                ".docx" => ReadDocx(stream),
                ".pdf" => ReadPdf(stream),
                ".xlsx" or ".xls" => ReadExcel(stream),
                _ => null
            };

            if (text is null)
            {
                return Result.Failure<QuestionBankAiSourceText>(
                    HomeworkErrors.UnsupportedQuestionBankFileType(extension));
            }

            var normalized = NormalizeText(text);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return Result.Failure<QuestionBankAiSourceText>(
                    HomeworkErrors.InvalidQuestionBankFile("No readable text found in source file"));
            }

            return new QuestionBankAiSourceText(
                normalized.Length > MaxExtractedTextLength
                    ? normalized[..MaxExtractedTextLength]
                    : normalized,
                fileName);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<QuestionBankAiSourceText>(
                HomeworkErrors.InvalidQuestionBankFile($"Unable to read source file: {ex.Message}"));
        }
    }

    private static string ReadText(Stream stream)
    {
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        return reader.ReadToEnd();
    }

    private static string ReadDocx(Stream stream)
    {
        using var document = WordprocessingDocument.Open(stream, false);
        var body = document.MainDocumentPart?.Document.Body;
        if (body is null)
        {
            return string.Empty;
        }

        var paragraphs = body
            .Descendants<Paragraph>()
            .Select(p => string.Join("", p.Descendants<Text>().Select(t => t.Text)));

        return string.Join(Environment.NewLine, paragraphs);
    }

    private static string ReadPdf(Stream stream)
    {
        using var document = PdfDocument.Open(stream);
        return string.Join(Environment.NewLine, document.GetPages().Select(p => p.Text));
    }

    private static string ReadExcel(Stream stream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var reader = ExcelReaderFactory.CreateReader(stream);
        var builder = new StringBuilder();

        do
        {
            while (reader.Read())
            {
                var rowValues = new List<string>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        rowValues.Add(value.Trim());
                    }
                }

                if (rowValues.Count > 0)
                {
                    builder.AppendLine(string.Join(" | ", rowValues));
                }
            }
        } while (reader.NextResult());

        return builder.ToString();
    }

    private static string StripHtml(string value)
    {
        var withoutTags = HtmlTagRegex().Replace(value, " ");
        return WebUtility.HtmlDecode(withoutTags);
    }

    private static string NormalizeText(string value)
    {
        return WhitespaceRegex().Replace(value, " ").Trim();
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex("\\s+")]
    private static partial Regex WhitespaceRegex();
}

internal sealed record QuestionBankAiSourceText(string Text, string FileName);
