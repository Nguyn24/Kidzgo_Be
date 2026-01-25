namespace Kidzgo.Application.Abstraction.Authentication;

public interface IUserContext
{
    Guid UserId { get; }
    Guid? StudentId { get; }
}