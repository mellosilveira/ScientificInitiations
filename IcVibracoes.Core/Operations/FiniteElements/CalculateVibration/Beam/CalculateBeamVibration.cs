using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.AuxiliarOperations;
using IcVibracoes.Core.Calculator.ArrayOperations;
using IcVibracoes.Core.Calculator.MainMatrixes.Beam;
using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.InputData.FiniteElements;
using IcVibracoes.Core.Mapper;
using IcVibracoes.Core.Mapper.BeamProfiles;
using IcVibracoes.Core.Models.BeamCharacteristics;
using IcVibracoes.Core.Models.Beams;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.Newmark;
using IcVibracoes.Core.NumericalIntegrationMethods.FiniteElement.NewmarkBeta;
using IcVibracoes.Core.Validators.Profiles;
using IcVibracoes.DataContracts.FiniteElements.Beam;
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
        ///// <summary>
        ///// Builds the beam object with the profile that is passed.
        ///// </summary>
        ///// <param name="request"></param>
        ///// <param name="degreesFreedomMaximum"></param>
        ///// <returns></returns>
        //public async override Task<Beam<TProfile>> BuildBeam(BeamRequest<TProfile> request, uint degreesFreedomMaximum)
        //{
        //    if (request == null)
        //    {
        //        return null;
        //    }

        //    GeometricProperty geometricProperty = new GeometricProperty();

        //    if (request.BeamData.Profile.Area != default && request.BeamData.Profile.MomentOfInertia != default)
        //    {
        //        geometricProperty.Area = await this._arrayOperation.CreateVector(request.BeamData.Profile.Area.Value, request.BeamData.NumberOfElements, nameof(request.BeamData.Profile.Area)).ConfigureAwait(false);
        //        geometricProperty.MomentOfInertia = await this._arrayOperation.CreateVector(request.BeamData.Profile.MomentOfInertia.Value, request.BeamData.NumberOfElements, nameof(request.BeamData.Profile.MomentOfInertia)).ConfigureAwait(false);
        //    }
        //    else
        //    {
        //        geometricProperty = await this._profileMapper.Execute(request.BeamData.Profile, request.BeamData.NumberOfElements).ConfigureAwait(false);
        //    }

        //    return new Beam<TProfile>()
        //    {
        //        FirstFastening = FasteningFactory.Create(request.BeamData.FirstFastening),
        //        Forces = await this._mappingResolver.BuildFrom(request.BeamData.Forces, degreesFreedomMaximum).ConfigureAwait(false),
        //        GeometricProperty = geometricProperty,
        //        LastFastening = FasteningFactory.Create(request.BeamData.LastFastening),
        //        Length = request.BeamData.Length,
        //        Material = MaterialFactory.Create(request.BeamData.Material),
        //        NumberOfElements = request.BeamData.NumberOfElements,
        //        Profile = request.BeamData.Profile
        //    };
        //}

        ///// <summary>
        ///// Creates the Newmark method input.
        ///// </summary>
        ///// <param name="beam"></param>
        ///// <param name="newmarkMethodParameter"></param>
        ///// <param name="degreesFreedomMaximum"></param>
        ///// <returns></returns>
        //public async override Task<NewmarkMethodInput> CreateInput(Beam<TProfile> beam, NewmarkMethodParameter newmarkMethodParameter, uint degreesFreedomMaximum)
        //{
        //    bool[] bondaryCondition = await this._mainMatrix.CalculateBondaryCondition(beam.FirstFastening, beam.LastFastening, degreesFreedomMaximum).ConfigureAwait(false);
        //    uint numberOfTrueBoundaryConditions = 0;

        //    for (int i = 0; i < degreesFreedomMaximum; i++)
        //    {
        //        if (bondaryCondition[i] == true)
        //        {
        //            numberOfTrueBoundaryConditions += 1;
        //        }
        //    }

        //    // Main matrixes to create input.
        //    double[,] mass = await this._mainMatrix.CalculateMass(beam, degreesFreedomMaximum).ConfigureAwait(false);

        //    double[,] stiffness = await this._mainMatrix.CalculateStiffness(beam, degreesFreedomMaximum).ConfigureAwait(false);

        //    double[,] damping = await this._mainMatrix.CalculateDamping(mass, stiffness).ConfigureAwait(false);

        //    double[] forces = beam.Forces;

        //    // Creating input.
        //    NewmarkMethodInput input = new NewmarkMethodInput
        //    {
        //        Mass = this._auxiliarOperation.ApplyBondaryConditions(mass, bondaryCondition, numberOfTrueBoundaryConditions),

        //        Stiffness = this._auxiliarOperation.ApplyBondaryConditions(stiffness, bondaryCondition, numberOfTrueBoundaryConditions),

        //        Damping = this._auxiliarOperation.ApplyBondaryConditions(damping, bondaryCondition, numberOfTrueBoundaryConditions),

        //        OriginalForce = this._auxiliarOperation.ApplyBondaryConditions(forces, bondaryCondition, numberOfTrueBoundaryConditions),

        //        NumberOfTrueBoundaryConditions = numberOfTrueBoundaryConditions,

        //        Parameter = new NewmarkMethodParameter
        //        {
        //            AngularFrequencyStep = newmarkMethodParameter.AngularFrequencyStep,
        //            FinalAngularFrequency = newmarkMethodParameter.FinalAngularFrequency,
        //            InitialAngularFrequency = newmarkMethodParameter.InitialAngularFrequency,
        //            InitialTime = newmarkMethodParameter.InitialTime,
        //            NumberOfPeriods = newmarkMethodParameter.NumberOfPeriods,
        //            PeriodDivision = newmarkMethodParameter.PeriodDivision
        //        }
        //    };

        //    return input;
        //}

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="newmarkBetaMethod"></param>
        /// <param name="profileValidator"></param>
        /// <param name="auxiliarOperation"></param>
        public CalculateBeamVibration(
            INewmarkBetaMethod newmarkBetaMethod,
            IProfileValidator<TProfile> profileValidator,
            IAuxiliarOperation auxiliarOperation)
            : base(newmarkBetaMethod, profileValidator, auxiliarOperation)
        { }

        public override Task<Beam<TProfile>> BuildBeam(BeamRequest<TProfile> request)
        {
            throw new System.NotImplementedException();
        }

        public override Task<NewmarkMethodInput> CreateInput(Beam<TProfile> beam)
        {
            throw new System.NotImplementedException();
        }

        public override Task<string> CreatePath(BeamRequest<TProfile> request)
        {
            throw new System.NotImplementedException();
        }
    }
}