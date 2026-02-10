using JasperFx;
using JasperFx.Resources;
using Marten;
using Wolverine;
using Wolverine.Marten;
using Wolverine.RabbitMQ;
using WolverineSample.ModuleA;
using WolverineSample.ModuleB;
using WolverineSample.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
var moduleAConnectionString = builder.Configuration.GetConnectionString("module-a-postgres");
var moduleBConnectionString = builder.Configuration.GetConnectionString("module-b-postgres");
var mainConnectionString = builder.Configuration.GetConnectionString("main-postgres");

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

builder.Services.AddMartenStore<IModuleAStore>(sp =>
{
    var storeOptions = new StoreOptions
    {
        Events =
        {
            DatabaseSchemaName = "modulea",
        },
        DatabaseSchemaName = "modulea"
    };

    storeOptions.Connection(moduleAConnectionString!);
    return storeOptions;
}).IntegrateWithWolverine(opt =>
{
    opt.SchemaName = "modulea";
    opt.AutoCreate = AutoCreate.All;
});

builder.Services.AddMartenStore<IModuleBStore>(sp =>
{
    var storeOptions = new StoreOptions
    {
        Events =
        {
            DatabaseSchemaName = "moduleb",
        },
        DatabaseSchemaName = "moduleb"
    };

    storeOptions.Connection(moduleBConnectionString!);
    return storeOptions;
}).IntegrateWithWolverine(opt =>
{
    opt.SchemaName = "moduleb";
    opt.AutoCreate = AutoCreate.All;
});

builder.UseWolverine(opts =>
{
    opts.UseRabbitMqUsingNamedConnection("rabbitMq").AutoProvision().AutoPurgeOnStartup();
    
    opts.PublishAllMessages().ToRabbitQueue("first-steps").UseDurableOutbox();
    opts.ListenToRabbitQueue("first-steps").UseDurableInbox();
    
    opts.Policies.DisableConventionalLocalRouting();
    
    opts.Services.AddMarten(mainConnectionString!).IntegrateWithWolverine();
    
    opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;
    opts.Durability.MessageIdentity = MessageIdentity.IdAndDestination;
    
    opts.Services.AddResourceSetupOnStartup();
    opts.AutoBuildMessageStorageOnStartup = AutoCreate.All;
    
    opts.Discovery.IncludeAssembly(typeof(StartFirstStepHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(IModuleBStore).Assembly);
});

var app = builder.Build();

app.UseExceptionHandler();
app.MapOpenApi();

app.MapGet("/start-a-thing", async (IMessageBus bus) =>
    {
        await bus.InvokeAsync(new StartFirstStep(Guid.NewGuid()));
        return TypedResults.Ok();
    })
    .WithName("StartAThing");

app.MapDefaultEndpoints();
app.Run();
