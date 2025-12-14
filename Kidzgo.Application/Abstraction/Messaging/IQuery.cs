using Kidzgo.Domain.Common;
using MediatR;

namespace Kidzgo.Application.Abstraction.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;