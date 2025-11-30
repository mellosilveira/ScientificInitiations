using MelloSilveiraTools;
using MelloSilveiraTools.MechanicsOfMaterials.Models.Profiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudRunner.Suspension.Core.Mapper;
using MudRunner.Suspension.Core.Operations;
using MudRunner.Suspension.Core.Operations.RunAnalysis;
using MudRunner.Suspension.Core.Operations.RunAnalysis.Dynamic.HalfCar.SixDegreeOfFreedom;
using Newtonsoft.Json.Converters;

namespace MudRunner.Suspension.Application;

/// <summary>
/// The application startup.
/// It configures the dependency injection and adds all necessary configuration.
/// </summary>
public class Startup
{
    /// <summary>
    /// Gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        // Register Mechanical of Materials sevices.
        services.AddMechanicalOfMaterialsServices();

        // Register Mapper
        services.AddScoped<IMappingResolver, MappingResolver>();

        // Register operations.
        services.AddScoped<CalculateReactions>();
        services.AddScoped<CalculateSteeringKnuckleReactions>();
        services.AddScoped<RunStaticAnalysis<CircularProfile>>();
        services.AddScoped<RunStaticAnalysis<RectangularProfile>>();
        services.AddScoped<RunFatigueAnalysis<CircularProfile>>();
        services.AddScoped<RunFatigueAnalysis<RectangularProfile>>();
        services.AddScoped<RunHalfCarSixDofDynamicAnalysis>();
        services.AddScoped<RunHalfCarSixDofAmplitudeDynamicAnalysis>();

        services
            .AddControllers()
            .AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new StringEnumConverter()));
    }

    /// <summary>
    /// Gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwaggerDocs();
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
