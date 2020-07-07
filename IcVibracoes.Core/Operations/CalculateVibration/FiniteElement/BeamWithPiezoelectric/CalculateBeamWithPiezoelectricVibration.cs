using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.ArrayOperations;
using IcVibracoes.Core.BoundaryCondition;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric;
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
using IcVibracoes.DataContracts.FiniteElement.BeamWithPiezoelectric;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithPiezoelectric
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with piezoelectric.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class CalculateBeamWithPiezoelectricVibration<TProfile> : CalculateVibration_FiniteElement<BeamWithPiezoelectricRequest<TProfile>, TProfile, BeamWithPiezoelectric<TProfile>>, ICalculateBeamWithPiezoelectricVibration<TProfile>
        where TProfile : Profile, new()
    {
        private readonly IBoundaryCondition _boundaryCondition;
        private readonly IArrayOperation _arrayOperation;
        private readonly IGeometricProperty<TProfile> _geometricProperty;
        private readonly IMappingResolver _mappingResolver;
        private readonly IBeamWithPiezoelectricMainMatrix<TProfile> _mainMatrix;
        private readonly IProfileValidator<TProfile> _profileValidator;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="boundaryCondition"></param>
        /// <param name="arrayOperation"></param>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="mainMatrix"></param>
        /// <param name="profileValidator"></param>
        /// <param name="time"></param>
        /// <param name="naturalFrequency"></param>
        public CalculateBeamWithPiezoelectricVibration(
            IBoundaryCondition boundaryCondition,
            IArrayOperation arrayOperation,
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IBeamWithPiezoelectricMainMatrix<TProfile> mainMatrix,
            IProfileValidator<TProfile> profileValidator,
            ITime time,
            INaturalFrequency naturalFrequency)
            : base(profileValidator, time, naturalFrequency)
        {
            this._boundaryCondition = boundaryCondition;
            this._arrayOperation = arrayOperation;
            this._geometricProperty = geometricProperty;
            this._mappingResolver = mappingResolver;
            this._mainMatrix = mainMatrix;
            this._profileValidator = profileValidator;
        }

        /// <summary>
        /// This method creates a new instance of class <see cref="BeamWithPiezoelectric{TProfile}"/>.
        /// This is a step to create the input fot finite element analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <param name="response"></param>
        /// <returns>A new instance of class <see cref="BeamWithPiezoelectric{TProfile}"/>.</returns>
        public override async Task<BeamWithPiezoelectric<TProfile>> BuildBeam(BeamWithPiezoelectricRequest<TProfile> request, uint degreesOfFreedom, FiniteElementResponse response)
        {
            if (request == null)
            {
                return null;
            }

            GeometricProperty geometricProperty = new GeometricProperty();
            GeometricProperty piezoelectricGeometricProperty = new GeometricProperty();

            uint numberOfPiezoelectricPerElements = PiezoelectricPositionFactory.Create(request.PiezoelectricPosition, response);
            uint[] elementsWithPiezoelectric = request.ElementsWithPiezoelectric ?? this.CreateVectorWithAllElements(request.NumberOfElements);

            // Calculating beam geometric properties.
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

            // Calculating piezoelectric geometric properties.
            if (request.PiezoelectricProfile.Area != default && request.PiezoelectricProfile.MomentOfInertia != default)
            {
                double area = request.PiezoelectricProfile.Area.Value * numberOfPiezoelectricPerElements;
                double momentOfInertia = request.PiezoelectricProfile.MomentOfInertia.Value * numberOfPiezoelectricPerElements;

                piezoelectricGeometricProperty.Area = await this._arrayOperation.CreateVector(area, request.NumberOfElements, elementsWithPiezoelectric).ConfigureAwait(false);
                piezoelectricGeometricProperty.MomentOfInertia = await this._arrayOperation.CreateVector(momentOfInertia, request.NumberOfElements, elementsWithPiezoelectric).ConfigureAwait(false);
            }
            else
            {
                piezoelectricGeometricProperty.Area = await this._geometricProperty.CalculatePiezoelectricArea(request.PiezoelectricProfile, request.NumberOfElements, elementsWithPiezoelectric, numberOfPiezoelectricPerElements).ConfigureAwait(false);
                piezoelectricGeometricProperty.MomentOfInertia = await this._geometricProperty.CalculatePiezoelectricMomentOfInertia(request.PiezoelectricProfile, request.Profile, request.NumberOfElements, elementsWithPiezoelectric, numberOfPiezoelectricPerElements).ConfigureAwait(false);
            }

            var beam = new BeamWithPiezoelectric<TProfile>()
            {
                DielectricConstant = request.DielectricConstant,
                DielectricPermissiveness = request.DielectricPermissiveness,
                ElasticityConstant = request.ElasticityConstant,
                ElectricalCharge = new double[request.NumberOfElements + 1],
                ElementsWithPiezoelectric = elementsWithPiezoelectric,
                Fastenings = await this._mappingResolver.BuildFastenings(request.Fastenings, response),
                Forces = await this._mappingResolver.BuildForceVector(request.Forces, degreesOfFreedom).ConfigureAwait(false),
                GeometricProperty = geometricProperty,
                Length = request.Length,
                Material = MaterialFactory.Create(request.Material, response),
                NumberOfElements = request.NumberOfElements,
                NumberOfPiezoelectricPerElements = numberOfPiezoelectricPerElements,
                PiezoelectricConstant = request.PiezoelectricConstant,
                PiezoelectricGeometricProperty = piezoelectricGeometricProperty,
                PiezoelectricProfile = request.PiezoelectricProfile,
                PiezoelectricSpecificMass = request.PiezoelectricSpecificMass,
                PiezoelectricYoungModulus = request.PiezoelectricYoungModulus,
                Profile = request.Profile
            };

            if (response.Success == false)
            {
                response.AddError(OperationErrorCode.InternalServerError, $"Ocurred an error while creating the object {beam.GetType().Name}.", HttpStatusCode.InternalServerError);

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
        public override async Task<FiniteElementMethodInput> CreateInput(BeamWithPiezoelectricRequest<TProfile> request, FiniteElementResponse response)
        {
            uint degreesOfFreedom = await base.CalculateDegreesOfFreedomMaximum(request.NumberOfElements).ConfigureAwait(false);

            BeamWithPiezoelectric<TProfile> beam = await this.BuildBeam(request, degreesOfFreedom, response).ConfigureAwait(false);

            if (beam == null)
            {
                return null;
            }

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

            bool[] piezoelectricBondaryConditions = await this._mainMatrix.CalculatePiezoelectricBondaryCondition(beam.Fastenings, beam.NumberOfElements, beam.ElementsWithPiezoelectric).ConfigureAwait(false);
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
            var numericalMethod = (NumericalMethod)Enum.Parse(typeof(NumericalMethod), request.NumericalMethod, ignoreCase: true);
            FiniteElementMethodInput input = new FiniteElementMethodInput(numericalMethod)
            {
                Mass = await this._boundaryCondition.Apply(equivalentMass, bondaryConditions, numberOfTrueBoundaryConditions),

                Stiffness = await this._boundaryCondition.Apply(equivalentStiffness, bondaryConditions, numberOfTrueBoundaryConditions),

                Damping = await this._boundaryCondition.Apply(damping, bondaryConditions, numberOfTrueBoundaryConditions),

                OriginalForce = await this._boundaryCondition.Apply(equivalentForce, bondaryConditions, numberOfTrueBoundaryConditions),

                NumberOfTrueBoundaryConditions = numberOfTrueBoundaryConditions,

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
        public override Task<string> CreateSolutionPath(BeamWithPiezoelectricRequest<TProfile> request, FiniteElementMethodInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions/FiniteElement/BeamWithPiezoelectric/{request.Profile.GetType().Name}/nEl={request.NumberOfElements}/Piezoelectric {Regex.Replace(request.PiezoelectricPosition, @"\s", "")}/{request.NumericalMethod}");

            string fileName = $"{request.AnalysisType}_{Regex.Replace(request.PiezoelectricPosition, @"\s", "")}_{request.Profile.GetType().Name}_w={Math.Round(input.AngularFrequency, 2)}_nEl={request.NumberOfElements}.csv";

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
        public override Task<string> CreateMaxValuesPath(BeamWithPiezoelectricRequest<TProfile> request, FiniteElementMethodInput input)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string fileUri = Path.Combine(
                previousPath,
                $"Solutions/FiniteElement/BeamWithPiezoelectric/MaxValues/{request.NumericalMethod}");

            string fileName = $"MaxValues_{request.AnalysisType}_{Regex.Replace(request.PiezoelectricPosition, @"\s", "")}_{request.Profile.GetType().Name}_w0={Math.Round(request.InitialAngularFrequency, 2)}_wf={Math.Round(request.FinalAngularFrequency, 2)}_nEl={request.NumberOfElements}.csv";

            string path = Path.Combine(fileUri, fileName);

            Directory.CreateDirectory(fileUri);

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

            for (uint i = 0; i < numberOfElements; i++)
            {
                vector[i] = i + 1;
            }

            return vector;
        }

        /// <summary>
        /// This method validates the <see cref="BeamWithPiezoelectricRequest{TProfile}"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<FiniteElementResponse> ValidateOperation(BeamWithPiezoelectricRequest<TProfile> request)
        {
            FiniteElementResponse response = await base.ValidateOperation(request).ConfigureAwait(false);

            if (request.PiezoelectricYoungModulus <= 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Piezoelectric Young Modulus: {request.PiezoelectricYoungModulus} must be greather than zero.");
            }

            if (request.PiezoelectricConstant <= 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Piezoelectric Constant: {request.PiezoelectricConstant} must be greather than zero.");
            }

            if (request.DielectricConstant <= 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Dielectric Constant: {request.DielectricConstant} must be greather than zero.");
            }

            if (request.ElasticityConstant <= 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Elasticity Constant: {request.ElasticityConstant} must be greather than zero.");
            }

            if (request.DielectricPermissiveness <= 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Dielectric Permissiveness: {request.DielectricPermissiveness} must be greather than zero.");
            }

            if (request.PiezoelectricSpecificMass <= 0)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Piezoelectric SpecificMass: {request.PiezoelectricSpecificMass} must be greather than zero.");
            }

            foreach (var electricalCharge in request.ElectricalCharges)
            {
                if (electricalCharge.NodePosition < 0 || electricalCharge.NodePosition > request.NumberOfElements)
                {
                    response.AddError(OperationErrorCode.RequestValidationError, $"Electrical Charge NodePosition: {electricalCharge.NodePosition} must be greather than zero and less than number of elements: {request.NumberOfElements}. Electrical Charge index: {request.ElectricalCharges.IndexOf(electricalCharge)}.");
                }
            }

            if (Enum.TryParse(typeof(PiezoelectricPosition), request.PiezoelectricPosition, ignoreCase: true, out _) == false)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Invalid piezoelectric position: '{request.PiezoelectricPosition}'.");
            }

            if (request.ElementsWithPiezoelectric.Max() > request.NumberOfElements || request.ElementsWithPiezoelectric.Min() < 1)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Element with piezoelectric must be greather than one (1) and less than number of elements: {request.NumberOfElements}.");
            }

            await this._profileValidator.Execute(request.PiezoelectricProfile, response).ConfigureAwait(false);

            return response;
        }
    }
}
