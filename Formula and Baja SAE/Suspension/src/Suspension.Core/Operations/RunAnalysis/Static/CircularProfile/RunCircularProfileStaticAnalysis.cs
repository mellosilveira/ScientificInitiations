﻿using MudRunner.Commons.Core.ConstitutiveEquations.MechanicsOfMaterials;
using MudRunner.Commons.Core.GeometricProperties.CircularProfile;
using MudRunner.Commons.DataContracts.Operation;
using MudRunner.Suspension.Core.Mapper;
using MudRunner.Suspension.Core.Operations.CalculateReactions;
using MudRunner.Suspension.DataContracts.RunAnalysis.Static;
using System.Threading.Tasks;
using DataContract = MudRunner.Commons.DataContracts.Models.Profiles;

namespace MudRunner.Suspension.Core.Operations.RunAnalysis.Static.CircularProfile
{
    /// <summary>
    /// It is responsible to run the static analysis to suspension system considering circular profile.
    /// </summary>
    public class RunCircularProfileStaticAnalysis : RunStaticAnalysis<DataContract.CircularProfile>, IRunCircularProfileStaticAnalysis
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="calculateReactions"></param>
        /// <param name="mechanicsOfMaterials"></param>
        /// <param name="geometricProperty"></param>
        /// <param name="mappingResolver"></param>
        public RunCircularProfileStaticAnalysis(
            ICalculateReactions calculateReactions,
            IMechanicsOfMaterials mechanicsOfMaterials,
            ICircularProfileGeometricProperty geometricProperty,
            IMappingResolver mappingResolver)
            : base(calculateReactions, mechanicsOfMaterials, geometricProperty, mappingResolver)
        { }

        /// <summary>
        /// Asynchronously, this method validates the <see cref="RunStaticAnalysisRequest{CircularProfile}"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Task<OperationResponse<RunStaticAnalysisResponseData>> ValidateOperationAsync(RunStaticAnalysisRequest<DataContract.CircularProfile> request)
        {
            OperationResponse<RunStaticAnalysisResponseData> response = new();
            response.SetSuccessOk();

            return Task.FromResult(response);
        }
    }
}
