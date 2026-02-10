using JasperFx;
using JasperFx.Resources;
using Marten;
using Wolverine;
using Wolverine.Marten;
using Wolverine.RabbitMQ;
using WolverineSample.ModuleA;
using WolverineSample.ModuleB;
using WolverineSample.ServiceDefaults;
using WolverineSample.SharedEvents;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
var mainConnectionString = builder.Configuration.GetConnectionString("mainpostgres");

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

builder.Services.AddModuleA(builder.Configuration);
builder.Services.AddModuleB(builder.Configuration);
builder.Services.AddNpgsqlDataSource(mainConnectionString!, serviceKey: "main");
builder.UseWolverine(opts =>
{
    opts.UseRabbitMq().AutoProvision().AutoPurgeOnStartup();
    
    opts.PublishAllMessages().ToRabbitQueue("first-steps").UseDurableOutbox();
    opts.ListenToRabbitQueue("first-steps").UseDurableInbox();
    opts.Policies.DisableConventionalLocalRouting();
    
    opts.Services.AddMarten().UseNpgsqlDataSource("main").IntegrateWithWolverine();
    
    opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;
    opts.Durability.MessageIdentity = MessageIdentity.IdAndDestination;
    
    opts.Services.AddResourceSetupOnStartup();
    opts.AutoBuildMessageStorageOnStartup = AutoCreate.All;
    
    opts.Discovery.IncludeAssembly(typeof(StartFirstStepHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(IModuleBStore).Assembly);
    
    opts.Policies.UseDurableInboxOnAllListeners();
    opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
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
await app.RunJasperFxCommands(args);


// dodalem moduleA connection jako datasource