using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.Admin.UpdateUser;

public class UpdateUserResponse
{
    public Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Name { get; init; }
    public string Email { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string Role { get; init; } = null!;
    public string? TeacherCompensationType { get; init; }
    public Guid? BranchId { get; init; }
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public UpdateUserResponse(User user)
    {
        Id = user.Id;
        Username = user.Username;
        Name = user.Name;
        Email = user.Email;
        PhoneNumber = user.PhoneNumber;
        Role = user.Role.ToString();
        TeacherCompensationType = user.TeacherCompensationType?.ToString();
        BranchId = user.BranchId;
        IsActive = user.IsActive;
        IsDeleted = user.IsDeleted;
        CreatedAt = user.CreatedAt;
        UpdatedAt = user.UpdatedAt;
    }
}
