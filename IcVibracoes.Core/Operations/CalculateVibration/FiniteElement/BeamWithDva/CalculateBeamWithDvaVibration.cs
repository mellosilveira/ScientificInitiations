using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations.BoundaryCondition;
using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElement;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models;
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
        private readonly IBoundaryCondition _boundaryCondition;
        private readonly IArrayOperation _arrayOperation;
        private readonly IGeometricProperty<TProfile> _geometricProperty;
        private readonly IMappingResolver _mappingResolver;
        private readonly IBeamWithDvaMainMatrix<TProfile> _mainMatrix;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="boundaryCondition"></param>
        /// <param name="arrayOperation"></param>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="mainMatrix"></param>
        /// <param name="profileValidator"></param>
        /// <param name="file"></param>
        /// <param name="time"></param>
        /// <param name="naturalFrequency"></param>
        public CalculateBeamWithDvaVibration(
            IBoundaryCondition boundaryCondition,
            IArrayOperation arrayOperation,
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IBeamWithDvaMainMatrix<TProfile> mainMatrix,
            IProfileValidator<TProfile> profileValidator,
            IFile file,
            ITime time,
            INaturalFrequency naturalFrequency)
            : base(profileValidator, file, time, naturalFrequency)
        {
            this._boundaryCondition = boundaryCondition;
            this._arrayOperation = arrayOperation;
            this._geometricProperty = geometricProperty;
            this._mappingResolver = mappingResolver;
            this._mainMatrix = mainMatrix;
        }

        /// <summary>
        /// This method creates a new instance of class <see cref="BeamWithDva{TProfile}"/>.
        /// This is a step to create the input fot finite element analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <param name="response"></param>
        /// <returns>A new instance of class <see cref="Beam{TProfile}"/>.</returns>
        public override async Task<BeamWithDva<TProfile>> BuildBeam(BeamWithDvaRequest<TProfile> request, uint degreesOfFreedom, FiniteElementResponse response)
        {
            if (request == null)
            {
                return null;
            }

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
                geometricProperty.Area = await this._arrayOperation.CreateVector(request.Profile.Area.Value, request.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await this._arrayOperation.CreateVector(request.Profile.MomentOfInertia.Value, request.NumberOfElements).ConfigureAwait(false);
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
                Fastenings = await this._mappingResolver.BuildFastenings(request.Fastenings, response).ConfigureAwait(false),
                Forces = await this._mappingResolver.BuildForceVector(request.Forces, degreesOfFreedom).ConfigureAwait(false),
                GeometricProperty = geometricProperty,
                Length = request.Length,
                Material = MaterialFactory.Create(request.Material, response),
                NumberOfElements = request.NumberOfElements,
                Profile = request.Profile
            };

            if (response.Success == false)
            {
                return null;
            }

            return beam;
        }

        /// <summary>
        /// This method creates the input to be used in finite element analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns>A new instance of class <see cref="FiniteElementMethodInput"/>.</returns>
        public override async Task<FiniteElementMethodInput> CreateInput(BeamWithDvaRequest<TProfile> request, FiniteElementResponse response)
        {
            uint degreesOfFreedom = await base.CalculateDegreesOfFreedomMaximum(request.NumberOfElements).ConfigureAwait(false);

            BeamWithDva<TProfile> beam = await this.BuildBeam(request, degreesOfFreedom, response).ConfigureAwait(false);

            if (beam == null)
            {
                return null;
            }

            bool[] bondaryCondition = await this._mainMatrix.CalculateBondaryCondition(beam.Fastenings, degreesOfFreedom + (uint)beam.DvaNodePositions.Length).ConfigureAwait(false);
            uint numberOfTrueBoundaryConditions = 0;

            for (int i = 0; i < degreesOfFreedom; i++)
            {
                if (bondaryCondition[i] == true)
                {
                    numberOfTrueBoundaryConditions += 1;
                }
            }

            // Main matrixes to create input.
            double[,] mass = await this._mainMatrix.CalculateMass(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] stiffness = await this._mainMatrix.CalculateStiffness(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] massWithDva = await this._mainMatrix.CalculateMassWithDva(mass, beam.DvaMasses, beam.DvaNodePositions).ConfigureAwait(false);

            double[,] stiffnessWithDva = await this._mainMatrix.CalculateStiffnessWithDva(stiffness, beam.DvaStiffnesses, beam.DvaNodePositions).ConfigureAwait(false);

            double[,] dampingWithDva = await this._mainMatrix.CalculateDamping(massWithDva, stiffnessWithDva).ConfigureAwait(false);

            double[] forces = beam.Forces;

            // Creating input.
            var numericalMethod = (NumericalMethod)Enum.Parse(typeof(NumericalMethod), request.NumericalMethod, ignoreCase: true);
            FiniteElementMethodInput input = new FiniteElementMethodInput(numericalMethod)
            {
                Mass = await this._boundaryCondition.Apply(massWithDva, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                Stiffness = await this._boundaryCondition.Apply(stiffnessWithDva, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                Damping = await this._boundaryCondition.Apply(dampingWithDva, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                OriginalForce = await this._boundaryCondition.Apply(forces, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                NumberOfTrueBoundaryConditions = numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length,

                AngularFrequency = request.InitialAngularFrequency,

                AngularFrequencyStep = request.AngularFrequencyStep,

                FinalAngularFrequency = request.FinalAngularFrequency
            };

            return input;
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
