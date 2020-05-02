using IcVibracoes.Calculator.GeometricProperties;
using IcVibracoes.Common.ErrorCodes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.DTO.InputData.FiniteElements;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Models;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.NewmarkBeta;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.FiniteElements;
using IcVibracoes.DataContracts.FiniteElements.Beam;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.Beam
{
    /// <summary>
    /// It's responsible to calculate the vibration in a beam.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class CalculateBeamVibration<TProfile> : CalculateVibration_FiniteElements<BeamRequest<TProfile>, BeamRequestData<TProfile>, TProfile, Beam<TProfile>>, ICalculateBeamVibration<TProfile>
        where TProfile : Profile, new()
    {
        private readonly IAuxiliarOperation _auxiliarOperation;
        private readonly IArrayOperation _arrayOperation;
        private readonly IGeometricProperty<TProfile> _geometricProperty;
        private readonly IMappingResolver _mappingResolver;
        private readonly IBeamMainMatrix<TProfile> _mainMatrix;

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="newmarkBetaMethod"></param>
        /// <param name="profileValidator"></param>
        /// <param name="auxiliarOperation"></param>
        public CalculateBeamVibration(
            INewmarkBetaMethod newmarkBetaMethod,
            IProfileValidator<TProfile> profileValidator,
            IAuxiliarOperation auxiliarOperation,
            IArrayOperation arrayOperation,
            IGeometricProperty<TProfile> geometricProperty,
            IMappingResolver mappingResolver,
            IBeamMainMatrix<TProfile> mainMatrix)
            : base(newmarkBetaMethod, profileValidator, auxiliarOperation)
        {
            this._auxiliarOperation = auxiliarOperation;
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

            return new Beam<TProfile>()
            {
                Fastenings = await this._mappingResolver.BuildFastenings(request.BeamData.Fastenings).ConfigureAwait(false),
                Forces = await this._mappingResolver.BuildForceVector(request.BeamData.Forces, degreesOfFreedom).ConfigureAwait(false),
                GeometricProperty = geometricProperty,
                Length = request.BeamData.Length,
                Material = MaterialFactory.Create(request.BeamData.Material),
                NumberOfElements = request.BeamData.NumberOfElements,
                Profile = request.BeamData.Profile
            };
        }

        public override async Task<NewmarkMethodInput> CreateInput(Beam<TProfile> beam, BeamRequest<TProfile> request, uint degreesOfFreedom)
        {
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
            NewmarkMethodInput input = new NewmarkMethodInput
            {
                Mass = this._auxiliarOperation.ApplyBondaryConditions(mass, bondaryCondition, numberOfTrueBoundaryConditions),

                Stiffness = this._auxiliarOperation.ApplyBondaryConditions(stiffness, bondaryCondition, numberOfTrueBoundaryConditions),

                Damping = this._auxiliarOperation.ApplyBondaryConditions(damping, bondaryCondition, numberOfTrueBoundaryConditions),

                OriginalForce = this._auxiliarOperation.ApplyBondaryConditions(forces, bondaryCondition, numberOfTrueBoundaryConditions),

                NumberOfTrueBoundaryConditions = numberOfTrueBoundaryConditions,

                AngularFrequency = request.BeamData.InitialAngularFrequency,

                AngularFrequencyStep = request.BeamData.AngularFrequencyStep,

                FinalAngularFrequency = request.BeamData.FinalAngularFrequency,

                TimeStep = request.BeamData.TimeStep,

                FinalTime = request.BeamData.FinalTime
            };

            return input;
        }

        public override Task<string> CreatePath(string analysisType, double angularFrequency, uint numberOfElements, FiniteElementsResponse response)
        {
            string previousPath = Path.GetDirectoryName(Directory.GetCurrentDirectory());

            string folderPath = Path.Combine(
                previousPath,
                $"Solutions/FiniteElements/Beam");

            string fileName = $"{analysisType.Trim()}_w={Math.Round(angularFrequency, 2)}_nEl={numberOfElements}.csv";

            string path = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            if (File.Exists(path))
            {
                response.AddError(ErrorCode.OperationError, $"File already exist in path: '{path}'.");
                return Task.FromResult<string>(null);
            }
            else
            {
                return Task.FromResult(path);
            }
        }
    }
}