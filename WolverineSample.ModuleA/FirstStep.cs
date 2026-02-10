using Wolverine.Attributes;

namespace WolverineSample.ModuleA;

[MessageIdentity("first-step")]
public sealed record FirstStep(Guid ThingId);

