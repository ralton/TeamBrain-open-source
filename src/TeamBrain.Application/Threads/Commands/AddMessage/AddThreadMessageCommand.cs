using MediatR;

namespace TeamBrain.Application.Threads.Commands.AddMessage;

public record AddThreadMessageCommand(Guid ThreadId, string Content) : IRequest<Guid>;
