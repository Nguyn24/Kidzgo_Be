using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Users.Errors;

public static class PinErrors
{
    public static readonly Error Invalid = Error.Validation(
        "Pin.Invalid",
        "PIN must be numeric and less than 10 digits.");

    public static readonly Error InvalidResetToken = Error.Validation(
        "Pin.InvalidResetToken",
        "PIN reset token is invalid or has expired.");

    public static readonly Error InvalidOtp = Error.Validation(
        "Pin.InvalidOtp",
        "OTP is invalid or has expired.");

    public static readonly Error OtpNotVerified = Error.Validation(
        "Pin.OtpNotVerified",
        "OTP has not been verified for this PIN reset request.");
    
    public static readonly Error Wrong = Error.Conflict(
        "Pin.Wrong",
        "PIN is incorrect.");
    
    public static readonly Error NotSet = Error.Conflict(
        "Pin.NotSet",
        "PIN has not been set yet.");
}

