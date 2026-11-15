using MediatR;

namespace TeamBrain.Application.Integrations.Commands.LinkWorkItem;

public record LinkWorkItemCommand(
    Guid TargetId,
    string TargetType, // thread | learning_record
    string Provider, // github | ado | asana
    string WorkItemId,
    string WorkItemUrl
) : IRequest<bool>;
