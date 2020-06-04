using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.AuxiliarOperations.BoundaryCondition;
using IcVibracoes.Core.AuxiliarOperations.File;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric;
using IcVibracoes.Core.Calculator.NaturalFrequency;
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
    public abstract class CalculateBeamWithPiezoelectricVibration<TProfile, TInput> : CalculateVibration_FiniteElements<BeamWithPiezoelectricRequest<TProfile>, PiezoelectricRequestData<TProfile>, TProfile, BeamWithPiezoelectric<TProfile>, TInput>, ICalculateBeamWithPiezoelectricVibration<TProfile, TInput>
        where TProfile : Profile, new()
        where TInput : NewmarkMethodInput, new()
    {
        private readonly IBoundaryCondition _boundaryCondition;
        private readonly IArrayOperation _arrayOperation;
        private readonly IGeometricProperty<TProfile> _geometricProperty;
        private readonly IMappingResolver _mappingResolver;
        private readonly IBeamWithPiezoelectricMainMatrix<TProfile> _mainMatrix;

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
        public CalculateBeamWithPiezoelectricVibration(
            IBoundaryCondition boundaryCondition,
            IArrayOperation arrayOperation,
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IBeamWithPiezoelectricMainMatrix<TProfile> mainMatrix,
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

        public override async Task<BeamWithPiezoelectric<TProfile>> BuildBeam(BeamWithPiezoelectricRequest<TProfile> request, uint degreesOfFreedom)
        {
            if (request == null)
            {
                return null;
            }

            GeometricProperty geometricProperty = new GeometricProperty();
            GeometricProperty piezoelectricGeometricProperty = new GeometricProperty();

            uint numberOfPiezoelectricPerElements = PiezoelectricPositionFactory.Create(request.Data.PiezoelectricPosition);
            uint[] elementsWithPiezoelectric = request.Data.ElementsWithPiezoelectric ?? this.CreateVectorWithAllElements(request.Data.NumberOfElements);

            // Calculating beam geometric properties.
            if (request.Data.Profile.Area != default && request.Data.Profile.MomentOfInertia != default)
            {
                geometricProperty.Area = await this._arrayOperation.CreateVector(request.Data.Profile.Area.Value, request.Data.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await this._arrayOperation.CreateVector(request.Data.Profile.MomentOfInertia.Value, request.Data.NumberOfElements).ConfigureAwait(false);
            }
            else
            {
                geometricProperty.Area = await this._geometricProperty.CalculateArea(request.Data.Profile, request.Data.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await this._geometricProperty.CalculateMomentOfInertia(request.Data.Profile, request.Data.NumberOfElements).ConfigureAwait(false);
            }

            // Calculating piezoelectric geometric properties.
            if (request.Data.PiezoelectricProfile.Area != default && request.Data.PiezoelectricProfile.MomentOfInertia != default)
            {
                double area = request.Data.PiezoelectricProfile.Area.Value * numberOfPiezoelectricPerElements;
                double momentOfInertia = request.Data.PiezoelectricProfile.MomentOfInertia.Value * numberOfPiezoelectricPerElements;

                piezoelectricGeometricProperty.Area = await this._arrayOperation.CreateVector(area, request.Data.NumberOfElements, elementsWithPiezoelectric).ConfigureAwait(false);
                piezoelectricGeometricProperty.MomentOfInertia = await this._arrayOperation.CreateVector(momentOfInertia, request.Data.NumberOfElements, elementsWithPiezoelectric).ConfigureAwait(false);
            }
            else
            {
                piezoelectricGeometricProperty.Area = await this._geometricProperty.CalculatePiezoelectricArea(request.Data.PiezoelectricProfile, request.Data.NumberOfElements, elementsWithPiezoelectric, numberOfPiezoelectricPerElements).ConfigureAwait(false);
                piezoelectricGeometricProperty.MomentOfInertia = await this._geometricProperty.CalculatePiezoelectricMomentOfInertia(request.Data.PiezoelectricProfile, request.Data.Profile, request.Data.NumberOfElements, elementsWithPiezoelectric, numberOfPiezoelectricPerElements).ConfigureAwait(false);
            }

            return new BeamWithPiezoelectric<TProfile>()
            {
                DielectricConstant = request.Data.DielectricConstant,
                DielectricPermissiveness = request.Data.DielectricPermissiveness,
                ElasticityConstant = request.Data.ElasticityConstant,
                ElectricalCharge = new double[request.Data.NumberOfElements + 1],
                ElementsWithPiezoelectric = elementsWithPiezoelectric,
                Fastenings = await this._mappingResolver.BuildFastenings(request.Data.Fastenings),
                Forces = await this._mappingResolver.BuildForceVector(request.Data.Forces, degreesOfFreedom).ConfigureAwait(false),
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

        public override async Task<TInput> CreateInput(BeamWithPiezoelectricRequest<TProfile> request)
        {
            uint degreesOfFreedom = await base.CalculateDegreesFreedomMaximum(request.Data.NumberOfElements).ConfigureAwait(false);

            BeamWithPiezoelectric<TProfile> beam = await this.BuildBeam(request, degreesOfFreedom).ConfigureAwait(false);

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
            TInput input = new TInput
            {
                Mass = await this._boundaryCondition.Apply(equivalentMass, bondaryConditions, numberOfTrueBoundaryConditions),

                Stiffness = await this._boundaryCondition.Apply(equivalentStiffness, bondaryConditions, numberOfTrueBoundaryConditions),

                Damping = await this._boundaryCondition.Apply(damping, bondaryConditions, numberOfTrueBoundaryConditions),

                OriginalForce = await this._boundaryCondition.Apply(equivalentForce, bondaryConditions, numberOfTrueBoundaryConditions),

                NumberOfTrueBoundaryConditions = numberOfTrueBoundaryConditions,

                AngularFrequency = request.Data.InitialAngularFrequency,

                AngularFrequencyStep = request.Data.AngularFrequencyStep,

                FinalAngularFrequency = request.Data.FinalAngularFrequency
            };

            return input;
        }

        public override Task<string> CreateSolutionPath(BeamWithPiezoelectricRequest<TProfile> request, TInput input, FiniteElementsResponse response)
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

        public override Task<string> CreateMaxValuesPath(BeamWithPiezoelectricRequest<TProfile> request, TInput input, FiniteElementsResponse response)
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

        /// <summary>
        /// This method creates a vector with the number of all elements.
        /// </summary>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        private uint[] CreateVectorWithAllElements(uint numberOfElements)
        {
            uint[] vector = new uint[numberOfElements];

            for(uint i = 0; i < numberOfElements; i++)
            {
                vector[i] = i + 1;
            }

            return vector;
        }
    }
}
