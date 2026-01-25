using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Abstraction.Authentication;

public interface ITokenProvider
{
    string Create(User user);
    string Create(User user, Guid? studentId);
    string GenerateRefreshToken();
    string GeneratePasswordResetToken();
    string CreateDonationMatchConfirmToken(Guid matchId, TimeSpan? lifetime = null);
    Guid? ValidateDonationMatchConfirmToken(string token);

}
