using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.Admin.GetAllUser;

public class GetUsersQuery : IPageableQuery, IQuery<Page<GetUsersResponse>>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public bool? IsActive { get; init; }
    public UserRole? Role { get; init; }
    public Guid? BranchId { get; init; }
}