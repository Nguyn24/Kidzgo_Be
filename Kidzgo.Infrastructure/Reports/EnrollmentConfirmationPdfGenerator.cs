using System.Globalization;
using System.Net;
using System.Text;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Application.Abstraction.Storage;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace Kidzgo.Infrastructure.Reports;

public sealed class EnrollmentConfirmationPdfGenerator(
    IFileStorageService fileStorage,
    ILogger<EnrollmentConfirmationPdfGenerator> logger
) : IEnrollmentConfirmationPdfGenerator
{
    private static readonly CultureInfo ViCulture = new("vi-VN");
    private static readonly SemaphoreSlim DownloadLock = new(1, 1);
    private static bool browserReady;

    public async Task<string> GeneratePdfAsync(
        EnrollmentConfirmationPdfDocument document,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var html = BuildHtml(document);
            var pdfBytes = await GeneratePdfBytesAsync(html, cancellationToken);
            var fileName = BuildFileName(document);

            using var stream = new MemoryStream(pdfBytes);
            return await fileStorage.UploadFileAsync(
                stream,
                fileName,
                "enrollment-confirmations",
                "document",
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to generate enrollment confirmation PDF for registration {RegistrationId}, enrollment {EnrollmentId}",
                document.RegistrationId,
                document.EnrollmentId);
            throw new InvalidOperationException($"Failed to generate enrollment confirmation PDF: {ex.Message}", ex);
        }
    }

    private static string BuildHtml(EnrollmentConfirmationPdfDocument document)
    {
        var issuedAt = VietnamTime.ToVietnamDateTime(document.GeneratedAt).ToString("dd/MM/yyyy HH:mm", ViCulture);
        var firstStudyDate = document.FirstStudyDate ?? document.EnrollDate;
        var studyDaySummary = string.IsNullOrWhiteSpace(document.StudyDaySummary)
            ? "Theo lich lop"
            : document.StudyDaySummary;
        var parentInfo = string.IsNullOrWhiteSpace(document.ParentName)
            ? "Chua cap nhat"
            : document.ParentName;

        if (!string.IsNullOrWhiteSpace(document.ParentPhoneNumber))
        {
            parentInfo += $" - {document.ParentPhoneNumber}";
        }

        return $$"""
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Phieu xac nhan nhap hoc</title>
    <style>
        * { box-sizing: border-box; }
        body {
            margin: 0;
            padding: 28px;
            color: #1f2933;
            font-family: Arial, sans-serif;
            font-size: 13px;
            line-height: 1.5;
        }
        .page {
            border: 1px solid #c9d4df;
            padding: 28px;
            min-height: 1040px;
        }
        .header {
            border-bottom: 3px solid #0f766e;
            padding-bottom: 18px;
            margin-bottom: 22px;
        }
        .brand {
            font-size: 20px;
            font-weight: 700;
            color: #0f766e;
            letter-spacing: .4px;
        }
        .branch {
            margin-top: 6px;
            color: #52616f;
        }
        h1 {
            margin: 28px 0 8px;
            text-align: center;
            color: #111827;
            font-size: 24px;
            letter-spacing: .4px;
        }
        .subtitle {
            text-align: center;
            color: #52616f;
            margin-bottom: 26px;
        }
        .section {
            margin: 18px 0;
        }
        .section-title {
            margin: 0 0 10px;
            color: #0f766e;
            font-size: 15px;
            font-weight: 700;
            text-transform: uppercase;
        }
        table {
            width: 100%;
            border-collapse: collapse;
        }
        th, td {
            border: 1px solid #d8e0e8;
            padding: 10px 12px;
            vertical-align: top;
        }
        th {
            width: 32%;
            background: #f3f7f8;
            text-align: left;
            color: #3b4a54;
            font-weight: 700;
        }
        .amount {
            font-size: 18px;
            font-weight: 700;
            color: #0f766e;
        }
        .note {
            margin-top: 18px;
            padding: 12px 14px;
            border-left: 4px solid #0f766e;
            background: #f3f7f8;
            color: #3b4a54;
        }
        .signature {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 36px;
            margin-top: 54px;
            text-align: center;
        }
        .signature strong {
            display: block;
            margin-bottom: 70px;
        }
        .footer {
            margin-top: 36px;
            border-top: 1px solid #d8e0e8;
            padding-top: 12px;
            color: #65727f;
            font-size: 12px;
        }
    </style>
</head>
<body>
    <main class="page">
        <section class="header">
            <div class="brand">{{H(document.BranchName)}}</div>
            <div class="branch">
                {{H(document.BranchAddress ?? "Dia chi dang cap nhat")}}
                {{(!string.IsNullOrWhiteSpace(document.BranchPhoneNumber) ? " | " + H(document.BranchPhoneNumber) : "")}}
            </div>
        </section>

        <h1>PHIEU XAC NHAN NHAP HOC</h1>
        <div class="subtitle">Ma dang ky: {{H(document.RegistrationId.ToString())}}</div>

        <section class="section">
            <div class="section-title">Thong tin hoc vien</div>
            <table>
                <tr><th>Hoc vien</th><td>{{H(document.StudentName)}}</td></tr>
                <tr><th>Phu huynh</th><td>{{H(parentInfo)}}</td></tr>
                <tr><th>Ngay ghi danh vao lop</th><td>{{DisplayDate(document.EnrollDate)}}</td></tr>
                <tr><th>Ngay vao hoc du kien</th><td>{{DisplayDate(firstStudyDate)}}</td></tr>
            </table>
        </section>

        <section class="section">
            <div class="section-title">Thong tin lop hoc</div>
            <table>
                <tr><th>Khoa / chuong trinh</th><td>{{H(document.ProgramName)}} ({{H(document.ProgramCode)}})</td></tr>
                <tr><th>Lop</th><td>{{H(document.ClassCode)}} - {{H(document.ClassTitle)}}</td></tr>
                <tr><th>Ca hoc</th><td>{{H(studyDaySummary)}}</td></tr>
                <tr><th>Track</th><td>{{H(document.Track)}}</td></tr>
                <tr><th>Hinh thuc vao lop</th><td>{{H(document.EntryType)}}</td></tr>
            </table>
        </section>

        <section class="section">
            <div class="section-title">Thong tin hoc phi</div>
            <table>
                <tr><th>Goi hoc</th><td>{{H(document.TuitionPlanName)}}</td></tr>
                <tr><th>So buoi</th><td>{{document.TotalSessions:N0}}</td></tr>
                <tr><th>Don gia / buoi</th><td>{{H(FormatAmount(document.UnitPriceSession, document.Currency))}}</td></tr>
                <tr><th>Hoc phi</th><td><span class="amount">{{H(FormatAmount(document.TuitionAmount, document.Currency))}}</span></td></tr>
            </table>
        </section>

        <div class="note">
            Phieu nay xac nhan hoc vien da duoc ghi danh vao lop tai trung tam.
            Thong tin hoc phi duoc lay theo goi hoc tai thoi diem xuat phieu.
        </div>

        <section class="signature">
            <div>
                <strong>Dai dien trung tam</strong>
                {{H(document.IssuedByName ?? string.Empty)}}
            </div>
            <div>
                <strong>Phu huynh hoc vien</strong>
            </div>
        </section>

        <section class="footer">
            Ngay xuat phieu: {{H(issuedAt)}} | Ma enrollment: {{H(document.EnrollmentId.ToString())}}
        </section>
    </main>
</body>
</html>
""";
    }

    private static async Task<byte[]> GeneratePdfBytesAsync(string htmlContent, CancellationToken cancellationToken)
    {
        await EnsureBrowserReadyAsync(cancellationToken);

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });

        using var page = await browser.NewPageAsync();
        await page.SetContentAsync(htmlContent, new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
        });

        return await page.PdfDataAsync(new PdfOptions
        {
            Format = PuppeteerSharp.Media.PaperFormat.A4,
            MarginOptions = new PuppeteerSharp.Media.MarginOptions
            {
                Top = "20px",
                Right = "20px",
                Bottom = "20px",
                Left = "20px"
            },
            PrintBackground = true
        });
    }

    private static async Task EnsureBrowserReadyAsync(CancellationToken cancellationToken)
    {
        if (browserReady)
        {
            return;
        }

        await DownloadLock.WaitAsync(cancellationToken);
        try
        {
            if (browserReady)
            {
                return;
            }

            try
            {
                using var testBrowser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                });
                await testBrowser.CloseAsync();
                browserReady = true;
                return;
            }
            catch
            {
            }

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            browserReady = true;
        }
        finally
        {
            DownloadLock.Release();
        }
    }

    private static string BuildFileName(EnrollmentConfirmationPdfDocument document)
    {
        var safeStudentName = Slugify(document.StudentName);
        var date = document.EnrollDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        return $"enrollment-confirmation-{safeStudentName}-{date}-{document.EnrollmentId:N}.pdf";
    }

    private static string Slugify(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '-');
        }

        var slug = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "student" : slug;
    }

    private static string DisplayDate(DateOnly value)
        => value.ToString("dd/MM/yyyy", ViCulture);

    private static string FormatAmount(decimal amount, string currency)
        => $"{amount.ToString("N0", ViCulture)} {currency}";

    private static string H(string value)
        => WebUtility.HtmlEncode(value);
}
