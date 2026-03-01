namespace Kidzgo.API.Requests;

/// <summary>
/// UC-174: AI enhance draft feedback request
/// </summary>
public class EnhanceFeedbackRequest
{
    /// <summary>
    /// Draft feedback text written by teacher (informal)
    /// </summary>
    public string Draft { get; set; } = string.Empty;
}
