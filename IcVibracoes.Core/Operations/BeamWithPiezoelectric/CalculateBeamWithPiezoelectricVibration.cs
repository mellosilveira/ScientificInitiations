using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric;
using IcVibracoes.Core.DTO.Input;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Mapper.PiezoelectricProfiles;
using IcVibracoes.Core.Mapper.Profiles;
using IcVibracoes.Core.Models.BeamWithPiezoelectric;
using IcVibracoes.Core.Models.Characteristics;
using IcVibracoes.Core.Models.Piezoelectric;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.CalculateVibration.BeamWithPiezoelectric;
using IcVibracoes.Methods.AuxiliarOperations;
using System;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.BeamWithPiezoelectric
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with piezoelectric.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class CalculateBeamWithPiezoelectricVibration<TProfile> : CalculateVibration<CalculateBeamWithPiezoelectricVibrationRequest<TProfile>, PiezoelectricRequestData<TProfile>, TProfile, BeamWithPiezoelectric<TProfile>>, ICalculateBeamWithPiezoelectricVibration<TProfile>
        where TProfile : Profile, new()
    {
        private readonly IMappingResolver _mappingResolver;
        private readonly IAuxiliarOperation _auxiliarOperation;
        private readonly IProfileMapper<TProfile> _profileMapper;
        private readonly IPiezoelectricProfileMapper<TProfile> _piezoelectricProfileMapper;
        private readonly IBeamWithPiezoelectricMainMatrix<TProfile> _mainMatrix;
        private readonly IArrayOperation _arrayOperation;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="newmarkMethod"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="profileValidator"></param>
        /// <param name="auxiliarOperation"></param>
        /// <param name="profileMapper"></param>
        /// <param name="mainMatrix"></param>
        /// <param name="arrayOperation"></param>
        public CalculateBeamWithPiezoelectricVibration(
            INewmarkMethod newmarkMethod,
            IMappingResolver mappingResolver,
            IProfileValidator<TProfile> profileValidator,
            IAuxiliarOperation auxiliarOperation,
            IProfileMapper<TProfile> profileMapper,
            IPiezoelectricProfileMapper<TProfile> piezoelectricProfileMapper,
            IBeamWithPiezoelectricMainMatrix<TProfile> mainMatrix,
            IArrayOperation arrayOperation)
            : base(newmarkMethod, profileValidator, auxiliarOperation)
        {
            this._mappingResolver = mappingResolver;
            this._auxiliarOperation = auxiliarOperation;
            this._profileMapper = profileMapper;
            this._piezoelectricProfileMapper = piezoelectricProfileMapper;
            this._mainMatrix = mainMatrix;
            this._arrayOperation = arrayOperation;
        }

        /// <summary>
        /// Builds the beam with piezoelectric object.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public async override Task<BeamWithPiezoelectric<TProfile>> BuildBeam(CalculateBeamWithPiezoelectricVibrationRequest<TProfile> request, uint degreesFreedomMaximum)
        {
            if (request == null || degreesFreedomMaximum == default)
            {
                return null;
            }

            GeometricProperty geometricProperty = new GeometricProperty();
            GeometricProperty piezoelectricGeometricProperty = new GeometricProperty();

            uint numberOfPiezoelectricPerElements = PiezoelectricPositionFactory.Create(request.BeamData.PiezoelectricPosition);

            // Calculating beam geometric properties.
            if (request.BeamData.Profile.Area != default && request.BeamData.Profile.MomentOfInertia != default)
            {
                geometricProperty.Area = await this._arrayOperation.CreateVector(request.BeamData.Profile.Area.Value, request.BeamData.NumberOfElements, nameof(request.BeamData.Profile.Area));
                geometricProperty.MomentOfInertia = await this._arrayOperation.CreateVector(request.BeamData.Profile.MomentOfInertia.Value, request.BeamData.NumberOfElements, nameof(request.BeamData.Profile.MomentOfInertia));
            }
            else
            {
                geometricProperty = await this._profileMapper.Execute(request.BeamData.Profile, request.BeamData.NumberOfElements);
            }

            // Calculating piezoelectric geometric properties.
            if (request.BeamData.PiezoelectricProfile.Area != default && request.BeamData.PiezoelectricProfile.MomentOfInertia != default)
            {
                double area = request.BeamData.PiezoelectricProfile.Area.Value * numberOfPiezoelectricPerElements;
                double momentOfInertia = request.BeamData.PiezoelectricProfile.MomentOfInertia.Value * numberOfPiezoelectricPerElements;

                piezoelectricGeometricProperty.Area = await this._arrayOperation.CreateVector(area, request.BeamData.NumberOfElements, request.BeamData.ElementsWithPiezoelectric, nameof(area));
                piezoelectricGeometricProperty.MomentOfInertia = await this._arrayOperation.CreateVector(momentOfInertia, request.BeamData.NumberOfElements, request.BeamData.ElementsWithPiezoelectric, nameof(momentOfInertia));
            }
            else
            {
                piezoelectricGeometricProperty = await this._piezoelectricProfileMapper.Execute(request.BeamData.PiezoelectricProfile, request.BeamData.Profile, numberOfPiezoelectricPerElements, request.BeamData.ElementsWithPiezoelectric, request.BeamData.NumberOfElements);
            }

            return new BeamWithPiezoelectric<TProfile>()
            {
                DielectricConstant = request.BeamData.DielectricConstant,
                DielectricPermissiveness = request.BeamData.DielectricPermissiveness,
                ElasticityConstant = request.BeamData.ElasticityConstant,
                ElementsWithPiezoelectric = request.BeamData.ElementsWithPiezoelectric,
                FirstFastening = FasteningFactory.Create(request.BeamData.FirstFastening),
                Forces = await this._mappingResolver.BuildFrom(request.BeamData.Forces, degreesFreedomMaximum),
                GeometricProperty = geometricProperty,
                LastFastening = FasteningFactory.Create(request.BeamData.LastFastening),
                Length = request.BeamData.Length,
                Material = MaterialFactory.Create(request.BeamData.Material),
                NumberOfElements = request.BeamData.NumberOfElements,
                NumberOfPiezoelectricPerElements = numberOfPiezoelectricPerElements,
                PiezoelectricConstant = request.BeamData.PiezoelectricConstant,
                PiezoelectricProfile = request.BeamData.PiezoelectricProfile,
                PiezoelectricGeometricProperty = piezoelectricGeometricProperty,
                Profile = request.BeamData.Profile,
                PiezoelectricSpecificMass = request.BeamData.PiezoelectricSpecificMass,
                YoungModulus = request.BeamData.PiezoelectricYoungModulus,
                ElectricalCharge = new double[request.BeamData.NumberOfElements + 1]
            };
        }

        /// <summary>
        /// Creates the Newmark method input.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="newmarkMethodParameter"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public async override Task<NewmarkMethodInput> CreateInput(BeamWithPiezoelectric<TProfile> beam, NewmarkMethodParameter newmarkMethodParameter, uint degreesFreedomMaximum)
        {
            uint piezoelectricDegreesFreedomMaximum = beam.NumberOfElements + 1;

            bool[] beamBondaryConditions = await this._mainMatrix.CalculateBondaryCondition(beam.FirstFastening, beam.LastFastening, degreesFreedomMaximum);
            uint numberOfTrueBeamBoundaryConditions = 0;

            for (int i = 0; i < degreesFreedomMaximum; i++)
            {
                if (beamBondaryConditions[i] == true)
                {
                    numberOfTrueBeamBoundaryConditions += 1;
                }
            }

            bool[] piezoelectricBondaryConditions = await this._mainMatrix.CalculatePiezoelectricBondaryCondition(piezoelectricDegreesFreedomMaximum, beam.ElementsWithPiezoelectric);
            uint numberOfTruePiezoelectricBoundaryConditions = 0;

            for (int i = 0; i < piezoelectricDegreesFreedomMaximum; i++)
            {
                if (piezoelectricBondaryConditions[i] == true)
                {
                    numberOfTruePiezoelectricBoundaryConditions += 1;
                }
            }

            bool[] bondaryConditions = await this._arrayOperation.MergeVectors<bool>(beamBondaryConditions, piezoelectricBondaryConditions);
            uint numberOfTrueBoundaryConditions = numberOfTrueBeamBoundaryConditions + numberOfTruePiezoelectricBoundaryConditions;

            // Main matrixes to create input.
            double[,] mass = await this._mainMatrix.CalculateMass(beam, degreesFreedomMaximum);

            double[,] hardness = await this._mainMatrix.CalculateHardness(beam, degreesFreedomMaximum);

            double[,] piezoelectricElectromechanicalCoupling = await this._mainMatrix.CalculatePiezoelectricElectromechanicalCoupling(beam, degreesFreedomMaximum);

            double[,] piezoelectricCapacitance = await this._mainMatrix.CalculatePiezoelectricCapacitance(beam);

            double[,] equivalentMass = await this._mainMatrix.CalculateEquivalentMass(mass, degreesFreedomMaximum, piezoelectricDegreesFreedomMaximum);

            double[,] equivalentHardness = await this._mainMatrix.CalculateEquivalentHardness(hardness, piezoelectricElectromechanicalCoupling, piezoelectricCapacitance, degreesFreedomMaximum, piezoelectricDegreesFreedomMaximum);

            double[,] damping = await this._mainMatrix.CalculateDamping(equivalentMass, equivalentHardness);

            double[] force = beam.Forces;

            double[] electricalCharge = beam.ElectricalCharge;

            double[] equivalentForce = await this._arrayOperation.MergeVectors<double>(force, electricalCharge);

            // Creating input.
            NewmarkMethodInput input = new NewmarkMethodInput
            {
                Mass = this._auxiliarOperation.ApplyBondaryConditions(equivalentMass, bondaryConditions, numberOfTrueBoundaryConditions),

                Hardness = this._auxiliarOperation.ApplyBondaryConditions(equivalentHardness, bondaryConditions, numberOfTrueBoundaryConditions),

                Damping = this._auxiliarOperation.ApplyBondaryConditions(damping, bondaryConditions, numberOfTrueBoundaryConditions),

                Force = this._auxiliarOperation.ApplyBondaryConditions(equivalentForce, bondaryConditions, numberOfTrueBoundaryConditions),

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
