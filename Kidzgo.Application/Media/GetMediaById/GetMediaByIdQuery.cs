using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Media.GetMediaById;

public sealed record GetMediaByIdQuery(Guid Id) : IQuery<GetMediaByIdResponse>;

