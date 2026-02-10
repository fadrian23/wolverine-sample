using Wolverine.Attributes;

namespace WolverineSample.ModuleB;

[MessageIdentity("first-step")]
public sealed record FirstStep(Guid ThingId);