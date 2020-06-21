using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations.BoundaryCondition;
using IcVibracoes.Core.AuxiliarOperations.File;
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
using IcVibracoes.Core.NumericalIntegrationMethods.RungeKuttaForthOrder;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.Beam.Circular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.Beam.Rectangular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithDva.Circular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithDva.Rectangular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithPiezoelectric.Circular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithPiezoelectric.Rectangular;
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
            // Register Array Operation
            services.AddScoped<IArrayOperation, ArrayOperation>();

            // Register Auxiliar Operations - Boundary Condition
            services.AddScoped<IBoundaryCondition, BoundaryCondition>();

            // Register Auxiliar Operations - File
            services.AddScoped<IFile, File>();

            // Register Calculator - Differential Equation of Motion
            services.AddScoped<IDifferentialEquationOfMotion, DifferentialEquationOfMotion>();

            // Register Calculator - Eigenvalue
            services.AddScoped<IEigenvalue, Eigenvalue>();

            // Register Calculator - Force
            services.AddScoped<IForce, Force>();

            // Register Calculator - Geometric Property
            services.AddScoped<ICircularGeometricProperty, CircularGeometricProperty>();
            services.AddScoped<IRectangularGeometricProperty, RectangularGeometricProperty>();

            // Register Calculator - Main Matrixes - Beam
            services.AddScoped<ICircularBeamMainMatrix, CircularBeamMainMatrix>();
            services.AddScoped<IRectangularBeamMainMatrix, RectangularBeamMainMatrix>();

            // Register Calculator - Main Matrixes - Beam with Dynamic Vibration Absorber
            services.AddScoped<ICircularBeamWithDvaMainMatrix, CircularBeamWithDvaMainMatrix>();
            services.AddScoped<IRectangularBeamWithDvaMainMatrix, RectangularBeamWithDvaMainMatrix>();

            // Register Calculator - Main Matrixes - Beam with Piezoelectric
            services.AddScoped<ICircularBeamWithPiezoelectricMainMatrix, CircularBeamWithPiezoelectricMainMatrix>();
            services.AddScoped<IRectangularBeamWithPiezoelectricMainMatrix, RectangularBeamWithPiezoelectricMainMatrix>();

            // Register Calculator - Natural Frequency
            services.AddScoped<INaturalFrequency, NaturalFrequency>();

            // Register Calculator - Time
            services.AddScoped<ITime, Time>();

            // Register Mapper
            services.AddScoped<IMappingResolver, MappingResolver>();

            // Register Numerical Integration Methods
            services.AddScoped<INewmarkMethod, NewmarkMethod>();
            services.AddScoped<INewmarkBetaMethod, NewmarkBetaMethod>();
            services.AddScoped<IRungeKuttaForthOrderMethod, RungeKuttaForthOrderMethod>();

            // Register Rigid Body Operations
            services.AddScoped<ICalculateVibrationToOneDegreeFreedom, CalculateVibrationToOneDegreeFreedom>();
            services.AddScoped<ICalculateVibrationToTwoDegreesFreedom, CalculateVibrationToTwoDegreesFreedom>();

            // Register Finite Element Operations - Beam
            services.AddScoped<ICalculateCircularBeamVibration, CalculateCircularBeamVibration>();
            services.AddScoped<ICalculateRectangularBeamVibration, CalculateRectangularBeamVibration>();

            // Register Finite Element Operations - Beam with Dynamic Vibration Absorber
            services.AddScoped<ICalculateCircularBeamWithDvaVibration, CalculateCircularBeamWithDvaVibration>();
            services.AddScoped<ICalculateRectangularBeamWithDvaVibration, CalculateRectangularBeamWithDvaVibration>();

            // Register Finite Element Operations - Beam with Piezoelectric
            services.AddScoped<ICalculateCircularBeamWithPiezoelectricVibration, CalculateCircularBeamWithPiezoelectricVibration>();
            services.AddScoped<ICalculateRectangularBeamWithPiezoelectricVibration, CalculateRectangularBeamWithPiezoelectricVibration>();

            // Register Validators - Beam Request Data

            // Register Validators - Profiles
            services.AddScoped<IRectangularProfileValidator, RectangularProfileValidator>();
            services.AddScoped<ICircularProfileValidator, CircularProfileValidator>();

            // Register Validators - Time Step
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
