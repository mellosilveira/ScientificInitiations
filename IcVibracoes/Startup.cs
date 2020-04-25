using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Mapper.PiezoelectricProfiles.Circular;
using IcVibracoes.Core.Mapper.PiezoelectricProfiles.Rectangular;
using IcVibracoes.Core.Mapper.Profiles.Circular;
using IcVibracoes.Core.Mapper.Profiles.Rectangular;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark.BeamWithDva;
using IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder.OneDegreeFreedom;
using IcVibracoes.Core.NumericalIntegrationMethods.RigidBody.RungeKuttaForthOrder.TwoDegreeFreedom;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.Beam.Circular;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.Beam.Rectangular;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithDva.Circular;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithDva.Rectangular;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithPiezoelectric.Circular;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeFreedom;
using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesFreedom;
using IcVibracoes.Core.Validators.Profiles.Circular;
using IcVibracoes.Core.Validators.Profiles.Rectangular;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace IcVibracoes
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Calculator - ArrayOperation
            services.AddScoped<IArrayOperation, ArrayOperation>();
            // Calculator - CalculateGeometricProperty
            services.AddScoped<ICalculateGeometricProperty, CalculateGeometricProperty>();
            // Calculator - MainMatrix
            services.AddScoped<IRectangularBeamMainMatrix, RectangularBeamMainMatrix>();
            services.AddScoped<ICircularBeamMainMatrix, CircularBeamMainMatrix>();
            services.AddScoped<ICircularBeamWithDvaMainMatrix, CircularBeamWithDvaMainMatrix>();
            services.AddScoped<IRectangularBeamWithDvaMainMatrix, RectangularBeamWithDvaMainMatrix>();
            services.AddScoped<IRectangularBeamWithPiezoelectricMainMatrix, RectangularBeamWithPiezoelectricMainMatrix>();
            services.AddScoped<ICircularBeamWithPiezoelectricMainMatrix, CircularBeamWithPiezoelectricMainMatrix>();

            // Mapper
            services.AddScoped<IMappingResolver, MappingResolver>();
            services.AddScoped<ICircularProfileMapper, CircularProfileMapper>();
            services.AddScoped<IRectangularProfileMapper, RectangularProfileMapper>();
            services.AddScoped<IPiezoelectricCircularProfileMapper, PiezoelectricCircularProfileMapper>();
            services.AddScoped<IPiezoelectricRectangularProfileMapper, PiezoelectricRectangularProfileMapper>();

            // Newmark Numerical Integration
            services.AddScoped<INewmarkMethod, NewmarkMethod>();
            services.AddScoped<IBeamWithDvaNewmarkMethod, BeamWithDvaNewmarkMethod>();

            // Runge Kutta Forth Order Numerical Integration
            services.AddScoped<IRungeKuttaForthOrderMethod_1DF, RungeKuttaForthOrderMethod_1DF>();
            services.AddScoped<IRungeKuttaForthOrderMethod_2DF, RungeKuttaForthOrderMethod_2DF>();

            // Auxiliar Operations
            services.AddScoped<IAuxiliarOperation, AuxiliarOperation>();
            
            // Rigid Body Operations
            services.AddScoped<ICalculateVibrationToOneDegreeFreedom, CalculateVibrationToOneDegreeFreedom>();
            services.AddScoped<ICalculateVibrationToTwoDegreesFreedom, CalculateVibrationToTwoDegreesFreedom>();

            // Beam Operations
            services.AddScoped<ICalculateCircularBeamVibration, CalculateCircularBeamVibration>();
            services.AddScoped<ICalculateRectangularBeamVibration, CalculateRectangularBeamVibration>();

            // BeamWithDva Operations
            services.AddScoped<ICalculateCircularBeamWithDvaVibration, CalculateCircularBeamWithDvaVibration>();
            services.AddScoped<ICalculateRectangularBeamWithDvaVibration, CalculateRectangularBeamWithDvaVibration>();

            // Piezoelectric Operations
            services.AddScoped<ICalculateCircularBeamWithPiezoelectricVibration, CalculateCircularBeamWithPiezoelectricVibration>();
            services.AddScoped<ICalculateRectangularBeamWithPiezoelectricVibration, CalculateRectangularBeamWithPiezoelectricVibration>();

            // Validators
            services.AddScoped<IRectangularProfileValidator, RectangularProfileValidator>();
            services.AddScoped<ICircularProfileValidator, CircularProfileValidator>();

            services.AddControllers();

            ConfigureSwagger(services);
        }

        private static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "IC Vibrations", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "IC Vibration V1");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
