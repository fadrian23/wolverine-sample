using JasperFx;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wolverine.Marten;

namespace WolverineSample.ModuleA;

public static class Extensions
{
    public static IServiceCollection AddModuleA(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("moduleapostgres")!;
        services.AddNpgsqlDataSource(connectionString, serviceKey: "moduleA");
        
        services.AddMartenStore<IModuleAStore>(sp =>
        {
            var storeOptions = new StoreOptions
            {
                Events =
                {
                    DatabaseSchemaName = "modulea",
                },
                DatabaseSchemaName = "modulea",
                AutoCreateSchemaObjects = AutoCreate.All
            };

            var dataSource = sp.GetRequiredKeyedService<NpgsqlDataSource>("moduleA");
            storeOptions.Connection(dataSource);
            return storeOptions;
        }).IntegrateWithWolverine(opt =>
        {
            opt.SchemaName = "modulea";
            opt.AutoCreate = AutoCreate.All;
        });
        
        return services;
    }
}