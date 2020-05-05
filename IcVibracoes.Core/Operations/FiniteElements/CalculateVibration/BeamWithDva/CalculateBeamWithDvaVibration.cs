using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.AuxiliarOperations.TimeOperation;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva;
using IcVibracoes.Core.DTO.InputData.FiniteElements;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.Newmark;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.FiniteElements;
using IcVibracoes.DataContracts.FiniteElements.BeamWithDynamicVibrationAbsorber;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithDva
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
            this._auxiliarOperation = auxiliarOperation;
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

            int i = 0;

            double[] dvaMasses = new double[request.BeamData.Dvas.Count];
            double[] dvaStiffnesses = new double[request.BeamData.Dvas.Count];
            uint[] dvaNodePositions = new uint[request.BeamData.Dvas.Count];

            foreach (DynamicVibrationAbsorber dva in request.BeamData.Dvas)
            {
                dvaMasses[i] = dva.DvaMass;
                dvaStiffnesses[i] = dva.DvaStiffness;
                dvaNodePositions[i] = dva.DvaNodePosition;
                i += 1;
            }

            GeometricProperty geometricProperty = new GeometricProperty();

            if (request.BeamData.Profile.Area != default && request.BeamData.Profile.MomentOfInertia != default)
            {
                geometricProperty.Area = await this._arrayOperation.CreateVector(request.BeamData.Profile.Area.Value, request.BeamData.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await this._arrayOperation.CreateVector(request.BeamData.Profile.MomentOfInertia.Value, request.BeamData.NumberOfElements).ConfigureAwait(false);
            }
            else
            {
                geometricProperty.Area = await this._geometricProperty.CalculateArea(request.BeamData.Profile, request.BeamData.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await this._geometricProperty.CalculateMomentOfInertia(request.BeamData.Profile, request.BeamData.NumberOfElements).ConfigureAwait(false);
            }

            return new BeamWithDva<TProfile>()
            {
                DvaMasses = dvaMasses,
                DvaNodePositions = dvaNodePositions,
                DvaStiffnesses = dvaStiffnesses,
                Fastenings = await this._mappingResolver.BuildFastenings(request.BeamData.Fastenings).ConfigureAwait(false),
                Forces = await this._mappingResolver.BuildForceVector(request.BeamData.Forces, degreesOfFreedom).ConfigureAwait(false),
                GeometricProperty = geometricProperty,
                Length = request.BeamData.Length,
                Material = MaterialFactory.Create(request.BeamData.Material),
                NumberOfElements = request.BeamData.NumberOfElements,
                Profile = request.BeamData.Profile
            };
        }

        public override async Task<NewmarkMethodInput> CreateInput(BeamWithDva<TProfile> beam, BeamWithDvaRequest<TProfile> request, uint degreesOfFreedom)
        {
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
            NewmarkMethodInput input = new NewmarkMethodInput
            {
                Mass = this._auxiliarOperation.ApplyBondaryConditions(massWithDva, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                Stiffness = this._auxiliarOperation.ApplyBondaryConditions(stiffnessWithDva, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                Damping = this._auxiliarOperation.ApplyBondaryConditions(dampingWithDva, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                OriginalForce = this._auxiliarOperation.ApplyBondaryConditions(forces, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                NumberOfTrueBoundaryConditions = numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length,

                AngularFrequency = request.BeamData.InitialAngularFrequency,

                AngularFrequencyStep = request.BeamData.AngularFrequencyStep,

                FinalAngularFrequency = request.BeamData.FinalAngularFrequency
            };

            return input;
        }

        public override Task<string> CreatePath(string analysisType, double angularFrequency, uint numberOfElements, FiniteElementsResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string folderPath = Path.Combine(
                previousPath,
                $"Solutions/FiniteElements/BeamWithDva");

            string fileName = $"{analysisType.Trim()}_w={Math.Round(angularFrequency, 2)}_nEl={numberOfElements}.csv";

            string path = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            return Task.FromResult(path);
        }
    }
}
