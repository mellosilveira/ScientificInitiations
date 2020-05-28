using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.NumericalIntegrationMethods.Newmark;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.FiniteElements;
using IcVibracoes.DataContracts.FiniteElements.BeamWithPiezoelectric;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithPiezoelectric
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with piezoelectric.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class CalculateBeamWithPiezoelectricVibration<TProfile> : CalculateVibration_FiniteElements<BeamWithPiezoelectricRequest<TProfile>, PiezoelectricRequestData<TProfile>, TProfile, BeamWithPiezoelectric<TProfile>>, ICalculateBeamWithPiezoelectricVibration<TProfile>
        where TProfile : Profile, new()
    {
        private readonly IAuxiliarOperation _auxiliarOperation;
        private readonly IArrayOperation _arrayOperation;
        private readonly IGeometricProperty<TProfile> _geometricProperty;
        private readonly IMappingResolver _mappingResolver;
        private readonly IBeamWithPiezoelectricMainMatrix<TProfile> _mainMatrix;

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
        public CalculateBeamWithPiezoelectricVibration(
            INewmarkMethod newmarkMethod,
            IProfileValidator<TProfile> profileValidator,
            IAuxiliarOperation auxiliarOperation,
            ITime time,
            IArrayOperation arrayOperation,
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IBeamWithPiezoelectricMainMatrix<TProfile> mainMatrix)
            : base(newmarkMethod, profileValidator, auxiliarOperation, time)
        {
            _auxiliarOperation = auxiliarOperation;
            _arrayOperation = arrayOperation;
            _geometricProperty = geometricProperty;
            _mappingResolver = mappingResolver;
            _mainMatrix = mainMatrix;
        }

        public override async Task<BeamWithPiezoelectric<TProfile>> BuildBeam(BeamWithPiezoelectricRequest<TProfile> request, uint degreesOfFreedom)
        {
            if (request == null)
            {
                return null;
            }

            GeometricProperty geometricProperty = new GeometricProperty();
            GeometricProperty piezoelectricGeometricProperty = new GeometricProperty();

            uint numberOfPiezoelectricPerElements = PiezoelectricPositionFactory.Create(request.Data.PiezoelectricPosition);

            // Calculating beam geometric properties.
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

            // Calculating piezoelectric geometric properties.
            if (request.Data.PiezoelectricProfile.Area != default && request.Data.PiezoelectricProfile.MomentOfInertia != default)
            {
                double area = request.Data.PiezoelectricProfile.Area.Value * numberOfPiezoelectricPerElements;
                double momentOfInertia = request.Data.PiezoelectricProfile.MomentOfInertia.Value * numberOfPiezoelectricPerElements;

                piezoelectricGeometricProperty.Area = await _arrayOperation.CreateVector(area, request.Data.NumberOfElements, request.Data.ElementsWithPiezoelectric).ConfigureAwait(false);
                piezoelectricGeometricProperty.MomentOfInertia = await _arrayOperation.CreateVector(momentOfInertia, request.Data.NumberOfElements, request.Data.ElementsWithPiezoelectric).ConfigureAwait(false);
            }
            else
            {
                piezoelectricGeometricProperty.Area = await _geometricProperty.CalculatePiezoelectricArea(request.Data.PiezoelectricProfile, request.Data.NumberOfElements, request.Data.ElementsWithPiezoelectric, numberOfPiezoelectricPerElements).ConfigureAwait(false);
                piezoelectricGeometricProperty.MomentOfInertia = await _geometricProperty.CalculatePiezoelectricMomentOfInertia(request.Data.PiezoelectricProfile, request.Data.Profile, request.Data.NumberOfElements, request.Data.ElementsWithPiezoelectric, numberOfPiezoelectricPerElements).ConfigureAwait(false);
            }

            return new BeamWithPiezoelectric<TProfile>()
            {
                DielectricConstant = request.Data.DielectricConstant,
                DielectricPermissiveness = request.Data.DielectricPermissiveness,
                ElasticityConstant = request.Data.ElasticityConstant,
                ElectricalCharge = new double[request.Data.NumberOfElements + 1],
                ElementsWithPiezoelectric = request.Data.ElementsWithPiezoelectric,
                Fastenings = await _mappingResolver.BuildFastenings(request.Data.Fastenings),
                Forces = await _mappingResolver.BuildForceVector(request.Data.Forces, degreesOfFreedom).ConfigureAwait(false),
                GeometricProperty = geometricProperty,
                Length = request.Data.Length,
                Material = MaterialFactory.Create(request.Data.Material),
                NumberOfElements = request.Data.NumberOfElements,
                NumberOfPiezoelectricPerElements = numberOfPiezoelectricPerElements,
                PiezoelectricConstant = request.Data.PiezoelectricConstant,
                PiezoelectricGeometricProperty = piezoelectricGeometricProperty,
                PiezoelectricProfile = request.Data.PiezoelectricProfile,
                PiezoelectricSpecificMass = request.Data.PiezoelectricSpecificMass,
                PiezoelectricYoungModulus = request.Data.PiezoelectricYoungModulus,
                Profile = request.Data.Profile
            };
        }

        public override async Task<NewmarkMethodInput> CreateInput(BeamWithPiezoelectric<TProfile> beam, BeamWithPiezoelectricRequest<TProfile> request, uint degreesOfFreedom)
        {
            uint piezoelectricDegreesFreedomMaximum = beam.NumberOfElements + 1;

            bool[] beamBondaryConditions = await _mainMatrix.CalculateBondaryCondition(beam.Fastenings, degreesOfFreedom).ConfigureAwait(false);
            uint numberOfTrueBeamBoundaryConditions = 0;

            for (int i = 0; i < degreesOfFreedom; i++)
            {
                if (beamBondaryConditions[i] == true)
                {
                    numberOfTrueBeamBoundaryConditions += 1;
                }
            }

            bool[] piezoelectricBondaryConditions = await _mainMatrix.CalculatePiezoelectricBondaryCondition(beam.NumberOfElements, beam.ElementsWithPiezoelectric).ConfigureAwait(false);
            uint numberOfTruePiezoelectricBoundaryConditions = 0;

            for (int i = 0; i < piezoelectricDegreesFreedomMaximum; i++)
            {
                if (piezoelectricBondaryConditions[i] == true)
                {
                    numberOfTruePiezoelectricBoundaryConditions += 1;
                }
            }

            bool[] bondaryConditions = await _arrayOperation.MergeVectors(beamBondaryConditions, piezoelectricBondaryConditions).ConfigureAwait(false);
            uint numberOfTrueBoundaryConditions = numberOfTrueBeamBoundaryConditions + numberOfTruePiezoelectricBoundaryConditions;

            // Main matrixes to create input.
            double[,] mass = await _mainMatrix.CalculateMass(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] stiffness = await _mainMatrix.CalculateStiffness(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] piezoelectricElectromechanicalCoupling = await _mainMatrix.CalculatePiezoelectricElectromechanicalCoupling(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] piezoelectricCapacitance = await _mainMatrix.CalculatePiezoelectricCapacitance(beam).ConfigureAwait(false);

            double[,] equivalentMass = await _mainMatrix.CalculateEquivalentMass(mass, degreesOfFreedom, piezoelectricDegreesFreedomMaximum).ConfigureAwait(false);

            double[,] equivalentStiffness = await _mainMatrix.CalculateEquivalentStiffness(stiffness, piezoelectricElectromechanicalCoupling, piezoelectricCapacitance, degreesOfFreedom, piezoelectricDegreesFreedomMaximum).ConfigureAwait(false);

            double[,] damping = await _mainMatrix.CalculateDamping(equivalentMass, equivalentStiffness).ConfigureAwait(false);

            double[] force = beam.Forces;

            double[] electricalCharge = beam.ElectricalCharge;

            double[] equivalentForce = await _arrayOperation.MergeVectors(force, electricalCharge);

            // Creating input.
            NewmarkMethodInput input = new NewmarkMethodInput
            {
                Mass = _auxiliarOperation.ApplyBondaryConditions(equivalentMass, bondaryConditions, numberOfTrueBoundaryConditions),

                Stiffness = _auxiliarOperation.ApplyBondaryConditions(equivalentStiffness, bondaryConditions, numberOfTrueBoundaryConditions),

                Damping = _auxiliarOperation.ApplyBondaryConditions(damping, bondaryConditions, numberOfTrueBoundaryConditions),

                OriginalForce = _auxiliarOperation.ApplyBondaryConditions(equivalentForce, bondaryConditions, numberOfTrueBoundaryConditions),

                NumberOfTrueBoundaryConditions = numberOfTrueBoundaryConditions,

                AngularFrequency = request.Data.InitialAngularFrequency,

                AngularFrequencyStep = request.Data.AngularFrequencyStep,

                FinalAngularFrequency = request.Data.FinalAngularFrequency
            };

            return input;
        }

        public override Task<string> CreateSolutionPath(BeamWithPiezoelectricRequest<TProfile> request, NewmarkMethodInput input, FiniteElementsResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string folderPath = Path.Combine(
                previousPath,
                $"Solutions/FiniteElements/BeamWithPiezoelectric/{request.Data.Profile.GetType().Name}/nEl={request.Data.NumberOfElements}/Piezoelectric {Regex.Replace(request.Data.PiezoelectricPosition, @"\s", "")}");

            string fileName = $"{request.AnalysisType.Trim()}_w={Math.Round(input.AngularFrequency, 2)}_nEl={request.Data.NumberOfElements}.csv";

            string path = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            return Task.FromResult(path);
        }

        public override Task<string> CreateMaxValuesPath(BeamWithPiezoelectricRequest<TProfile> request, NewmarkMethodInput input, FiniteElementsResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string folderPath = Path.Combine(
                previousPath,
                $"Solutions/FiniteElements/BeamWithPiezoelectric/MaxValues");

            string fileName = $"MaxValues_{request.AnalysisType.Trim()}_{Regex.Replace(request.Data.PiezoelectricPosition, @"\s", "")}_{request.Data.Profile.GetType().Name}_w0={Math.Round(request.Data.InitialAngularFrequency, 2)}_wf={Math.Round(request.Data.FinalAngularFrequency, 2)}_nEl={request.Data.NumberOfElements}.csv";

            string path = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            return Task.FromResult(path);
        }
    }
}
