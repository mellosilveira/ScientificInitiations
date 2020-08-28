using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Classes;
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
using IcVibracoes.DataContracts;
using IcVibracoes.DataContracts.FiniteElement;
using IcVibracoes.DataContracts.FiniteElement.BeamWithDynamicVibrationAbsorber;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with dynamic vibration absorber.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class CalculateBeamWithDvaVibration<TProfile> : CalculateVibration_FiniteElement<BeamWithDvaRequest<TProfile>, TProfile, BeamWithDva<TProfile>>, ICalculateBeamWithDvaVibration<TProfile>
        where TProfile : Profile, new()
    {
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
        public CalculateBeamWithDvaVibration(
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IProfileValidator<TProfile> profileValidator, 
            ITime time, 
            INaturalFrequency naturalFrequency, 
            IBeamWithDvaMainMatrix<TProfile> mainMatrix) 
            : base(profileValidator, time, naturalFrequency, mainMatrix)
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
        public override async Task<BeamWithDva<TProfile>> BuildBeam(BeamWithDvaRequest<TProfile> request, uint degreesOfFreedom)
        {
            double[] dvaMasses = new double[request.Dvas.Count];
            double[] dvaStiffnesses = new double[request.Dvas.Count];
            uint[] dvaNodePositions = new uint[request.Dvas.Count];

            int i = 0;
            foreach (DynamicVibrationAbsorber dva in request.Dvas)
            {
                dvaMasses[i] = dva.Mass;
                dvaStiffnesses[i] = dva.Stiffness;
                dvaNodePositions[i] = dva.NodePosition;
                i += 1;
            }

            GeometricProperty geometricProperty = new GeometricProperty();

            if (request.Profile.Area != 0 && request.Profile.MomentOfInertia != 0)
            {
                geometricProperty.Area = await ArrayFactory.CreateVectorAsync(request.Profile.Area.Value, request.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await ArrayFactory.CreateVectorAsync(request.Profile.MomentOfInertia.Value, request.NumberOfElements).ConfigureAwait(false);
            }
            else
            {
                geometricProperty.Area = await this._geometricProperty.CalculateArea(request.Profile, request.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await this._geometricProperty.CalculateMomentOfInertia(request.Profile, request.NumberOfElements).ConfigureAwait(false);
            }

            var beam = new BeamWithDva<TProfile>()
            {
                DvaMasses = dvaMasses,
                DvaNodePositions = dvaNodePositions,
                DvaStiffnesses = dvaStiffnesses,
                Fastenings = await this._mappingResolver.BuildFastenings(request.Fastenings).ConfigureAwait(false),
                Forces = await this._mappingResolver.BuildForceVector(request.Forces, degreesOfFreedom + (uint)request.Dvas.Count).ConfigureAwait(false),
                GeometricProperty = geometricProperty,
                Length = request.Length,
                Material = MaterialFactory.Create(request.Material),
                NumberOfElements = request.NumberOfElements,
                Profile = request.Profile
            };

            return beam;
        }

        /// <summary>
        /// This method creates the path to save the solution files.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the solution files.</returns>
        public override Task<string> CreateSolutionPath(BeamWithDvaRequest<TProfile> request, FiniteElementMethodInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions/FiniteElement/BeamWithDva/{request.Profile.GetType().Name}/nEl={request.NumberOfElements}/{request.NumericalMethod}");

            string fileName = $"{request.AnalysisType}_{request.Profile.GetType().Name}_NumberOfDvas={request.Dvas.Count}_w={Math.Round(input.AngularFrequency, 2)}_nEl={request.NumberOfElements}.csv";

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

            return Task.FromResult(path);
        }

        /// <summary>
        /// This method creates the path to save the file with the maximum values for each angular frequency.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the file with the maximum values for each angular frequency.</returns>
        public override Task<string> CreateMaxValuesPath(BeamWithDvaRequest<TProfile> request, FiniteElementMethodInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions/FiniteElement/BeamWithDva/MaxValues/{request.NumericalMethod}");

            string fileName = $"MaxValues_{request.AnalysisType}_{request.Profile.GetType().Name}_NumberOfDvas={request.Dvas.Count}_w0={Math.Round(request.InitialAngularFrequency, 2)}_wf={Math.Round(request.FinalAngularFrequency, 2)}_nEl={request.NumberOfElements}.csv";

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

            return Task.FromResult(path);
        }

        /// <summary>
        /// This method validates the <see cref="BeamWithDvaRequest{TProfile}"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<FiniteElementResponse> ValidateOperation(BeamWithDvaRequest<TProfile> request)
        {
            FiniteElementResponse response = await base.ValidateOperation(request).ConfigureAwait(false);

            foreach (var dva in request.Dvas)
            {
                if (dva.Mass < 0)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"DVA Mass: {dva.Mass} cannot be less than zero. DVA index: {request.Dvas.IndexOf(dva)}.");
                }

                if (dva.Stiffness < 0)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"DVA Stiffness: {dva.Stiffness} cannot be less than zero. DVA index: {request.Dvas.IndexOf(dva)}.");
                }

                if (dva.NodePosition < 0 || dva.NodePosition > request.NumberOfElements)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"DVA NodePosition: {dva.NodePosition} must be greather than zero and less than number of elements: {request.NumberOfElements}. DVA index: {request.Dvas.IndexOf(dva)}.");
                }
            }

            return response;
        }
    }
}
