using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.GenerateEnrollmentConfirmationPdf;

public sealed record GenerateEnrollmentConfirmationPdfCommand(
    Guid RegistrationId,
    string? Track,
    bool Regenerate,
    string? FormType) : ICommand<GenerateEnrollmentConfirmationPdfResponse>;
