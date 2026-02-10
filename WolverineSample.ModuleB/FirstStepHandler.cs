using Marten;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;
using Wolverine.Marten;

namespace WolverineSample.ModuleB;

[MartenStore(typeof(IModuleBStore))]
public sealed class FirstStepHandler
{
    private readonly ILogger<FirstStepHandler> _logger;
    private readonly IDocumentSession _session;
    
    public FirstStepHandler(ILogger<FirstStepHandler> logger, IDocumentSession session)
    {
        _logger = logger;
        _session = session;
    }
    
    [Transactional]
    public async Task Handle(FirstStep message)
    {
        _logger.LogInformation("Handling first step message... {@Message}", message);
        _session.Events.Append(message.ThingId, message);
        await _session.SaveChangesAsync();
    }
}