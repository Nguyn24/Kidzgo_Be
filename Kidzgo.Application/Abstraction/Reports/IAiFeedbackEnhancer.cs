namespace Kidzgo.Application.Abstraction.Reports;

/// <summary>
/// Interface for AI-powered Session Report Feedback enhancement
/// UC-174: AI enhance draft feedback before teacher submits
/// </summary>
public interface IAiFeedbackEnhancer
{
    /// <summary>
    /// Enhance draft feedback to make it more formal/professional
    /// This is a preview API - does NOT save to database
    /// </summary>
    /// <param name="draft">The draft feedback text written by teacher</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enhanced feedback result</returns>
    Task<EnhancedFeedbackResult> EnhanceAsync(
        string draft,
        CancellationToken cancellationToken = default);
}

public class EnhancedFeedbackResult
{
    public string EnhancedFeedback { get; set; } = string.Empty;
    public string OriginalFeedback { get; set; } = string.Empty;
}
