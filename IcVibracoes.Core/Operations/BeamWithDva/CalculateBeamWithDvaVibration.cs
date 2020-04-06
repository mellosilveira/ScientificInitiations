using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithDva;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Mapper.Profiles;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.Characteristics;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark.BeamWithDva;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.CalculateVibration.BeamWithDynamicVibrationAbsorber;
using IcVibracoes.Methods.AuxiliarOperations;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.BeamWithDva
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with dynamic vibration absorber.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class CalculateBeamWithDvaVibration<TProfile> : CalculateVibration<CalculateBeamWithDvaVibrationRequest<TProfile>, BeamWithDvaRequestData<TProfile>, TProfile, BeamWithDva<TProfile>>, ICalculateBeamWithDvaVibration<TProfile>
        where TProfile : Profile, new()
    {
        private readonly IMappingResolver _mappingResolver;
        private readonly IAuxiliarOperation _auxiliarOperation;
        private readonly IProfileMapper<TProfile> _profileMapper;
        private readonly IBeamWithDvaMainMatrix<TProfile> _mainMatrix;
        private readonly IBeamMainMatrix<TProfile> _beamMainMatrix;
        private readonly IArrayOperation _arrayOperation;

        /// <summary>
        /// Class contructor.
        /// </summary>
        /// <param name="newmarkMethod"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="profileValidator"></param>
        /// <param name="auxiliarOperation"></param>
        /// <param name="profileMapper"></param>
        /// <param name="mainMatrix"></param>
        /// <param name="beamMainMatrix"></param>
        /// <param name="arrayOperation"></param>
        public CalculateBeamWithDvaVibration(
            IBeamWithDvaNewmarkMethod newmarkMethod,
            IMappingResolver mappingResolver,
            IProfileValidator<TProfile> profileValidator,
            IAuxiliarOperation auxiliarOperation,
            IProfileMapper<TProfile> profileMapper,
            IBeamWithDvaMainMatrix<TProfile> mainMatrix,
            IBeamMainMatrix<TProfile> beamMainMatrix,
            IArrayOperation arrayOperation)
            : base(newmarkMethod, profileValidator, auxiliarOperation)
        {
            this._mappingResolver = mappingResolver;
            this._auxiliarOperation = auxiliarOperation;
            this._profileMapper = profileMapper;
            this._mainMatrix = mainMatrix;
            this._beamMainMatrix = beamMainMatrix;
            this._arrayOperation = arrayOperation;
        }

        /// <summary>
        /// Builds the beam with dynamic vibration absorbers object.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public async override Task<BeamWithDva<TProfile>> BuildBeam(CalculateBeamWithDvaVibrationRequest<TProfile> request, uint degreesFreedomMaximum)
        {
            if (request == null)
            {
                return null;
            }

            int i = 0;

            double[] dvaMasses = new double[request.BeamData.Dvas.Count];
            double[] dvaHardnesses = new double[request.BeamData.Dvas.Count];
            uint[] dvaNodePositions = new uint[request.BeamData.Dvas.Count];

            foreach (DynamicVibrationAbsorber dva in request.BeamData.Dvas)
            {
                dvaMasses[i] = dva.DvaMass;
                dvaHardnesses[i] = dva.DvaHardness;
                dvaNodePositions[i] = dva.DvaNodePosition;
                i += 1;
            }

            GeometricProperty geometricProperty = new GeometricProperty();

            if (request.BeamData.Profile.Area != default && request.BeamData.Profile.MomentOfInertia != default)
            {
                geometricProperty.Area = await this._arrayOperation.CreateVector(request.BeamData.Profile.Area.Value, request.BeamData.NumberOfElements, nameof(request.BeamData.Profile.Area));
                geometricProperty.MomentOfInertia = await this._arrayOperation.CreateVector(request.BeamData.Profile.MomentOfInertia.Value, request.BeamData.NumberOfElements, nameof(request.BeamData.Profile.MomentOfInertia));
            }
            else
            {
                geometricProperty = await this._profileMapper.Execute(request.BeamData.Profile, request.BeamData.NumberOfElements);
            }

            return new BeamWithDva<TProfile>()
            {
                FirstFastening = FasteningFactory.Create(request.BeamData.FirstFastening),
                Forces = await this._mappingResolver.BuildFrom(request.BeamData.Forces, degreesFreedomMaximum),
                GeometricProperty = geometricProperty,
                LastFastening = FasteningFactory.Create(request.BeamData.LastFastening),
                Length = request.BeamData.Length,
                Material = MaterialFactory.Create(request.BeamData.Material),
                NumberOfElements = request.BeamData.NumberOfElements,
                Profile = request.BeamData.Profile,
                DvaHardnesses = dvaHardnesses,
                DvaMasses = dvaMasses,
                DvaNodePositions = dvaNodePositions
            };
        }

        /// <summary>
        /// Creates the Newmark method input.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="newmarkMethodParameter"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public async override Task<NewmarkMethodInput> CreateInput(BeamWithDva<TProfile> beam, NewmarkMethodParameter newmarkMethodParameter, uint degreesFreedomMaximum)
        {
            bool[] bondaryCondition = await this._mainMatrix.CalculateBondaryCondition(beam.FirstFastening, beam.LastFastening, degreesFreedomMaximum, (uint)beam.DvaNodePositions.Length);
            uint numberOfTrueBoundaryConditions = 0;

            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                if (bondaryCondition[i] == true)
                {
                    numberOfTrueBoundaryConditions += 1;
                }
            }

            // Main matrixes to create input.
            double[,] mass = await this._beamMainMatrix.CalculateMass(beam, degreesFreedomMaximum);

            double[,] hardness = await this._beamMainMatrix.CalculateHardness(beam, degreesFreedomMaximum);

            double[,] massWithDva = await this._mainMatrix.CalculateMassWithDva(mass, beam.DvaMasses, beam.DvaNodePositions);

            double[,] hardnessWithDva = await this._mainMatrix.CalculateHardnessWithDva(hardness, beam.DvaHardnesses, beam.DvaNodePositions);

            double[,] dampingWithDva = await this._mainMatrix.CalculateDamping(massWithDva, hardnessWithDva);

            double[] forces = beam.Forces;

            // Creating input.
            NewmarkMethodInput input = new NewmarkMethodInput
            {
                Mass = this._auxiliarOperation.ApplyBondaryConditions(massWithDva, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                Hardness = this._auxiliarOperation.ApplyBondaryConditions(hardnessWithDva, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                Damping = this._auxiliarOperation.ApplyBondaryConditions(dampingWithDva, bondaryCondition, numberOfTrueBoundaryConditions + (uint)beam.DvaNodePositions.Length),

                Force = this._auxiliarOperation.ApplyBondaryConditions(forces, bondaryCondition, numberOfTrueBoundaryConditions),

                NumberOfTrueBoundaryConditions = numberOfTrueBoundaryConditions,

                Parameter = new NewmarkMethodParameter
                {
                    DeltaAngularFrequency = newmarkMethodParameter.DeltaAngularFrequency,
                    FinalAngularFrequency = newmarkMethodParameter.FinalAngularFrequency,
                    InitialAngularFrequency = newmarkMethodParameter.InitialAngularFrequency,
                    InitialTime = newmarkMethodParameter.InitialTime,
                    NumberOfPeriods = newmarkMethodParameter.NumberOfPeriods,
                    PeriodDivision = newmarkMethodParameter.PeriodDivision
                }
            };

            return input;
        }
    }
}
