using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Calculator.MainMatrixes;
using IcVibracoes.Core.Calculator.NaturalFrequency;
using IcVibracoes.Core.Calculator.Time;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.ExtensionMethods;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.FiniteElement;
using IcVibracoes.DataContracts.FiniteElement.BeamWithPiezoelectric;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IcVibracoes.Core.Calculator.GeometricProperties;

namespace IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithPiezoelectric
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam with piezoelectric.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class CalculateBeamWithPiezoelectricVibration<TProfile> : CalculateVibrationFiniteElement<BeamWithPiezoelectricRequest<TProfile>, TProfile, BeamWithPiezoelectric<TProfile>>, ICalculateBeamWithPiezoelectricVibration<TProfile>
        where TProfile : Profile, new()
    {
        private static readonly string TemplateBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Solutions\\FiniteElement\\BeamWithPiezoelectric");

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
        protected CalculateBeamWithPiezoelectricVibration(
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IProfileValidator<TProfile> profileValidator,
            ITime time,
            INaturalFrequency naturalFrequency,
            IMainMatrix<BeamWithPiezoelectric<TProfile>, TProfile> mainMatrix) : base(profileValidator, time, naturalFrequency, mainMatrix)
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
        public override BeamWithPiezoelectric<TProfile> BuildBeam(BeamWithPiezoelectricRequest<TProfile> request, uint degreesOfFreedom)
        {
            GeometricProperty geometricProperty;

            uint numberOfPiezoelectricPerElements = PiezoelectricPositionFactory.Create(request.PiezoelectricPosition);
            uint[] elementsWithPiezoelectric = request.ElementsWithPiezoelectric ?? this.CreateVectorWithAllElements(request.NumberOfElements);

            // Calculating beam geometric properties.
            if (request.Profile.Area != null && request.Profile.MomentOfInertia != null)
            {
                geometricProperty = GeometricProperty.Create(
                    area: ArrayFactory.CreateVector(request.Profile.Area.Value, request.NumberOfElements), 
                    momentOfInertia: ArrayFactory.CreateVector(request.Profile.MomentOfInertia.Value, request.NumberOfElements));
            }
            else
            {
                geometricProperty = GeometricProperty.Create(
                    area: this._geometricProperty.CalculateArea(request.Profile, request.NumberOfElements),
                    momentOfInertia: this._geometricProperty.CalculateMomentOfInertia(request.Profile, request.NumberOfElements));
            }

            GeometricProperty piezoelectricGeometricProperty;

            // Calculating piezoelectric geometric properties.
            if (request.PiezoelectricProfile.Area != null && request.PiezoelectricProfile.MomentOfInertia != null)
            {
                double area = request.PiezoelectricProfile.Area.Value * numberOfPiezoelectricPerElements;
                double momentOfInertia = request.PiezoelectricProfile.MomentOfInertia.Value * numberOfPiezoelectricPerElements;

                piezoelectricGeometricProperty = GeometricProperty.Create(
                    area: ArrayFactory.CreateVector(area, request.NumberOfElements, elementsWithPiezoelectric),
                    momentOfInertia: ArrayFactory.CreateVector(momentOfInertia, request.NumberOfElements, elementsWithPiezoelectric));
            }
            else
            {
                piezoelectricGeometricProperty = GeometricProperty.Create(
                    area: this._geometricProperty.CalculatePiezoelectricArea(request.PiezoelectricProfile, request.NumberOfElements, elementsWithPiezoelectric, numberOfPiezoelectricPerElements),
                    momentOfInertia: this._geometricProperty.CalculatePiezoelectricMomentOfInertia(request.PiezoelectricProfile, request.Profile, request.NumberOfElements, elementsWithPiezoelectric, numberOfPiezoelectricPerElements));
            }

            var beam = new BeamWithPiezoelectric<TProfile>
            {
                DielectricConstant = request.DielectricConstant,
                DielectricPermissiveness = request.DielectricPermissiveness,
                ElasticityConstant = request.ElasticityConstant,
                ElectricalCharge = new double[request.NumberOfElements + 1],
                ElementsWithPiezoelectric = elementsWithPiezoelectric,
                Fastenings = this._mappingResolver.BuildFastenings(request.Fastenings),
                Forces = this._mappingResolver.BuildForceVector(request.Forces, degreesOfFreedom),
                GeometricProperty = geometricProperty,
                Length = request.Length,
                Material = Material.Create(request.Material),
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
        public override string CreateSolutionPath(BeamWithPiezoelectricRequest<TProfile> request, FiniteElementMethodInput input)
        {
            var fileInfo = new FileInfo(Path.Combine(
                TemplateBasePath,
                $"{request.Profile.GetType().Name}/nEl={request.NumberOfElements}/Piezoelectric {Regex.Replace(request.PiezoelectricPosition, @"\s", "")}/{request.NumericalMethod}",
                $"{request.AnalysisType}_{Regex.Replace(request.PiezoelectricPosition, @"\s", "")}_{request.Profile.GetType().Name}_w={Math.Round(input.AngularFrequency, 2)}_nEl={request.NumberOfElements}.csv"));

            if (!fileInfo.Exists && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            return fileInfo.FullName;
        }

        /// <summary>
        /// This method creates the path to save the file with the maximum values for each angular frequency.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <returns>The path to save the file with the maximum values for each angular frequency.</returns>
        public override string CreateMaxValuesPath(BeamWithPiezoelectricRequest<TProfile> request, FiniteElementMethodInput input)
        {
            var fileInfo = new FileInfo(Path.Combine(
                TemplateBasePath,
                "MaxValues",
                request.NumericalMethod,
                $"MaxValues_{request.AnalysisType}_{Regex.Replace(request.PiezoelectricPosition, @"\s", "")}_{request.Profile.GetType().Name}_w0={Math.Round(request.InitialAngularFrequency, 2)}_wf={Math.Round(request.FinalAngularFrequency, 2)}_nEl={request.NumberOfElements}.csv"));

            if (!fileInfo.Exists && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            return fileInfo.FullName;
        }

        /// <summary>
        /// This method creates a vector with the number of all elements.
        /// </summary>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        private uint[] CreateVectorWithAllElements(uint numberOfElements)
        {
            uint[] vector = new uint[numberOfElements];
            
            for (uint i = 1; i <= numberOfElements; i++)
            {
                vector[i] = i;
            }

            return vector;
        }

        /// <summary>
        /// This method validates the <see cref="BeamWithPiezoelectricRequest{TProfile}"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<FiniteElementResponse> ValidateOperationAsync(BeamWithPiezoelectricRequest<TProfile> request)
        {
            var response = await base.ValidateOperationAsync(request).ConfigureAwait(false);

            if (response.Success == false || request.ElementsWithPiezoelectric == null)
            {
                return response;
            }

            return response
                .AddErrorIf(() => request.PiezoelectricYoungModulus <= 0, $"Piezoelectric Young Modulus: {request.PiezoelectricYoungModulus} must be greater than zero.")
                .AddErrorIf(() => request.PiezoelectricConstant <= 0, $"Piezoelectric Constants: {request.PiezoelectricConstant} must be greater than zero.")
                .AddErrorIf(() => request.DielectricConstant <= 0, $"Dielectric Constants: {request.DielectricConstant} must be greater than zero.")
                .AddErrorIf(() => request.ElasticityConstant <= 0, $"Elasticity Constants: {request.ElasticityConstant} must be greater than zero.")
                .AddErrorIf(() => request.DielectricPermissiveness <= 0, $"Dielectric Permissiveness: {request.DielectricPermissiveness} must be greater than zero.")
                .AddErrorIf(() => request.PiezoelectricSpecificMass <= 0, $"Piezoelectric SpecificMass: {request.PiezoelectricSpecificMass} must be greater than zero.")
                .AddErrorIf(() => Enum.TryParse(typeof(PiezoelectricPosition), Regex.Replace(request.PiezoelectricPosition, @"\s", ""), ignoreCase: true, out _) == false, $"Invalid piezoelectric position: '{request.PiezoelectricPosition}'.")
                .AddErrorIf(() => this._profileValidator.Execute(request.PiezoelectricProfile, response) == false, "Invalid piezoelectric profile.")
                .AddErrorIf(() => request.ElementsWithPiezoelectric.Max() > request.NumberOfElements || request.ElementsWithPiezoelectric.Min() < 1, $"Element with piezoelectric must be greater than one (1) and less than number of elements: {request.NumberOfElements}.")
                .AddErrorIf(request.ElectricalCharges, electricalCharge => (electricalCharge.NodePosition < 0 || electricalCharge.NodePosition > request.NumberOfElements), electricalCharge => $"Electrical Charge NodePosition: {electricalCharge.NodePosition} must be greater than zero and less than number of elements: {request.NumberOfElements}. Electrical Charge index: {request.ElectricalCharges.IndexOf(electricalCharge)}.");
        }
    }
}
