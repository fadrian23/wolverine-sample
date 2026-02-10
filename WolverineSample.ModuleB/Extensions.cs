using JasperFx;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wolverine.Marten;

namespace WolverineSample.ModuleB;

public static class Extensions
{
    public static IServiceCollection AddModuleB(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("modulebpostgres")!;
        services.AddNpgsqlDataSource(connectionString, serviceKey: "moduleB");
        
        services.AddMartenStore<IModuleBStore>(sp =>
        {
            var storeOptions = new StoreOptions
            {
                Events =
                {
                    DatabaseSchemaName = "moduleb",
                },
                DatabaseSchemaName = "moduleb",
                AutoCreateSchemaObjects = AutoCreate.All
            };

            var dataSource = sp.GetRequiredKeyedService<NpgsqlDataSource>("moduleB");
            storeOptions.Connection(dataSource);
            return storeOptions;
        }).IntegrateWithWolverine(opt =>
        {
            opt.SchemaName = "moduleb";
            opt.AutoCreate = AutoCreate.All;
        });
        return services;
    }
}

