using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.DifferentialEquationOfMotion;
using IcVibracoes.Core.Calculator.Eigenvalue;
using IcVibracoes.Core.Calculator.Force;
using IcVibracoes.Core.Calculator.GeometricProperties.Circular;
using IcVibracoes.Core.Calculator.GeometricProperties.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva.Rectangular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Circular;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.NumericalIntegrationMethods.NewmarkBeta;
using IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder.RigidBody_1DF;
using IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder.RigidBody_2DF;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.Beam.Circular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.Beam.Rectangular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithDva.Circular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithDva.Rectangular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithPiezoelectric.Circular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeOfFreedom;
using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesOfFreedom;
using IcVibracoes.Core.Validators.Profiles.Circular;
using IcVibracoes.Core.Validators.Profiles.Rectangular;
using IcVibracoes.Core.Validators.TimeStep;
using IcVibracoes.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace IcVibracoes
{
    /// <summary>
    /// The application startup.
    /// It configures the dependecy injection and adds all necessary configuration.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// The configuration used in application.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Auxiliar Operations
            services.AddScoped<IAuxiliarOperation, AuxiliarOperation>();
            services.AddScoped<IDifferentialEquationOfMotion, DifferentialEquationOfMotion>();
            services.AddScoped<IEigenvalue, Eigenvalue>();
            services.AddScoped<IForce, Force>();
            services.AddScoped<INaturalFrequency, NaturalFrequency>();

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

            // Calculator - Time
            services.AddScoped<ITime, Time>();

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

            services.AddSwaggerDocs();
        }

        /// <summary>
        /// Configures the application dependecies and web hosting environment.
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
