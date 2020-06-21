using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations.BoundaryCondition;
using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
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
    public abstract class CalculateBeamWithDvaVibration<TProfile> : CalculateVibration_FiniteElements<BeamWithDvaRequest<TProfile>, TProfile, BeamWithDva<TProfile>>, ICalculateBeamWithDvaVibration<TProfile>
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
        /// <param name="file"></param>
        /// <param name="time"></param>
        /// <param name="newmarkMethod"></param>
        /// <param name="naturalFrequency"></param>
        public CalculateBeamWithDvaVibration(
            IBoundaryCondition boundaryCondition,
            IArrayOperation arrayOperation,
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IBeamWithDvaMainMatrix<TProfile> mainMatrix,
            IFile file,
            ITime time,
            INewmarkMethod newmarkMethod,
            INaturalFrequency naturalFrequency)
            : base(file, time, newmarkMethod, naturalFrequency)
        {
            this._boundaryCondition = boundaryCondition;
            this._arrayOperation = arrayOperation;
            this._geometricProperty = geometricProperty;
            this._mappingResolver = mappingResolver;
            this._mainMatrix = mainMatrix;
        }

        public override async Task<BeamWithDva<TProfile>> BuildBeam(BeamWithDvaRequest<TProfile> request, uint degreesOfFreedom)
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
                dvaMasses[i] = dva.DvaMass;
                dvaStiffnesses[i] = dva.DvaStiffness;
                dvaNodePositions[i] = dva.DvaNodePosition;
                i += 1;
            }

            GeometricProperty geometricProperty = new GeometricProperty();

            if (request.Profile.Area != default && request.Profile.MomentOfInertia != default)
            {
                geometricProperty.Area = await this._arrayOperation.CreateVector(request.Profile.Area.Value, request.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await this._arrayOperation.CreateVector(request.Profile.MomentOfInertia.Value, request.NumberOfElements).ConfigureAwait(false);
            }
            else
            {
                geometricProperty.Area = await this._geometricProperty.CalculateArea(request.Profile, request.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await this._geometricProperty.CalculateMomentOfInertia(request.Profile, request.NumberOfElements).ConfigureAwait(false);
            }

            return new BeamWithDva<TProfile>()
            {
                DvaMasses = dvaMasses,
                DvaNodePositions = dvaNodePositions,
                DvaStiffnesses = dvaStiffnesses,
                Fastenings = await this._mappingResolver.BuildFastenings(request.Fastenings).ConfigureAwait(false),
                Forces = await this._mappingResolver.BuildForceVector(request.Forces, degreesOfFreedom).ConfigureAwait(false),
                GeometricProperty = geometricProperty,
                Length = request.Length,
                Material = MaterialFactory.Create(request.Material),
                NumberOfElements = request.NumberOfElements,
                Profile = request.Profile
            };
        }

        public override async Task<FiniteElementsMethodInput> CreateInput(BeamWithDvaRequest<TProfile> request)
        {
            uint degreesOfFreedom = await base.CalculateDegreesFreedomMaximum(request.NumberOfElements).ConfigureAwait(false);

            BeamWithDva<TProfile> beam = await this.BuildBeam(request, degreesOfFreedom).ConfigureAwait(false);

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
            FiniteElementsMethodInput input = new FiniteElementsMethodInput(NumericalMethodFactory.Create(request.NumericalMethod))
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

        public override Task<string> CreateSolutionPath(BeamWithDvaRequest<TProfile> request, FiniteElementsMethodInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions/FiniteElements/BeamWithDva/{request.Profile.GetType().Name}/nEl={request.NumberOfElements}/{request.NumericalMethod}");

            string fileName = $"{request.AnalysisType}_w={Math.Round(input.AngularFrequency, 2)}_nEl={request.NumberOfElements}.csv";

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

            return Task.FromResult(path);
        }

        public override Task<string> CreateMaxValuesPath(BeamWithDvaRequest<TProfile> request, FiniteElementsMethodInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions/FiniteElements/BeamWithDva/MaxValues/{request.NumericalMethod}");

            string fileName = $"MaxValues_{request.AnalysisType}_{request.Profile.GetType().Name}_NumberOfDvas={request.Dvas.Count}_w0={Math.Round(request.InitialAngularFrequency, 2)}_wf={Math.Round(request.FinalAngularFrequency, 2)}_nEl={request.NumberOfElements}.csv";

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

            return Task.FromResult(path);
        }
    }
}
