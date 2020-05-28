using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.FiniteElements;
using IcVibracoes.DataContracts.FiniteElements.BeamWithDynamicVibrationAbsorber;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with dynamic vibration absorber.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class CalculateBeamWithDvaVibration<TProfile> : CalculateVibration_FiniteElements<BeamWithDvaRequest<TProfile>, BeamWithDvaRequestData<TProfile>, TProfile, BeamWithDva<TProfile>>, ICalculateBeamWithDvaVibration<TProfile>
        where TProfile : Profile, new()
    {
        private readonly IAuxiliarOperation _auxiliarOperation;
        private readonly IArrayOperation _arrayOperation;
        private readonly IGeometricProperty<TProfile> _geometricProperty;
        private readonly IMappingResolver _mappingResolver;
        private readonly IBeamWithDvaMainMatrix<TProfile> _mainMatrix;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="newmarkMethod"></param>
        /// <param name="profileValidator"></param>
        /// <param name="auxiliarOperation"></param>
        /// <param name="time"></param>
        /// <param name="arrayOperation"></param>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="mainMatrix"></param>
        public CalculateBeamWithDvaVibration(
            INewmarkMethod newmarkMethod,
            IProfileValidator<TProfile> profileValidator,
            IAuxiliarOperation auxiliarOperation,
            ITime time,
            IArrayOperation arrayOperation,
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IBeamWithDvaMainMatrix<TProfile> mainMatrix)
            : base(newmarkMethod, profileValidator, auxiliarOperation, time)
        {
            _auxiliarOperation = auxiliarOperation;
            _arrayOperation = arrayOperation;
            _geometricProperty = geometricProperty;
            _mappingResolver = mappingResolver;
            _mainMatrix = mainMatrix;
        }

        public override async Task<BeamWithDva<TProfile>> BuildBeam(BeamWithDvaRequest<TProfile> request, uint degreesOfFreedom)
        {
            if (request == null)
            {
                return null;
            }

            int i = 0;

            double[] dvaMasses = new double[request.Data.Dvas.Count];
            double[] dvaStiffnesses = new double[request.Data.Dvas.Count];
            uint[] dvaNodePositions = new uint[request.Data.Dvas.Count];

            foreach (DynamicVibrationAbsorber dva in request.Data.Dvas)
            {
                dvaMasses[i] = dva.DvaMass;
                dvaStiffnesses[i] = dva.DvaStiffness;
                dvaNodePositions[i] = dva.DvaNodePosition;
                i += 1;
            }

            GeometricProperty geometricProperty = new GeometricProperty();

            if (request.Data.Profile.Area != default && request.Data.Profile.MomentOfInertia != default)
            {
                geometricProperty.Area = await _arrayOperation.CreateVector(request.Data.Profile.Area.Value, request.Data.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await _arrayOperation.CreateVector(request.Data.Profile.MomentOfInertia.Value, request.Data.NumberOfElements).ConfigureAwait(false);
            }
            else
            {
                geometricProperty.Area = await _geometricProperty.CalculateArea(request.Data.Profile, request.Data.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await _geometricProperty.CalculateMomentOfInertia(request.Data.Profile, request.Data.NumberOfElements).ConfigureAwait(false);
            }

            return new BeamWithDva<TProfile>()
            {
                DvaMasses = dvaMasses,
                DvaNodePositions = dvaNodePositions,
                DvaStiffnesses = dvaStiffnesses,
                Fastenings = await _mappingResolver.BuildFastenings(request.Data.Fastenings).ConfigureAwait(false),
                Forces = await _mappingResolver.BuildForceVector(request.Data.Forces, degreesOfFreedom).ConfigureAwait(false),
                GeometricProperty = geometricProperty,
                Length = request.Data.Length,
                Material = MaterialFactory.Create(request.Data.Material),
                NumberOfElements = request.Data.NumberOfElements,
                Profile = request.Data.Profile
            };
        }

        public override async Task<NewmarkMethodInput> CreateInput(BeamWithDva<TProfile> beam, BeamWithDvaRequest<TProfile> request, uint degreesOfFreedom)
        {
            bool[] bondaryCondition = await _mainMatrix.CalculateBondaryCondition(beam.Fastenings, degreesOfFreedom + (uint)beam.DvaNodePositions.Length).ConfigureAwait(false);
            uint numberOfTrueBoundaryConditions = 0;

            for (int i = 0; i < degreesOfFreedom; i++)
            {
                if (bondaryCondition[i] == true)
                {
                    numberOfTrueBoundaryConditions += 1;
                }
            }

            // Main matrixes to create input.
            double[,] mass = await _mainMatrix.CalculateMass(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] stiffness = await _mainMatrix.CalculateStiffness(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] massWithDva = await _mainMatrix.CalculateMassWithDva(mass, beam.DvaMasses, beam.DvaNodePositions).ConfigureAwait(false);

            double[,] stiffnessWithDva = await _mainMatrix.CalculateStiffnessWithDva(stiffness, beam.DvaStiffnesses, beam.DvaNodePositions).ConfigureAwait(false);

            double[,] dampingWithDva = await _mainMatrix.CalculateDamping(massWithDva, stiffnessWithDva).ConfigureAwait(false);

            double[] forces = beam.Forces;

            // Creating input.
            NewmarkMethodInput input = new NewmarkMethodInput
            {
                Mass = _auxiliarOperation.ApplyBondaryConditions(massWithDva, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                Stiffness = _auxiliarOperation.ApplyBondaryConditions(stiffnessWithDva, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                Damping = _auxiliarOperation.ApplyBondaryConditions(dampingWithDva, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                OriginalForce = _auxiliarOperation.ApplyBondaryConditions(forces, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                NumberOfTrueBoundaryConditions = numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length,

                AngularFrequency = request.Data.InitialAngularFrequency,

                AngularFrequencyStep = request.Data.AngularFrequencyStep,

                FinalAngularFrequency = request.Data.FinalAngularFrequency
            };

            return input;
        }

        public override Task<string> CreateSolutionPath(BeamWithDvaRequest<TProfile> request, NewmarkMethodInput input, FiniteElementsResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string folderPath = Path.Combine(
                previousPath,
                $"Solutions/FiniteElements/BeamWithDva/{request.Data.Profile.GetType().Name}/nEl={request.Data.NumberOfElements}");

            string fileName = $"{request.AnalysisType.Trim()}_w={Math.Round(input.AngularFrequency, 2)}_nEl={request.Data.NumberOfElements}.csv";

            string path = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            return Task.FromResult(path);
        }

        public override Task<string> CreateMaxValuesPath(BeamWithDvaRequest<TProfile> request, NewmarkMethodInput input, FiniteElementsResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string folderPath = Path.Combine(
                previousPath,
                $"Solutions/FiniteElements/BeamWithDva/MaxValues");

            string fileName = $"MaxValues_{request.AnalysisType.Trim()}_{request.Data.Profile.GetType().Name}_NumberOfDvas={request.Data.Dvas.Count}_w0={Math.Round(request.Data.InitialAngularFrequency, 2)}_wf={Math.Round(request.Data.FinalAngularFrequency, 2)}_nEl={request.Data.NumberOfElements}.csv";

            string path = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            return Task.FromResult(path);
        }
    }
}
