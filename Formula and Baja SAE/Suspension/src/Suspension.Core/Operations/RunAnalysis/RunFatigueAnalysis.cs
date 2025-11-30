using MelloSilveiraTools.Application.Operations;
using MelloSilveiraTools.ExtensionMethods;
using MelloSilveiraTools.Infrastructure.Logger;
using MelloSilveiraTools.MechanicsOfMaterials.Fatigue;
using MelloSilveiraTools.MechanicsOfMaterials.Models;
using MelloSilveiraTools.MechanicsOfMaterials.Models.Fatigue;
using MelloSilveiraTools.MechanicsOfMaterials.Models.Profiles;
using MudRunner.Suspension.DataContracts.RunAnalysis.Fatigue;
using MudRunner.Suspension.DataContracts.RunAnalysis.Static;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MudRunner.Suspension.Core.Operations.RunAnalysis;

/// <summary>
/// Runs the fatigue analysis to suspension system.
/// </summary>
public class RunFatigueAnalysis<TProfile>(
    ILogger logger,
    RunStaticAnalysis<TProfile> runStaticAnalysis,
    IFatigueCalculator fatigueCalculator) : OperationBaseWithData<RunFatigueAnalysisRequest<TProfile>, RunFatigueAnalysisResponseData>(logger)
    where TProfile : Profile
{
    /// <inheritdoc/>
    protected override async Task<OperationResponseBase<RunFatigueAnalysisResponseData>> ProcessOperationAsync(RunFatigueAnalysisRequest<TProfile> request)
    {
        var response = OperationResponse.CreateSuccessOk<OperationResponseBase<RunFatigueAnalysisResponseData>>();

        // Step 1 - Run static analysis for the minimum and maximum force.
        (RunStaticAnalysisResponseData maximumResponseData, RunStaticAnalysisResponseData minimumResponseData) = await RunStaticAnalyses(request, response).ConfigureAwait(false);

        // If some error occurred while running the static analysis, the errors will be added in response and the success will be set as false.
        if (response.Success == false)
            return response;

        // Step 2 - Generate the result and maps to response.
        int numberOfDecimalsToRound = request.NumberOfDecimalsToRound.GetValueOrDefault();
        List<Task> tasks =
        [
            Task.Run(() => response.Data.ShockAbsorberResult = GenerateShockAbsorberResult(maximumResponseData.ShockAbsorberResult, minimumResponseData.ShockAbsorberResult, request.ShouldRoundResults, numberOfDecimalsToRound)),
            // It is necessary to inform the profile aside for it to be used correctly according to the component.
            Task.Run(() => response.Data.LowerWishboneResult = GenerateWishboneResult(request, request.LowerWishbone.Profile, maximumResponseData.LowerWishboneResult, minimumResponseData.LowerWishboneResult)),
            // It is necessary to inform the profile aside for it to be used correctly according to the component.
            Task.Run(() => response.Data.UpperWishboneResult = GenerateWishboneResult(request, request.UpperWishbone.Profile, maximumResponseData.UpperWishboneResult, minimumResponseData.UpperWishboneResult)),
            // It is necessary to inform the profile aside for it to be used correctly according to the component.
            Task.Run(() => response.Data.TieRodResult = GenerateSingleComponentResult(request, request.TieRod.Profile, maximumResponseData.TieRodResult, minimumResponseData.TieRodResult)),
        ];

        await Task.WhenAll(tasks);

        return response;
    }

    /// <inheritdoc/>
    protected override Task<OperationResponseBase<RunFatigueAnalysisResponseData>> ValidateOperationAsync(RunFatigueAnalysisRequest<TProfile> request)
        => OperationResponse.CreateSuccessOk<OperationResponseBase<RunFatigueAnalysisResponseData>>().AsTask();

    /// <summary>
    /// Asynchronously, this method runs the static analysis for the minimum and maximum applied force.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public async Task<(RunStaticAnalysisResponseData RunMaximumStaticAnalysisResponseData, RunStaticAnalysisResponseData RunMinimumStaticAnalysisResponseData)> RunStaticAnalyses(RunFatigueAnalysisRequest<TProfile> request, OperationResponseBase<RunFatigueAnalysisResponseData> response)
    {
        var runMaximumStaticAnalysisResponse = OperationResponse.CreateSuccessOk<OperationResponseBase<RunStaticAnalysisResponseData>>();
        var runMinimumStaticAnalysisResponse = OperationResponse.CreateSuccessOk<OperationResponseBase<RunStaticAnalysisResponseData>>();

        List<Task> tasks =
        [
            Task.Run(async () =>
            {
                RunStaticAnalysisRequest<TProfile> runMaximumStaticAnalysisRequest = BuildRunStaticAnalysisRequest(request, request.MaximumAppliedForce);
                runMaximumStaticAnalysisResponse = await runStaticAnalysis.ProcessAsync(runMaximumStaticAnalysisRequest).ConfigureAwait(false);
            }),
            Task.Run(async () =>
            {
                // If the minimum applied force is equals to zero, all values must be zero and the safaty factor is the biggest.
                if (Vector3D.Create(request.MinimumAppliedForce).IsZero())
                {
                    runMinimumStaticAnalysisResponse.Data.ShockAbsorberResult = new();
                    runMinimumStaticAnalysisResponse.Data.TieRodResult = new() { BucklingSafetyFactor = double.MaxValue, StressSafetyFactor = double.MaxValue };
                    runMinimumStaticAnalysisResponse.Data.LowerWishboneResult = new()
                    {
                        FirstSegment = new() { BucklingSafetyFactor = double.MaxValue, StressSafetyFactor = double.MaxValue },
                        SecondSegment = new() { BucklingSafetyFactor = double.MaxValue, StressSafetyFactor = double.MaxValue }
                    };
                    runMinimumStaticAnalysisResponse.Data.UpperWishboneResult = new()
                    {
                        FirstSegment = new() { BucklingSafetyFactor = double.MaxValue, StressSafetyFactor = double.MaxValue },
                        SecondSegment = new() { BucklingSafetyFactor = double.MaxValue, StressSafetyFactor = double.MaxValue }
                    };
                }
                else
                {
                    RunStaticAnalysisRequest<TProfile> runMinimumStaticAnalysisRequest = BuildRunStaticAnalysisRequest(request, request.MinimumAppliedForce);
                    runMinimumStaticAnalysisResponse = await runStaticAnalysis.ProcessAsync(runMinimumStaticAnalysisRequest).ConfigureAwait(false);
                }
            }),
        ];
        await Task.WhenAll(tasks);

        if (runMaximumStaticAnalysisResponse.Success == false)
        {
            response.SetInternalServerError("Occurred error durring the static analysis to suspension system considering the maximum force.");
            response.ErrorMessages.AddRange(runMaximumStaticAnalysisResponse.ErrorMessages);
        }

        if (runMinimumStaticAnalysisResponse.Success == false)
        {
            response.SetInternalServerError("Occurred error durring the static analysis to suspension system considering the minimum force.");
            response.ErrorMessages.AddRange(runMinimumStaticAnalysisResponse.ErrorMessages);
        }

        return (runMaximumStaticAnalysisResponse.Data, runMinimumStaticAnalysisResponse.Data);
    }

    /// <summary>
    /// Builds the <see cref="RunStaticAnalysisRequest{TProfile}"/> based on <see cref="RunFatigueAnalysisRequest{TProfile}"/>.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="appliedForce"></param>
    /// <returns></returns>
    public RunStaticAnalysisRequest<TProfile> BuildRunStaticAnalysisRequest(RunFatigueAnalysisRequest<TProfile> request, string appliedForce) => new()
    {
        AppliedForce = appliedForce,
        Material = request.Material,
        ShouldRoundResults = request.ShouldRoundResults,
        NumberOfDecimalsToRound = request.NumberOfDecimalsToRound,
        Origin = request.Origin,
        ShockAbsorber = request.ShockAbsorber,
        LowerWishbone = request.LowerWishbone,
        UpperWishbone = request.UpperWishbone,
        TieRod = request.TieRod
    };

    /// <summary>
    /// Generates the result for shock absorber.
    /// </summary>
    /// <param name="shockAbsorberMaximumResult"></param>
    /// <param name="shockAbsorberMinimumResult"></param>
    /// <param name="shouldRoundResults"></param>
    /// <param name="decimals"></param>
    /// <returns></returns>
    public ShockAbsorberFatigueAnalysisResult GenerateShockAbsorberResult(Force shockAbsorberMaximumResult, Force shockAbsorberMinimumResult, bool shouldRoundResults, int decimals)
    {
        // Force amplitude = Absolut((Maximum force - Minimum Force) / 2)
        Force forceAmplitude = shockAbsorberMaximumResult.Subtract(shockAbsorberMinimumResult).Divide(2).Abs();
        // Mean force = (Maximum force + Minimum Force) / 2
        Force meanForce = shockAbsorberMaximumResult.Sum(shockAbsorberMinimumResult).Divide(2);

        return new ShockAbsorberFatigueAnalysisResult
        {
            ForceAmplitude = shouldRoundResults ? forceAmplitude.Round(decimals) : forceAmplitude,
            MeanForce = shouldRoundResults ? meanForce.Round(decimals) : meanForce
        };
    }

    /// <summary>
    /// Generates the result for wishbone.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="profile"></param>
    /// <param name="wishboneMaximumResult"></param>
    /// <param name="wishboneMinimumResult"></param>
    /// <returns></returns>
    public WishboneFatigueAnalysisResult GenerateWishboneResult(RunFatigueAnalysisRequest<TProfile> request, TProfile profile, WishboneStaticAnalysisResult wishboneMaximumResult, WishboneStaticAnalysisResult wishboneMinimumResult) => new()
    {
        FirstSegment = GenerateSingleComponentResult(request, profile, wishboneMaximumResult.FirstSegment, wishboneMinimumResult.FirstSegment),
        SecondSegment = GenerateSingleComponentResult(request, profile, wishboneMaximumResult.SecondSegment, wishboneMinimumResult.SecondSegment)
    };

    /// <summary>
    /// Generates the result for suspension single component.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="profile"></param>
    /// <param name="singleComponentMaximumResult"></param>
    /// <param name="singleComponentMinimumResult"></param>
    /// <returns></returns>
    public SingleComponentFatigueAnalysisResult GenerateSingleComponentResult(RunFatigueAnalysisRequest<TProfile> request, TProfile profile,
        SingleComponentStaticAnalysisResult singleComponentMaximumResult, SingleComponentStaticAnalysisResult singleComponentMinimumResult)
    {
        // Step i - Builds the input for fatigue analysis.
        FatigueInput fatigueInput = new()
        {
            FatigueLimit = request.FatigueLimit,
            FatigueLimitFraction = request.FatigueLimitFraction,
            IsRotativeSection = request.IsRotativeSection,
            Reliability = request.Reliability,
            SurfaceFinish = request.SurfaceFinish,
            TensileStress = Material.Create(request.Material).TensileStress,
            Profile = profile,
            MinimumAppliedStress = request.FatigueStressConcentrationFactor * singleComponentMinimumResult.EquivalentStress,
            MaximumAppliedStress = request.FatigueStressConcentrationFactor * singleComponentMaximumResult.EquivalentStress,
            LoadingType = LoadingType.Axial,
        };

        // Step ii - Calculates the results for fatigue analysis.
        FatigueResult fatigueResult = fatigueCalculator.CalculateFatigueResult(fatigueInput);

        // Step iii - Maps the result for SingleComponentFatigueAnalysisResult.
        SingleComponentFatigueAnalysisResult result = new()
        {
            AppliedForce = GetAbsoluteMaximum(singleComponentMaximumResult.AppliedForce, singleComponentMinimumResult.AppliedForce),
            BucklingSafetyFactor = GetAbsoluteMinimum(singleComponentMaximumResult.BucklingSafetyFactor, singleComponentMinimumResult.BucklingSafetyFactor),
            EquivalentStress = GetAbsoluteMaximum(singleComponentMaximumResult.EquivalentStress, singleComponentMinimumResult.EquivalentStress),
            StressSafetyFactor = GetAbsoluteMinimum(singleComponentMaximumResult.StressSafetyFactor, singleComponentMinimumResult.StressSafetyFactor),
            FatigueEquivalentStress = fatigueResult.EquivalentStress,
            FatigueNumberOfCycles = fatigueResult.NumberOfCycles,
            FatigueSafetyFactor = fatigueResult.SafetyFactor
        };

        // Step iv - Rounds the results if it was requested. 
        if (request.ShouldRoundResults)
        {
            result.AppliedForce = Math.Round(result.AppliedForce, request.NumberOfDecimalsToRound.GetValueOrDefault());
            result.BucklingSafetyFactor = Math.Round(result.BucklingSafetyFactor, request.NumberOfDecimalsToRound.GetValueOrDefault());
            result.EquivalentStress = Math.Round(result.EquivalentStress, request.NumberOfDecimalsToRound.GetValueOrDefault());
            result.StressSafetyFactor = Math.Round(result.StressSafetyFactor, request.NumberOfDecimalsToRound.GetValueOrDefault());
            result.FatigueEquivalentStress = Math.Round(result.FatigueEquivalentStress, request.NumberOfDecimalsToRound.GetValueOrDefault());
            result.FatigueNumberOfCycles = Math.Round(result.FatigueNumberOfCycles, request.NumberOfDecimalsToRound.GetValueOrDefault());
            result.FatigueSafetyFactor = Math.Round(result.FatigueSafetyFactor, request.NumberOfDecimalsToRound.GetValueOrDefault());
        }

        return result;
    }

    /// <summary>
    /// Gets the absolute minimum between two values.
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    private double GetAbsoluteMinimum(double value1, double value2) => Math.Abs(value1) > Math.Abs(value2) ? value2 : value1;

    /// <summary>
    /// Gets the absolute maximum between two values.
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    private double GetAbsoluteMaximum(double value1, double value2) => Math.Abs(value1) > Math.Abs(value2) ? value1 : value2;
}
