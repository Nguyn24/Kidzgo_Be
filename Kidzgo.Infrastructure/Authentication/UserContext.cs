using Kidzgo.Application.Abstraction.Authentication;
using Microsoft.AspNetCore.Http;

namespace Kidzgo.Infrastructure.Authentication;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public Guid UserId =>
        _httpContextAccessor
            .HttpContext?
            .User
            .GetUserId() ??
        throw new ApplicationException("Users context is unavailable");

    public Guid? StudentId =>
        _httpContextAccessor
            .HttpContext?
            .User
            .GetStudentId();
}
