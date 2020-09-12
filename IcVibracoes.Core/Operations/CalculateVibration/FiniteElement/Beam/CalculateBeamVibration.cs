using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.ExtensionMethods;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.FiniteElement.Beam;
using System;
using System.IO;
using IcVibracoes.Core.Calculator.GeometricProperties;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.Beam
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class CalculateBeamVibration<TProfile> : CalculateVibrationFiniteElement<BeamRequest<TProfile>, TProfile, Beam<TProfile>>, ICalculateBeamVibration<TProfile>
        where TProfile : Profile, new()
    {
        private static readonly string TemplateBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Solutions/FiniteElement/Beam");

        private readonly IGeometricProperty<TProfile> _geometricProperty;
        private readonly IMappingResolver _mappingResolver;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="mainMatrix"></param>
        /// <param name="profileValidator"></param>
        /// <param name="time"></param>
        /// <param name="naturalFrequency"></param>
        protected CalculateBeamVibration(
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IBeamMainMatrix<TProfile> mainMatrix,
            IProfileValidator<TProfile> profileValidator,
            ITime time,
            INaturalFrequency naturalFrequency)
            : base(profileValidator, time, naturalFrequency, mainMatrix)
        {
            this._geometricProperty = geometricProperty;
            this._mappingResolver = mappingResolver;
        }

        /// <summary>
        /// This method creates a new instance of class <see cref="Beam{TProfile}"/>.
        /// This is a step to create the input fot finite element analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>A new instance of class <see cref="Beam{TProfile}"/>.</returns>
        public override Beam<TProfile> BuildBeam(BeamRequest<TProfile> request, uint degreesOfFreedom)
        {
            GeometricProperty geometricProperty;

            if (request.Profile.Area != null && request.Profile.MomentOfInertia != null)
            {
                geometricProperty = GeometricProperty.Create(
                    area: ArrayFactory.CreateVector(request.Profile.Area.Value, request.NumberOfElements),
                    momentOfInertia: ArrayFactory.CreateVector(request.Profile.MomentOfInertia.Value, request.NumberOfElements));
            }
            else
            {
                geometricProperty = GeometricProperty.Create(
                    area: this._geometricProperty.CalculateArea(request.Profile, request.NumberOfElements),
                    momentOfInertia: this._geometricProperty.CalculateMomentOfInertia(request.Profile, request.NumberOfElements));
            }

            return new Beam<TProfile>
            {
                Fastenings = this._mappingResolver.BuildFastenings(request.Fastenings),
                Forces = this._mappingResolver.BuildForceVector(request.Forces, degreesOfFreedom),
                GeometricProperty = geometricProperty,
                Length = request.Length,
                Material = Material.Create(request.Material),
                NumberOfElements = request.NumberOfElements,
                Profile = request.Profile
            };
        }

        /// <summary>
        /// This method creates the path to save the solution files.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the solution files.</returns>
        public override string CreateSolutionPath(BeamRequest<TProfile> request, FiniteElementMethodInput input)
        {
            var fileInfo = new FileInfo(Path.Combine(
                TemplateBasePath,
                $"{request.Profile.GetType().Name}/nEl={request.NumberOfElements}/{request.NumericalMethod}",
                $"{request.AnalysisType}_{request.Profile.GetType().Name}_w={Math.Round(input.AngularFrequency, 2)}_nEl={request.NumberOfElements}.csv"));

            if (fileInfo.Exists && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            return fileInfo.FullName;
        }

        /// <summary>
        /// This method creates the path to save the file with the maximum values for each angular frequency.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the file with the maximum values for each angular frequency.</returns>
        public override string CreateMaxValuesPath(BeamRequest<TProfile> request, FiniteElementMethodInput input)
        {
            var fileInfo = new FileInfo(Path.Combine(
                TemplateBasePath, 
                $"MaxValues/{request.NumericalMethod}",
                $"MaxValues_{request.AnalysisType}_{request.Profile.GetType().Name}_w0={Math.Round(request.InitialAngularFrequency, 2)}_wf={Math.Round(request.FinalAngularFrequency, 2)}_nEl={request.NumberOfElements}.csv"));

            if (fileInfo.Exists && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            return fileInfo.FullName;
        }
    }
}