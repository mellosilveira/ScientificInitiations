using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.AuxiliarOperations.DifferentialEquationOfMotion;
using IcVibracoes.Core.AuxiliarOperations.Eigenvalue;
using IcVibracoes.Core.AuxiliarOperations.NaturalFrequency;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.GeometricProperties.Circular;
using IcVibracoes.Core.Calculator.GeometricProperties.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.Newmark;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.NewmarkBeta;
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
using IcVibracoes.Core.Validators.TimeStep;
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
            // Auxiliar Operations
            services.AddScoped<ICalculateDifferentialEquationOfMotion, CalculateDifferentialEquationOfMotion>();
            services.AddScoped<ICalculateEigenvalue, CalculateEigenvalue>();
            services.AddScoped<INaturalFrequency, NaturalFrequency>();
            services.AddScoped<IAuxiliarOperation, AuxiliarOperation>();

            // Calculator - Array Operation
            services.AddScoped<IArrayOperation, ArrayOperation>();

            // Calculator - Geometric Property
            services.AddScoped<ICircularGeometricProperty, CircularGeometricProperty>();
            services.AddScoped<IRectangularGeometricProperty, RectangularGeometricProperty>();

            // Calculator - Main Matrixes - Beam
            services.AddScoped<ICircularBeamMainMatrix, CircularBeamMainMatrix>();
            services.AddScoped<IRectangularBeamMainMatrix, RectangularBeamMainMatrix>();

            // Calculator - Main Matrixes - Beam with Dynamic Vibration Absorber
            services.AddScoped<ICircularBeamWithDvaMainMatrix, CircularBeamWithDvaMainMatrix>();
            services.AddScoped<IRectangularBeamWithDvaMainMatrix, RectangularBeamWithDvaMainMatrix>();

            // Calculator - Main Matrixes - Beam with Piezoelectric
            services.AddScoped<ICircularBeamWithPiezoelectricMainMatrix, CircularBeamWithPiezoelectricMainMatrix>();
            services.AddScoped<IRectangularBeamWithPiezoelectricMainMatrix, RectangularBeamWithPiezoelectricMainMatrix>();

            // Mapper
            services.AddScoped<IMappingResolver, MappingResolver>();

            // Numerical Integration Methods - Finite Element - Newmark
            services.AddScoped<INewmarkMethod, NewmarkMethod>();

            // Numerical Integration Methods - Finite Element - Newmark Beta
            services.AddScoped<INewmarkBetaMethod, NewmarkBetaMethod>();

            // Numerical Integration Methods - Rigid Body - Runge Kutta Forth Order
            services.AddScoped<IRungeKuttaForthOrderMethod_1DF, RungeKuttaForthOrderMethod_1DF>();
            services.AddScoped<IRungeKuttaForthOrderMethod_2DF, RungeKuttaForthOrderMethod_2DF>();

            // Rigid Body Operations
            services.AddScoped<ICalculateVibrationToOneDegreeFreedom, CalculateVibrationToOneDegreeFreedom>();
            services.AddScoped<ICalculateVibrationToTwoDegreesFreedom, CalculateVibrationToTwoDegreesFreedom>();

            // Finite Element Operations - Beam
            services.AddScoped<ICalculateCircularBeamVibration, CalculateCircularBeamVibration>();
            services.AddScoped<ICalculateRectangularBeamVibration, CalculateRectangularBeamVibration>();

            // Finite Element Operations - Beam with Dynamic Vibration Absorber
            services.AddScoped<ICalculateCircularBeamWithDvaVibration, CalculateCircularBeamWithDvaVibration>();
            services.AddScoped<ICalculateRectangularBeamWithDvaVibration, CalculateRectangularBeamWithDvaVibration>();

            // Finite Element Operations - Beam with Piezoelectric
            services.AddScoped<ICalculateCircularBeamWithPiezoelectricVibration, CalculateCircularBeamWithPiezoelectricVibration>();
            services.AddScoped<ICalculateRectangularBeamWithPiezoelectricVibration, CalculateRectangularBeamWithPiezoelectricVibration>();

            // Validators -  Beam Request Data

            // Validators - Profiles
            services.AddScoped<IRectangularProfileValidator, RectangularProfileValidator>();
            services.AddScoped<ICircularProfileValidator, CircularProfileValidator>();

            // Validators - Time Step
            services.AddScoped<ITimeStepValidator, TimeStepValidator>();

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
