using System.ComponentModel.DataAnnotations;
using Kidzgo.Domain.Users;

namespace Kidzgo.API.Requests;

public sealed class CreateUserRequest
{
    [Required]
    public string Name { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    public string Password { get; set; } = null!;
    
    [Required]
    public string Role { get; set; } = null!;
}

