using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Kidzgo.Application.Abstraction.Homework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Infrastructure.AI;

public sealed class HttpAiHomeworkAssistant : IAiHomeworkAssistant
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger _logger;
    private readonly Uri? _publicBaseUri;

    public HttpAiHomeworkAssistant(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<HttpAiHomeworkAssistant> logger)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["AiService:BaseUrl"]
            ?? throw new InvalidOperationException("AiService:BaseUrl not configured");
        _logger = logger;

        var storageBaseUrl = configuration["FileStorage:Local:BaseUrl"];
        if (Uri.TryCreate(storageBaseUrl, UriKind.Absolute, out var storageBaseUri))
        {
            _publicBaseUri = new Uri(storageBaseUri, "/");
        }
    }

    public async Task<AiHomeworkGradeResult> GradeSubmissionAsync(
        AiHomeworkGradeSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var submissionType = request.Context.SubmissionType?.Trim().ToLowerInvariant();
        return submissionType switch
        {
            "text" => await GradeTextAsync(request, cancellationToken),
            "link" => await GradeLinkAsync(request, cancellationToken),
            "image" => await GradeImageFromAttachmentAsync(request, cancellationToken),
            "file" => await GradeFileAsync(request, cancellationToken),
            "quiz" => await GradeQuizAsync(request, cancellationToken),
            _ => await GradeBestEffortAsync(request, cancellationToken)
        };
    }

    public async Task<AiHomeworkSpeakingResult> AnalyzeSpeakingSubmissionAsync(
        AiHomeworkSpeakingSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.Transcript))
        {
            var payload = new
            {
                context = new
                {
                    homework_id = request.Context.HomeworkId,
                    student_id = request.Context.StudentId,
                    mode = string.IsNullOrWhiteSpace(request.Context.SpeakingMode) ? "speaking" : request.Context.SpeakingMode,
                    target_words = request.Context.TargetWords,
                    expected_text = request.ExpectedText,
                    instructions = request.Context.Instructions,
                    language = string.IsNullOrWhiteSpace(request.Language) ? "vi" : request.Language,
                },
                transcript = request.Transcript,
            };

            return await PostAsync<object, AiHomeworkSpeakingResult>(
                $"{_baseUrl}/a8/analyze-transcript",
                payload,
                "A8 analyze-transcript",
                cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(request.AttachmentUrl))
        {
            return BuildUnavailableSpeakingResult(
                "Speaking submission has no transcript or attachment for AI analysis.",
                "Khong tim thay audio/video hoac transcript de AI phan tich.");
        }

        var attachment = await TryDownloadAsync(request.AttachmentUrl, cancellationToken);
        if (attachment is null)
        {
            return BuildUnavailableSpeakingResult(
                "Unable to download the speaking attachment for AI analysis.",
                "Khong tai duoc audio/video bai nop de AI phan tich.");
        }

        if (!IsAudioOrVideo(attachment.ContentType, attachment.FileName))
        {
            return BuildUnavailableSpeakingResult(
                "Speaking analysis currently supports audio or video attachments only.",
                "File hien tai khong phai audio/video de AI phan tich.");
        }

        return await AnalyzeSpeakingMediaInternalAsync(
            request.Context,
            request.ExpectedText,
            request.Language,
            attachment.Bytes,
            attachment.FileName,
            attachment.ContentType,
            cancellationToken);
    }

    public async Task<AiHomeworkSpeakingResult> AnalyzeSpeakingMediaAsync(
        AiHomeworkSpeakingMediaRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.FileBytes.Length == 0)
        {
            return BuildUnavailableSpeakingResult(
                "Speaking analysis requires an audio or video file.",
                "Khong tim thay audio/video de AI phan tich.");
        }

        if (!IsAudioOrVideo(request.ContentType, request.FileName))
        {
            return BuildUnavailableSpeakingResult(
                "Speaking analysis currently supports audio or video attachments only.",
                "File hien tai khong phai audio/video de AI phan tich.");
        }

        return await AnalyzeSpeakingMediaInternalAsync(
            request.Context,
            request.ExpectedText,
            request.Language,
            request.FileBytes,
            request.FileName,
            request.ContentType,
            cancellationToken);
    }

    public async Task<AiQuestionBankGenerationResult> GenerateQuestionBankItemsAsync(
        AiQuestionBankGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await PostAsync<AiQuestionBankGenerationRequest, AiQuestionBankGenerationResult>(
            $"{_baseUrl}/a3/generate-question-bank-items",
            request,
            "A3 generate-question-bank-items",
            cancellationToken);
    }

    public async Task<AiHomeworkHintResult> GetHintAsync(
        AiHomeworkHintRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await PostAsync<AiHomeworkHintRequest, AiHomeworkHintResult>(
            $"{_baseUrl}/a3/generate-hint",
            request,
            "A3 generate-hint",
            cancellationToken);
    }

    public async Task<AiHomeworkRecommendationResult> GetRecommendationsAsync(
        AiHomeworkRecommendationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await PostAsync<AiHomeworkRecommendationRequest, AiHomeworkRecommendationResult>(
            $"{_baseUrl}/a3/recommend-practice",
            request,
            "A3 recommend-practice",
            cancellationToken);
    }

    private async Task<AiHomeworkGradeResult> GradeQuizAsync(
        AiHomeworkGradeSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.StudentAnswerText))
        {
            return await GradeTextAsync(request, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.AttachmentUrl))
        {
            return await GradeFileAsync(request, cancellationToken);
        }

        return BuildUnavailableResult(
            "Quiz submission has no answer content for AI grading.",
            "Bai quiz chua co du lieu de AI cham nhanh.");
    }

    private async Task<AiHomeworkGradeResult> GradeBestEffortAsync(
        AiHomeworkGradeSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.StudentAnswerText))
        {
            return await GradeTextAsync(request, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.AttachmentUrl))
        {
            return await GradeFileAsync(request, cancellationToken);
        }

        return BuildUnavailableResult(
            "Submission has no text or attachment for AI grading.",
            "Khong tim thay bai lam de AI cham nhanh.");
    }

    private async Task<AiHomeworkGradeResult> GradeTextAsync(
        AiHomeworkGradeSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.StudentAnswerText))
        {
            return BuildUnavailableResult(
                "Text submission has no answer content for AI grading.",
                "Bai text chua co noi dung de AI cham.");
        }

        var payload = new
        {
            context = request.Context,
            student_answer_text = request.StudentAnswerText,
            expected_answer_text = request.ExpectedAnswerText,
            language = request.Language
        };

        return await PostAsync<object, AiHomeworkGradeResult>(
            $"{_baseUrl}/a3/grade-text",
            payload,
            "A3 grade-text",
            cancellationToken);
    }

    private async Task<AiHomeworkGradeResult> GradeLinkAsync(
        AiHomeworkGradeSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AttachmentUrl))
        {
            return BuildUnavailableResult(
                "Link submission has no URL for AI grading.",
                "Bai link chua co URL de AI cham.");
        }

        var resolvedUrl = ResolvePublicUrl(request.AttachmentUrl) ?? request.AttachmentUrl;
        var extractedText = await TryReadTextFromUrlAsync(resolvedUrl, cancellationToken);

        var payload = new
        {
            context = request.Context,
            link_url = resolvedUrl,
            extracted_text = extractedText,
            expected_answer_text = request.ExpectedAnswerText,
            language = request.Language
        };

        return await PostAsync<object, AiHomeworkGradeResult>(
            $"{_baseUrl}/a3/grade-link",
            payload,
            "A3 grade-link",
            cancellationToken);
    }

    private async Task<AiHomeworkGradeResult> GradeImageFromAttachmentAsync(
        AiHomeworkGradeSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AttachmentUrl))
        {
            return BuildUnavailableResult(
                "Image submission has no attachment URL for AI grading.",
                "Bai image chua co file de AI cham.");
        }

        var attachment = await TryDownloadAsync(request.AttachmentUrl, cancellationToken);
        if (attachment is null)
        {
            return BuildUnavailableResult(
                "Unable to download the image attachment for AI grading.",
                "Khong tai duoc anh bai nop de AI cham.");
        }

        return await GradeImageAsync(request, attachment, cancellationToken);
    }

    private async Task<AiHomeworkGradeResult> GradeFileAsync(
        AiHomeworkGradeSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AttachmentUrl))
        {
            return BuildUnavailableResult(
                "File submission has no attachment URL for AI grading.",
                "Bai file chua co tep de AI cham.");
        }

        var attachment = await TryDownloadAsync(request.AttachmentUrl, cancellationToken);
        if (attachment is null)
        {
            return BuildUnavailableResult(
                "Unable to download the submission file for AI grading.",
                "Khong tai duoc file bai nop de AI cham.");
        }

        if (IsImage(attachment.ContentType, attachment.FileName))
        {
            return await GradeImageAsync(request, attachment, cancellationToken);
        }

        var extractedText = TryExtractText(attachment);
        if (!string.IsNullOrWhiteSpace(extractedText))
        {
            return await GradeTextAsync(
                new AiHomeworkGradeSubmissionRequest
                {
                    Context = request.Context,
                    StudentAnswerText = extractedText,
                    AttachmentUrl = request.AttachmentUrl,
                    ExpectedAnswerText = request.ExpectedAnswerText,
                    Language = request.Language
                },
                cancellationToken);
        }

        return BuildUnavailableResult(
            "AI grading currently supports image or text-like file submissions only.",
            "File hien tai khong phai anh hoac text de AI cham nhanh.");
    }

    private async Task<AiHomeworkGradeResult> GradeImageAsync(
        AiHomeworkGradeSubmissionRequest request,
        DownloadedResource attachment,
        CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.Context.HomeworkId), "homework_id");
        content.Add(new StringContent(request.Context.StudentId), "student_id");
        content.Add(new StringContent(string.IsNullOrWhiteSpace(request.Context.Skill) ? "writing" : request.Context.Skill), "skill");
        content.Add(new StringContent(string.IsNullOrWhiteSpace(request.Language) ? "vi" : request.Language), "language");

        if (!string.IsNullOrWhiteSpace(request.Context.Instructions))
        {
            content.Add(new StringContent(request.Context.Instructions), "instructions");
        }

        if (!string.IsNullOrWhiteSpace(request.ExpectedAnswerText))
        {
            content.Add(new StringContent(request.ExpectedAnswerText), "expected_answer_text");
        }

        var fileContent = new ByteArrayContent(attachment.Bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(attachment.ContentType);
        content.Add(fileContent, "file", attachment.FileName);

        return await PostMultipartAsync<AiHomeworkGradeResult>(
            $"{_baseUrl}/a3/grade-image",
            content,
            "A3 grade-image",
            cancellationToken);
    }

    private async Task<AiHomeworkSpeakingResult> AnalyzeSpeakingMediaInternalAsync(
        AiHomeworkContext context,
        string? expectedText,
        string? language,
        byte[] fileBytes,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(context.HomeworkId), "homework_id");
        content.Add(new StringContent(context.StudentId), "student_id");
        content.Add(new StringContent(string.IsNullOrWhiteSpace(context.SpeakingMode) ? "speaking" : context.SpeakingMode), "mode");
        content.Add(new StringContent(string.IsNullOrWhiteSpace(language) ? "vi" : language), "language");
        content.Add(new StringContent(string.Join(", ", context.TargetWords ?? [])), "target_words");

        if (!string.IsNullOrWhiteSpace(expectedText))
        {
            content.Add(new StringContent(expectedText), "expected_text");
        }

        if (!string.IsNullOrWhiteSpace(context.Instructions))
        {
            content.Add(new StringContent(context.Instructions), "instructions");
        }

        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.TryParse(contentType, out var mediaTypeHeader)
            ? mediaTypeHeader
            : MediaTypeHeaderValue.Parse("application/octet-stream");
        content.Add(fileContent, "file", fileName);

        return await PostMultipartAsync<AiHomeworkSpeakingResult>(
            $"{_baseUrl}/a8/analyze-media",
            content,
            "A8 analyze-media",
            cancellationToken);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string url,
        TRequest request,
        string operation,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("{Operation} returned {StatusCode}: {Error}", operation, response.StatusCode, errorContent);
                throw new InvalidOperationException(
                    $"{operation} returned {response.StatusCode}: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                },
                cancellationToken: cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException($"{operation} returned null response");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed calling {Operation} at {BaseUrl}", operation, _baseUrl);
            throw new InvalidOperationException(
                $"Failed to call {operation}: {ex.Message}. Make sure AI-KidzGo service is running at {_baseUrl}",
                ex);
        }
    }

    private async Task<TResponse> PostMultipartAsync<TResponse>(
        string url,
        MultipartFormDataContent content,
        string operation,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("{Operation} returned {StatusCode}: {Error}", operation, response.StatusCode, errorContent);
                throw new InvalidOperationException(
                    $"{operation} returned {response.StatusCode}: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                },
                cancellationToken: cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException($"{operation} returned null response");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed calling {Operation} at {BaseUrl}", operation, _baseUrl);
            throw new InvalidOperationException(
                $"Failed to call {operation}: {ex.Message}. Make sure AI-KidzGo service is running at {_baseUrl}",
                ex);
        }
    }

    private async Task<DownloadedResource?> TryDownloadAsync(
        string rawUrl,
        CancellationToken cancellationToken)
    {
        var resolvedUrl = ResolvePublicUrl(rawUrl);
        if (resolvedUrl is null)
        {
            return null;
        }

        try
        {
            using var response = await _httpClient.GetAsync(
                resolvedUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Unable to download homework attachment from {Url}. Status {StatusCode}", resolvedUrl, response.StatusCode);
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            if (bytes.Length == 0)
            {
                return null;
            }

            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = InferContentTypeFromName(rawUrl);
            }

            return new DownloadedResource(
                bytes,
                string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                ResolveFileName(resolvedUrl));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Failed to download homework attachment from {Url}", resolvedUrl);
            return null;
        }
    }

    private async Task<string?> TryReadTextFromUrlAsync(
        string rawUrl,
        CancellationToken cancellationToken)
    {
        var resolvedUrl = ResolvePublicUrl(rawUrl);
        if (resolvedUrl is null)
        {
            return null;
        }

        try
        {
            using var response = await _httpClient.GetAsync(
                resolvedUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var contentType = response.Content.Headers.ContentType?.MediaType;
            var fileName = ResolveFileName(resolvedUrl);

            if (!IsTextLike(contentType, fileName))
            {
                return null;
            }

            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            return NormalizeExtractedText(text, contentType);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Failed to extract text from link {Url}", resolvedUrl);
            return null;
        }
    }

    private string? TryExtractText(DownloadedResource attachment)
    {
        if (!IsTextLike(attachment.ContentType, attachment.FileName))
        {
            return null;
        }

        var encoding = TryGetEncoding(attachment.ContentType) ?? Encoding.UTF8;
        var text = encoding.GetString(attachment.Bytes);
        return NormalizeExtractedText(text, attachment.ContentType);
    }

    private string? NormalizeExtractedText(string? text, string? contentType)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var normalized = text;
        if (!string.IsNullOrWhiteSpace(contentType) &&
            contentType.Contains("html", StringComparison.OrdinalIgnoreCase))
        {
            normalized = Regex.Replace(normalized, "<[^>]+>", " ");
            normalized = WebUtility.HtmlDecode(normalized);
        }

        normalized = Regex.Replace(normalized, "\\s+", " ").Trim();

        if (normalized.Length > 4000)
        {
            normalized = normalized[..4000];
        }

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private string? ResolvePublicUrl(string? rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            return null;
        }

        if (Uri.TryCreate(rawUrl, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        if (_publicBaseUri is null)
        {
            return null;
        }

        return new Uri(_publicBaseUri, rawUrl.TrimStart('/')).ToString();
    }

    private static bool IsImage(string? contentType, string fileName)
        => (!string.IsNullOrWhiteSpace(contentType) &&
            contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
           || IsImageExtension(fileName);

    private static bool IsImageExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg";
    }

    private static bool IsTextLike(string? contentType, string fileName)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            if (contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("xml", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("html", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".txt" or ".md" or ".csv" or ".json" or ".xml" or ".html" or ".htm";
    }

    private static bool IsAudioOrVideo(string? contentType, string fileName)
    {
        if (!string.IsNullOrWhiteSpace(contentType) &&
            (contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ||
             contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".mp3" or ".wav" or ".m4a" or ".aac" or ".ogg" or ".webm" or ".mp4" or ".mov" or ".avi" or ".mkv";
    }

    private static Encoding? TryGetEncoding(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return null;
        }

        var marker = "charset=";
        var index = contentType.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var charset = contentType[(index + marker.Length)..].Trim();
        if (charset.Contains(';'))
        {
            charset = charset[..charset.IndexOf(';')];
        }

        try
        {
            return Encoding.GetEncoding(charset.Trim('"'));
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static string InferContentTypeFromName(string rawUrl)
    {
        return Path.GetExtension(rawUrl).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".m4a" => "audio/mp4",
            ".aac" => "audio/aac",
            ".ogg" => "audio/ogg",
            ".webm" => "video/webm",
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".mkv" => "video/x-matroska",
            ".txt" => "text/plain",
            ".md" => "text/markdown",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".html" or ".htm" => "text/html",
            _ => "application/octet-stream"
        };
    }

    private static string ResolveFileName(string rawUrl)
    {
        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
        {
            return "submission";
        }

        var fileName = Path.GetFileName(uri.LocalPath);
        return string.IsNullOrWhiteSpace(fileName) ? "submission" : fileName;
    }

    private static AiHomeworkGradeResult BuildUnavailableResult(
        string summary,
        params string[] warnings)
    {
        return new AiHomeworkGradeResult
        {
            AiUsed = false,
            Result = new AiHomeworkGradePayload
            {
                Score = 0,
                MaxScore = 10,
                Summary = summary,
                Suggestions = new List<string>
                {
                    "Giao vien co the cham tay hoac thu lai voi bai nop ro rang hon."
                },
                Warnings = warnings
                    .Where(static warning => !string.IsNullOrWhiteSpace(warning))
                    .ToList()
            }
        };
    }

    private static AiHomeworkSpeakingResult BuildUnavailableSpeakingResult(
        string summary,
        params string[] warnings)
    {
        return new AiHomeworkSpeakingResult
        {
            AiUsed = false,
            Result = new AiHomeworkSpeakingPayload
            {
                Summary = summary,
                Transcript = string.Empty,
                Suggestions = new List<string>
                {
                    "Co the thu lai voi audio/video ro hon hoac gui transcript neu can."
                },
                Warnings = warnings
                    .Where(static warning => !string.IsNullOrWhiteSpace(warning))
                    .ToList()
            }
        };
    }

    private sealed record DownloadedResource(
        byte[] Bytes,
        string ContentType,
        string FileName);
}
