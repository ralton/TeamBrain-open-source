using MediatR;

namespace TeamBrain.Application.Threads.Commands.PromoteLearning;

public record PromoteThreadToLearningCommand(Guid ThreadId) : IRequest<Guid>;
