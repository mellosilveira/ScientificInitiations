using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations.BoundaryCondition;
using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.DataContracts.FiniteElements;
using IcVibracoes.DataContracts.FiniteElements.Beam;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.Beam
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class CalculateBeamVibration<TProfile, TInput> : CalculateVibration_FiniteElements<BeamRequest<TProfile>, TProfile, Beam<TProfile>, TInput>, ICalculateBeamVibration<TProfile, TInput>
        where TProfile : Profile, new()
        where TInput : NewmarkMethodInput, new()
    {
        private readonly IBoundaryCondition _boundaryCondition;
        private readonly IArrayOperation _arrayOperation;
        private readonly IGeometricProperty<TProfile> _geometricProperty;
        private readonly IMappingResolver _mappingResolver;
        private readonly IBeamMainMatrix<TProfile> _mainMatrix;

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
        public CalculateBeamVibration(
            IBoundaryCondition boundaryCondition,
            IArrayOperation arrayOperation,
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IBeamMainMatrix<TProfile> mainMatrix,
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

        public async override Task<Beam<TProfile>> BuildBeam(BeamRequest<TProfile> request, uint degreesOfFreedom)
        {
            if (request == null)
            {
                return null;
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

            return new Beam<TProfile>()
            {
                Fastenings = await this._mappingResolver.BuildFastenings(request.Fastenings).ConfigureAwait(false),
                Forces = await this._mappingResolver.BuildForceVector(request.Forces, degreesOfFreedom).ConfigureAwait(false),
                GeometricProperty = geometricProperty,
                Length = request.Length,
                Material = MaterialFactory.Create(request.Material),
                NumberOfElements = request.NumberOfElements,
                Profile = request.Profile
            };
        }

        public override async Task<TInput> CreateInput(BeamRequest<TProfile> request)
        {
            uint degreesOfFreedom = await base.CalculateDegreesFreedomMaximum(request.NumberOfElements).ConfigureAwait(false);

            Beam<TProfile> beam = await this.BuildBeam(request, degreesOfFreedom);

            bool[] bondaryCondition = await this._mainMatrix.CalculateBondaryCondition(beam.Fastenings, degreesOfFreedom).ConfigureAwait(false);
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

            double[,] damping = await this._mainMatrix.CalculateDamping(mass, stiffness).ConfigureAwait(false);

            double[] forces = beam.Forces;

            // Creating input.
            TInput input = new TInput
            {
                Mass = await this._boundaryCondition.Apply(mass, bondaryCondition, numberOfTrueBoundaryConditions),

                Stiffness = await this._boundaryCondition.Apply(stiffness, bondaryCondition, numberOfTrueBoundaryConditions),

                Damping = await this._boundaryCondition.Apply(damping, bondaryCondition, numberOfTrueBoundaryConditions),

                OriginalForce = await this._boundaryCondition.Apply(forces, bondaryCondition, numberOfTrueBoundaryConditions),

                NumberOfTrueBoundaryConditions = numberOfTrueBoundaryConditions,

                AngularFrequency = request.InitialAngularFrequency,

                AngularFrequencyStep = request.AngularFrequencyStep,

                FinalAngularFrequency = request.FinalAngularFrequency
            };

            return input;
        }

        public override Task<string> CreateSolutionPath(BeamRequest<TProfile> request, TInput input, FiniteElementsResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions/FiniteElements/Beam/{request.Profile.GetType().Name}/nEl={request.NumberOfElements}/{input.GetType().Name}");

            string fileName = $"{request.AnalysisType}_w={Math.Round(input.AngularFrequency, 2)}_nEl={request.NumberOfElements}.csv";

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

            return Task.FromResult(path);
        }

        public override Task<string> CreateMaxValuesPath(BeamRequest<TProfile> request, TInput input, FiniteElementsResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions/FiniteElements/Beam/MaxValues/{input.GetType().Name}");

            string fileName = $"MaxValues_{request.AnalysisType}_{request.Profile.GetType().Name}_w0={Math.Round(request.InitialAngularFrequency, 2)}_wf={Math.Round(request.FinalAngularFrequency, 2)}_nEl={request.NumberOfElements}.csv";

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

            return Task.FromResult(path);
        }
    }
}