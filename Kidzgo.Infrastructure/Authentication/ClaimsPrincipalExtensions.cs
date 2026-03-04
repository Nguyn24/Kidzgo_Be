using System.Security.Claims;

namespace Kidzgo.Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out Guid parsedUserId) ?
            parsedUserId :
            throw new ApplicationException("Users id is unavailable");
    }

    public static Guid? GetStudentId(this ClaimsPrincipal? principal)
    {
        string? studentId = principal?.FindFirstValue("StudentId");

        return Guid.TryParse(studentId, out Guid parsedStudentId) ?
            parsedStudentId :
            null;
    }
    
    public static Guid? GetParentId(this ClaimsPrincipal? principal)
    {
        string? parentId = principal?.FindFirstValue("ParentId");

        return Guid.TryParse(parentId, out Guid parsedParentId) ?
            parsedParentId :
            null;
    }
}
