using Marten;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Marten;

namespace WolverineSample.ModuleA;

public sealed record StartFirstStep(Guid ThingId);

[MartenStore(typeof(IModuleAStore))]
public sealed class StartFirstStepHandler
{
    private readonly ILogger<FirstStep> _logger;
    private readonly IDocumentSession _session;
    private readonly IMessageBus _bus;

    public StartFirstStepHandler(ILogger<FirstStep> logger, IDocumentSession session, IMessageBus bus)
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
        await _session.SaveChangesAsync();
        
        await _bus.PublishAsync(@event);
    }
}