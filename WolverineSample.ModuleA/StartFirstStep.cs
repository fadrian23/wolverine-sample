using Marten;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Marten;
using WolverineSample.SharedEvents;

namespace WolverineSample.ModuleA;

public sealed record StartFirstStep(Guid ThingId);

[MartenStore(typeof(IModuleAStore))]
public sealed class StartFirstStepHandler
{
    private readonly ILogger<StartFirstStepHandler> _logger;
    private readonly IDocumentSession _session;
    private readonly IMessageContext _bus;

    public StartFirstStepHandler(ILogger<StartFirstStepHandler> logger, IDocumentSession session, IMessageContext bus)
    {
        _logger = logger;
        _session = session;
        _bus = bus;
    }

    [Transactional]
    public async Task Handle(StartFirstStep message)
    {
        _logger.LogInformation("Starting a thing... {ThingId}", message.ThingId);

        var @event = new FirstStep(message.ThingId);
        
        _session.Events.Append(message.ThingId, @event);
        await _bus.PublishAsync(@event);
    }
}