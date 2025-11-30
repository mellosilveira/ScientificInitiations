using MelloSilveiraTools.Application.Operations;
using MelloSilveiraTools.ExtensionMethods;
using MelloSilveiraTools.Infrastructure.Logger;
using MelloSilveiraTools.MechanicsOfMaterials.ConstitutiveEquations;
using MelloSilveiraTools.MechanicsOfMaterials.GeometricProperties;
using MelloSilveiraTools.MechanicsOfMaterials.Models;
using MelloSilveiraTools.MechanicsOfMaterials.Models.Profiles;
using MudRunner.Suspension.Core.Mapper;
using MudRunner.Suspension.Core.Models.SuspensionComponents;
using MudRunner.Suspension.DataContracts.CalculateReactions;
using MudRunner.Suspension.DataContracts.Models.SuspensionComponents;
using MudRunner.Suspension.DataContracts.RunAnalysis.Static;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MudRunner.Suspension.Core.Operations.RunAnalysis;

/// <summary>
/// Runs the static analysis to suspension system.
/// </summary>
public class RunStaticAnalysis<TProfile>(
    ILogger logger,
    CalculateReactions calculateReactions,
    IConstitutiveEquationsCalculator constitutiveEquationsCalculator,
    IGeometricPropertyCalculator<TProfile> geometricProperty,
    IMappingResolver mappingResolver)
    : OperationBaseWithData<RunStaticAnalysisRequest<TProfile>, RunStaticAnalysisResponseData>(logger)
    where TProfile : Profile
{
    /// <inheritdoc/>
    protected override async Task<OperationResponseBase<RunStaticAnalysisResponseData>> ProcessOperationAsync(RunStaticAnalysisRequest<TProfile> request)
    {
        var response = OperationResponse.CreateSuccessOk<OperationResponseBase<RunStaticAnalysisResponseData>>();

        // Step 1 - Calculates the reactions at suspension system for the applied force.
        OperationResponseBase<CalculateReactionsResponseData> calculateReactionsResponse = await calculateReactions.ProcessAsync(BuildCalculateReactionsRequest(request)).ConfigureAwait(false);
        if (!calculateReactionsResponse.Success)
        {
            response.SetInternalServerError("Occurred error while calculating the reactions to suspension system.");
            response.ErrorMessages.AddRange(calculateReactionsResponse.ErrorMessages);

            return response;
        }

        // Step 2 - Builds the suspension system based on CalculateReaction operation response and request.
        // Here is built the main information to be used at analysis.
        SuspensionSystem<TProfile> suspensionSystem = mappingResolver.MapFrom(request, calculateReactionsResponse.Data);

        // Step 3 - Generate the result and maps to response.
        int numberOfDecimalsToRound = request.NumberOfDecimalsToRound.GetValueOrDefault();
        List<Task> tasks =
        [
            Task.Run(() => response.Data.ShockAbsorberResult = GenerateShockAbsorberResult(calculateReactionsResponse.Data.ShockAbsorberReaction, request.ShouldRoundResults, numberOfDecimalsToRound)),
            Task.Run(() => response.Data.LowerWishboneResult = GenerateWishboneResult(suspensionSystem.LowerWishbone, request.ShouldRoundResults, numberOfDecimalsToRound)),
            Task.Run(() => response.Data.UpperWishboneResult = GenerateWishboneResult(suspensionSystem.UpperWishbone, request.ShouldRoundResults, numberOfDecimalsToRound)),
            Task.Run(() => response.Data.TieRodResult = GenerateSingleComponentResult(suspensionSystem.TieRod, request.ShouldRoundResults, numberOfDecimalsToRound)),
        ];

        await Task.WhenAll(tasks);

        return response;
    }

    /// <inheritdoc/>
    protected override Task<OperationResponseBase<RunStaticAnalysisResponseData>> ValidateOperationAsync(RunStaticAnalysisRequest<TProfile> request)
        => OperationResponse.CreateSuccessOk<OperationResponseBase<RunStaticAnalysisResponseData>>().AsTask();

    /// <summary>
    /// Builds <see cref="CalculateReactionsRequest"/> based on <see cref="RunStaticAnalysisRequest{TProfile}"/>.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public CalculateReactionsRequest BuildCalculateReactionsRequest(RunStaticAnalysisRequest<TProfile> request) => new()
    {
        ShouldRoundResults = false,
        AppliedForce = request.AppliedForce,
        Origin = request.Origin,
        ShockAbsorber = ShockAbsorberPoint.Create(request.ShockAbsorber),
        LowerWishbone = WishbonePoint.Create(request.LowerWishbone),
        UpperWishbone = WishbonePoint.Create(request.UpperWishbone),
        TieRod = TieRodPoint.Create(request.TieRod)
    };

    /// <summary>
    /// Generates the analysis result to shock absorber.
    /// </summary>
    /// <param name="shockAbsorberReaction"></param>
    /// <param name="shouldRoundResults"></param>
    /// <param name="numberOfDecimalsToRound"></param>
    /// <returns></returns>
    public Force GenerateShockAbsorberResult(Force shockAbsorberReaction, bool shouldRoundResults, int numberOfDecimalsToRound)
        => shouldRoundResults ? shockAbsorberReaction.Round(numberOfDecimalsToRound) : shockAbsorberReaction;

    /// <summary>
    /// Generates the analysis result to suspension A-arm.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="shouldRoundResults"></param>
    /// <param name="decimals"></param>
    /// <returns></returns>
    public WishboneStaticAnalysisResult GenerateWishboneResult(Models.SuspensionComponents.Wishbone<TProfile> component, bool shouldRoundResults, int decimals = 0)
    {
        if (component == null)
            throw new ArgumentNullException(nameof(component), "The object suspension A-arm cannot be null to calculate the results.");

        return new()
        {
            FirstSegment = GenerateSingleComponentResult(new SingleComponent<TProfile>
            {
                AppliedForce = component.AppliedForce1,
                PivotPoint = component.FrontPivot,
                FasteningPoint = component.OuterBallJoint,
                Material = component.Material,
                Profile = component.Profile,
            }, shouldRoundResults, decimals),
            SecondSegment = GenerateSingleComponentResult(new SingleComponent<TProfile>
            {
                AppliedForce = component.AppliedForce2,
                PivotPoint = component.RearPivot,
                FasteningPoint = component.OuterBallJoint,
                Material = component.Material,
                Profile = component.Profile,
            }, shouldRoundResults, decimals)
        };
    }

    /// <summary>
    /// Generates the analysis result to single component.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="shouldRoundResults"></param>
    /// <param name="decimals"></param>
    /// <returns></returns>
    public SingleComponentStaticAnalysisResult GenerateSingleComponentResult(SingleComponent<TProfile> component, bool shouldRoundResults, int decimals)
    {
        if (component == null)
            throw new ArgumentNullException(nameof(component), "The object tie rod cannot be null to calculate the results.");

        // Step i - Calculates the geometric properties.
        double area = geometricProperty.CalculateArea(component.Profile);
        double momentOfInertia = geometricProperty.CalculateMomentOfInertia(component.Profile);

        // Step ii - Calculates the equivalent stress.
        // For that case, just is considered the normal stress because the applied force is at same axis of geometry.
        double equivalentStress = constitutiveEquationsCalculator.CalculateNormalStress(component.AppliedForce, area);

        // Step iii - Calculates the critical buckling force.
        double criticalBucklingForce = constitutiveEquationsCalculator.CalculateCriticalBucklingForce(component.Material.YoungModulus, momentOfInertia, component.Length);

        // Step iv - Builds the analysis result.
        SingleComponentStaticAnalysisResult result = new()
        {
            AppliedForce = component.AppliedForce,
            BucklingSafetyFactor = Math.Abs(criticalBucklingForce / component.AppliedForce),
            EquivalentStress = equivalentStress,
            StressSafetyFactor = Math.Abs(component.Material.YieldStrength / equivalentStress)
        };

        // Step v - Rounds the results if it was requested. 
        if (shouldRoundResults)
        {
            result.AppliedForce = Math.Round(result.AppliedForce, decimals);
            result.BucklingSafetyFactor = Math.Round(result.BucklingSafetyFactor, decimals);
            result.EquivalentStress = Math.Round(result.EquivalentStress, decimals);
            result.StressSafetyFactor = Math.Round(result.StressSafetyFactor, decimals);
        }

        return result;
    }
}
