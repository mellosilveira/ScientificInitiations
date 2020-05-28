using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.InputData.FiniteElements;
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

namespace IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithPiezoelectric
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
            this._auxiliarOperation = auxiliarOperation;
            this._arrayOperation = arrayOperation;
            this._geometricProperty = geometricProperty;
            this._mappingResolver = mappingResolver;
            this._mainMatrix = mainMatrix;
        }

        public override async Task<BeamWithPiezoelectric<TProfile>> BuildBeam(BeamWithPiezoelectricRequest<TProfile> request, uint degreesOfFreedom)
        {
            if (request == null)
            {
                return null;
            }

            GeometricProperty geometricProperty = new GeometricProperty();
            GeometricProperty piezoelectricGeometricProperty = new GeometricProperty();

            uint numberOfPiezoelectricPerElements = PiezoelectricPositionFactory.Create(request.BeamData.PiezoelectricPosition);

            // Calculating beam geometric properties.
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

            // Calculating piezoelectric geometric properties.
            if (request.BeamData.PiezoelectricProfile.Area != default && request.BeamData.PiezoelectricProfile.MomentOfInertia != default)
            {
                double area = request.BeamData.PiezoelectricProfile.Area.Value * numberOfPiezoelectricPerElements;
                double momentOfInertia = request.BeamData.PiezoelectricProfile.MomentOfInertia.Value * numberOfPiezoelectricPerElements;

                piezoelectricGeometricProperty.Area = await this._arrayOperation.CreateVector(area, request.BeamData.NumberOfElements, request.BeamData.ElementsWithPiezoelectric).ConfigureAwait(false);
                piezoelectricGeometricProperty.MomentOfInertia = await this._arrayOperation.CreateVector(momentOfInertia, request.BeamData.NumberOfElements, request.BeamData.ElementsWithPiezoelectric).ConfigureAwait(false);
            }
            else
            {
                piezoelectricGeometricProperty.Area = await this._geometricProperty.CalculatePiezoelectricArea(request.BeamData.PiezoelectricProfile, request.BeamData.NumberOfElements, request.BeamData.ElementsWithPiezoelectric, numberOfPiezoelectricPerElements).ConfigureAwait(false);
                piezoelectricGeometricProperty.MomentOfInertia = await this._geometricProperty.CalculatePiezoelectricMomentOfInertia(request.BeamData.PiezoelectricProfile, request.BeamData.Profile, request.BeamData.NumberOfElements, request.BeamData.ElementsWithPiezoelectric, numberOfPiezoelectricPerElements).ConfigureAwait(false);
            }

            return new BeamWithPiezoelectric<TProfile>()
            {
                DielectricConstant = request.BeamData.DielectricConstant,
                DielectricPermissiveness = request.BeamData.DielectricPermissiveness,
                ElasticityConstant = request.BeamData.ElasticityConstant,
                ElectricalCharge = new double[request.BeamData.NumberOfElements + 1],
                ElementsWithPiezoelectric = request.BeamData.ElementsWithPiezoelectric,
                Fastenings = await this._mappingResolver.BuildFastenings(request.BeamData.Fastenings),
                Forces = await this._mappingResolver.BuildForceVector(request.BeamData.Forces, degreesOfFreedom).ConfigureAwait(false),
                GeometricProperty = geometricProperty,
                Length = request.BeamData.Length,
                Material = MaterialFactory.Create(request.BeamData.Material),
                NumberOfElements = request.BeamData.NumberOfElements,
                NumberOfPiezoelectricPerElements = numberOfPiezoelectricPerElements,
                PiezoelectricConstant = request.BeamData.PiezoelectricConstant,
                PiezoelectricGeometricProperty = piezoelectricGeometricProperty,
                PiezoelectricProfile = request.BeamData.PiezoelectricProfile,
                PiezoelectricSpecificMass = request.BeamData.PiezoelectricSpecificMass,
                PiezoelectricYoungModulus = request.BeamData.PiezoelectricYoungModulus,
                Profile = request.BeamData.Profile
            };
        }

        public override async Task<NewmarkMethodInput> CreateInput(BeamWithPiezoelectric<TProfile> beam, BeamWithPiezoelectricRequest<TProfile> request, uint degreesOfFreedom)
        {
            uint piezoelectricDegreesFreedomMaximum = beam.NumberOfElements + 1;

            bool[] beamBondaryConditions = await this._mainMatrix.CalculateBondaryCondition(beam.Fastenings, degreesOfFreedom).ConfigureAwait(false);
            uint numberOfTrueBeamBoundaryConditions = 0;

            for (int i = 0; i < degreesOfFreedom; i++)
            {
                if (beamBondaryConditions[i] == true)
                {
                    numberOfTrueBeamBoundaryConditions += 1;
                }
            }

            bool[] piezoelectricBondaryConditions = await this._mainMatrix.CalculatePiezoelectricBondaryCondition(beam.NumberOfElements, beam.ElementsWithPiezoelectric).ConfigureAwait(false);
            uint numberOfTruePiezoelectricBoundaryConditions = 0;

            for (int i = 0; i < piezoelectricDegreesFreedomMaximum; i++)
            {
                if (piezoelectricBondaryConditions[i] == true)
                {
                    numberOfTruePiezoelectricBoundaryConditions += 1;
                }
            }

            bool[] bondaryConditions = await this._arrayOperation.MergeVectors(beamBondaryConditions, piezoelectricBondaryConditions).ConfigureAwait(false);
            uint numberOfTrueBoundaryConditions = numberOfTrueBeamBoundaryConditions + numberOfTruePiezoelectricBoundaryConditions;

            // Main matrixes to create input.
            double[,] mass = await this._mainMatrix.CalculateMass(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] stiffness = await this._mainMatrix.CalculateStiffness(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] piezoelectricElectromechanicalCoupling = await this._mainMatrix.CalculatePiezoelectricElectromechanicalCoupling(beam, degreesOfFreedom).ConfigureAwait(false);

            double[,] piezoelectricCapacitance = await this._mainMatrix.CalculatePiezoelectricCapacitance(beam).ConfigureAwait(false);

            double[,] equivalentMass = await this._mainMatrix.CalculateEquivalentMass(mass, degreesOfFreedom, piezoelectricDegreesFreedomMaximum).ConfigureAwait(false);

            double[,] equivalentStiffness = await this._mainMatrix.CalculateEquivalentStiffness(stiffness, piezoelectricElectromechanicalCoupling, piezoelectricCapacitance, degreesOfFreedom, piezoelectricDegreesFreedomMaximum).ConfigureAwait(false);

            double[,] damping = await this._mainMatrix.CalculateDamping(equivalentMass, equivalentStiffness).ConfigureAwait(false);

            double[] force = beam.Forces;

            double[] electricalCharge = beam.ElectricalCharge;

            double[] equivalentForce = await this._arrayOperation.MergeVectors(force, electricalCharge);

            // Creating input.
            NewmarkMethodInput input = new NewmarkMethodInput
            {
                Mass = this._auxiliarOperation.ApplyBondaryConditions(equivalentMass, bondaryConditions, numberOfTrueBoundaryConditions),

                Stiffness = this._auxiliarOperation.ApplyBondaryConditions(equivalentStiffness, bondaryConditions, numberOfTrueBoundaryConditions),

                Damping = this._auxiliarOperation.ApplyBondaryConditions(damping, bondaryConditions, numberOfTrueBoundaryConditions),

                OriginalForce = this._auxiliarOperation.ApplyBondaryConditions(equivalentForce, bondaryConditions, numberOfTrueBoundaryConditions),

                NumberOfTrueBoundaryConditions = numberOfTrueBoundaryConditions,

                AngularFrequency = request.BeamData.InitialAngularFrequency,

                AngularFrequencyStep = request.BeamData.AngularFrequencyStep,

                FinalAngularFrequency = request.BeamData.FinalAngularFrequency
            };

            return input;
        }

        public override Task<string> CreateSolutionPath(BeamWithPiezoelectricRequest<TProfile> request, NewmarkMethodInput input, FiniteElementsResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string folderPath = Path.Combine(
                previousPath,
                $"Solutions/FiniteElements/BeamWithPiezoelectric/{request.BeamData.Profile.GetType().Name}/nEl={request.BeamData.NumberOfElements}/Piezoelectric {Regex.Replace(request.BeamData.PiezoelectricPosition, @"\s", "")}");

            string fileName = $"{request.AnalysisType.Trim()}_w={Math.Round(input.AngularFrequency, 2)}_nEl={request.BeamData.NumberOfElements}.csv";

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

            string fileName = $"MaxValues_{request.AnalysisType.Trim()}_{Regex.Replace(request.BeamData.PiezoelectricPosition, @"\s", "")}_{request.BeamData.Profile.GetType().Name}_w0={Math.Round(request.BeamData.InitialAngularFrequency, 2)}_wf={Math.Round(request.BeamData.FinalAngularFrequency, 2)}_nEl={request.BeamData.NumberOfElements}.csv";

            string path = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            return Task.FromResult(path);
        }
    }
}
