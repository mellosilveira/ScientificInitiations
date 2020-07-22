using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes;
using IcVibracoes.Core.Calculator.MainMatrixes.BeamWithPiezoelectric;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElement;
using IcVibracoes.Core.ExtensionMethods;
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
        private readonly IGeometricProperty<TProfile> _geometricProperty;
        private readonly IMappingResolver _mappingResolver;
        private readonly IProfileValidator<TProfile> _profileValidator;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        /// <param name="profileValidator"></param>
        /// <param name="time"></param>
        /// <param name="naturalFrequency"></param>
        /// <param name="mainMatrix"></param>
        public CalculateBeamWithPiezoelectricVibration(
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IProfileValidator<TProfile> profileValidator,
            ITime time,
            INaturalFrequency naturalFrequency,
            IMainMatrix<BeamWithPiezoelectric<TProfile>, TProfile> mainMatrix)
            : base(profileValidator, time, naturalFrequency, mainMatrix)
        {
            this._geometricProperty = geometricProperty;
            this._mappingResolver = mappingResolver;
            this._profileValidator = profileValidator;
        }

        /// <summary>
        /// This method creates a new instance of class <see cref="BeamWithPiezoelectric{TProfile}"/>.
        /// This is a step to create the input fot finite element analysis.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="degreesOfFreedom"></param>
        /// <returns>A new instance of class <see cref="BeamWithPiezoelectric{TProfile}"/>.</returns>
        public override async Task<BeamWithPiezoelectric<TProfile>> BuildBeam(BeamWithPiezoelectricRequest<TProfile> request, uint degreesOfFreedom)
        {
            GeometricProperty geometricProperty = new GeometricProperty();
            GeometricProperty piezoelectricGeometricProperty = new GeometricProperty();

            uint numberOfPiezoelectricPerElements = PiezoelectricPositionFactory.Create(request.PiezoelectricPosition);
            uint[] elementsWithPiezoelectric = request.ElementsWithPiezoelectric ?? this.CreateVectorWithAllElements(request.NumberOfElements);

            // Calculating beam geometric properties.
            if (request.Profile.Area != null && request.Profile.MomentOfInertia != null)
            {
                geometricProperty.Area = await ArrayFactory.CreateVectorAsync(request.Profile.Area.Value, request.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await ArrayFactory.CreateVectorAsync(request.Profile.MomentOfInertia.Value, request.NumberOfElements).ConfigureAwait(false);
            }
            else
            {
                geometricProperty.Area = await this._geometricProperty.CalculateArea(request.Profile, request.NumberOfElements).ConfigureAwait(false);
                geometricProperty.MomentOfInertia = await this._geometricProperty.CalculateMomentOfInertia(request.Profile, request.NumberOfElements).ConfigureAwait(false);
            }

            // Calculating piezoelectric geometric properties.
            if (request.PiezoelectricProfile.Area != null && request.PiezoelectricProfile.MomentOfInertia != null)
            {
                double area = request.PiezoelectricProfile.Area.Value * numberOfPiezoelectricPerElements;
                double momentOfInertia = request.PiezoelectricProfile.MomentOfInertia.Value * numberOfPiezoelectricPerElements;

                piezoelectricGeometricProperty.Area = await ArrayFactory.CreateVectorAsync(area, request.NumberOfElements, elementsWithPiezoelectric).ConfigureAwait(false);
                piezoelectricGeometricProperty.MomentOfInertia = await ArrayFactory.CreateVectorAsync(momentOfInertia, request.NumberOfElements, elementsWithPiezoelectric).ConfigureAwait(false);
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
                Fastenings = await this._mappingResolver.BuildFastenings(request.Fastenings).ConfigureAwait(false),
                Forces = await this._mappingResolver.BuildForceVector(request.Forces, degreesOfFreedom).ConfigureAwait(false),
                GeometricProperty = geometricProperty,
                Length = request.Length,
                Material = MaterialFactory.Create(request.Material),
                NumberOfElements = request.NumberOfElements,
                NumberOfPiezoelectricPerElements = numberOfPiezoelectricPerElements,
                PiezoelectricConstant = request.PiezoelectricConstant,
                PiezoelectricDegreesOfFreedom = request.NumberOfElements + 1,
                PiezoelectricGeometricProperty = piezoelectricGeometricProperty,
                PiezoelectricProfile = request.PiezoelectricProfile,
                PiezoelectricSpecificMass = request.PiezoelectricSpecificMass,
                PiezoelectricYoungModulus = request.PiezoelectricYoungModulus,
                Profile = request.Profile
            };

            return beam;
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

            if (response.Success == false)
            {
                return response;
            }

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

            if (Enum.TryParse(typeof(PiezoelectricPosition), Regex.Replace(request.PiezoelectricPosition, @"\s", ""), ignoreCase: true, out _) == false)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Invalid piezoelectric position: '{request.PiezoelectricPosition}'.");
            }

            if (await this._profileValidator.Execute(request.PiezoelectricProfile, response).ConfigureAwait(false) == false)
            {
                response.AddError(OperationErrorCode.RequestValidationError, "Invalid piezoelectric profile.");
            }

            if (request.ElementsWithPiezoelectric == null)
            {
                return response;
            }

            if (request.ElementsWithPiezoelectric.Max() > request.NumberOfElements || request.ElementsWithPiezoelectric.Min() < 1)
            {
                response.AddError(OperationErrorCode.RequestValidationError, $"Element with piezoelectric must be greather than one (1) and less than number of elements: {request.NumberOfElements}.");
            }

            return response;
        }
    }
}
