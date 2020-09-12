using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.ExtensionMethods;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.FiniteElement;
using IcVibracoes.DataContracts.FiniteElement.BeamWithDynamicVibrationAbsorber;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IcVibracoes.Core.Calculator.GeometricProperties;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with dynamic vibration absorber.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class CalculateBeamWithDvaVibration<TProfile> : CalculateVibrationFiniteElement<BeamWithDvaRequest<TProfile>, TProfile, BeamWithDva<TProfile>>, ICalculateBeamWithDvaVibration<TProfile>
        where TProfile : Profile, new()
    {
        private static readonly string TemplateBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Solutions/FiniteElement/BeamWithDva");

        private readonly IGeometricProperty<TProfile> _geometricProperty;
        private readonly IMappingResolver _mappingResolver;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="profileValidator"></param>
        /// <param name="time"></param>
        /// <param name="naturalFrequency"></param>
        /// <param name="mainMatrix"></param>
        protected CalculateBeamWithDvaVibration(
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IProfileValidator<TProfile> profileValidator, 
            ITime time, 
            INaturalFrequency naturalFrequency, 
            IBeamWithDvaMainMatrix<TProfile> mainMatrix) : base(profileValidator, time, naturalFrequency, mainMatrix)
        {
            this._geometricProperty = geometricProperty;
            this._mappingResolver = mappingResolver;
        }

        /// <summary>
        /// This method creates a new instance of class <see cref="BeamWithDva{TProfile}"/>.
        /// This is a step to create the input fot finite element analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>A new instance of class <see cref="Beam{TProfile}"/>.</returns>
        public override BeamWithDva<TProfile> BuildBeam(BeamWithDvaRequest<TProfile> request, uint degreesOfFreedom)
        {
            double[] dvaMasses = new double[request.Dvas.Count];
            double[] dvaStiffnesses = new double[request.Dvas.Count];
            uint[] dvaNodePositions = new uint[request.Dvas.Count];

            foreach (var item in request.Dvas.Select((dva, index) => new { Dva = dva, Index = index }))
            {
                dvaMasses[item.Index] = item.Dva.Mass;
                dvaStiffnesses[item.Index] = item.Dva.Stiffness;
                dvaNodePositions[item.Index] = item.Dva.NodePosition;
            }

            GeometricProperty geometricProperty;

            if (request.Profile.Area != 0 && request.Profile.MomentOfInertia != 0)
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

            return new BeamWithDva<TProfile>
            {
                DvaMasses = dvaMasses,
                DvaNodePositions = dvaNodePositions,
                DvaStiffnesses = dvaStiffnesses,
                Fastenings = this._mappingResolver.BuildFastenings(request.Fastenings),
                Forces = this._mappingResolver.BuildForceVector(request.Forces, degreesOfFreedom + (uint)request.Dvas.Count),
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
        public override string CreateSolutionPath(BeamWithDvaRequest<TProfile> request, FiniteElementMethodInput input)
        {
            var fileInfo = new FileInfo(Path.Combine(
                TemplateBasePath,
                $"{request.Profile.GetType().Name}/nEl={request.NumberOfElements}/{request.NumericalMethod}",
                $"{request.AnalysisType}_{request.Profile.GetType().Name}_NumberOfDvas={request.Dvas.Count}_w={Math.Round(input.AngularFrequency, 2)}_nEl={request.NumberOfElements}.csv"));

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
        public override string CreateMaxValuesPath(BeamWithDvaRequest<TProfile> request, FiniteElementMethodInput input)
        {
            var fileInfo = new FileInfo(Path.Combine(
                TemplateBasePath,
                $"MaxValues/{request.NumericalMethod}",
                $"MaxValues_{request.AnalysisType}_{request.Profile.GetType().Name}_NumberOfDvas={request.Dvas.Count}_w0={Math.Round(request.InitialAngularFrequency, 2)}_wf={Math.Round(request.FinalAngularFrequency, 2)}_nEl={request.NumberOfElements}.csv"));

            if (fileInfo.Exists && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            return fileInfo.FullName;
        }

        /// <summary>
        /// This method validates the <see cref="BeamWithDvaRequest{TProfile}"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<FiniteElementResponse> ValidateOperationAsync(BeamWithDvaRequest<TProfile> request)
        {
            var response = await base.ValidateOperationAsync(request).ConfigureAwait(false);

            foreach (var dva in request.Dvas)
            {
                response
                    .AddErrorIf(() => dva.Mass < 0, $"DVA Mass: {dva.Mass} cannot be less than zero. DVA index: {request.Dvas.IndexOf(dva)}.")
                    .AddErrorIf(() => dva.Stiffness < 0, $"DVA Stiffness: {dva.Stiffness} cannot be less than zero. DVA index: {request.Dvas.IndexOf(dva)}.")
                    .AddErrorIf(() => dva.NodePosition < 0 || dva.NodePosition > request.NumberOfElements, $"DVA NodePosition: {dva.NodePosition} must be greater than zero and less than number of elements: {request.NumberOfElements}. DVA index: {request.Dvas.IndexOf(dva)}.");
            }

            return response;
        }
    }
}
