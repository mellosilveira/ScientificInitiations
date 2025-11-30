using MelloSilveiraTools.MechanicsOfMaterials.Models.NumericalMethods;
using MelloSilveiraTools.MechanicsOfMaterials.Models.Profiles;
using MudRunner.Suspension.Core.Models.SuspensionComponents;
using MudRunner.Suspension.DataContracts.CalculateReactions;
using MudRunner.Suspension.DataContracts.RunAnalysis.Dynamic;
using MudRunner.Suspension.DataContracts.RunAnalysis.Static;

namespace MudRunner.Suspension.Core.Mapper;

/// <summary>
/// It is responsible to map an object to another.
/// </summary>
public interface IMappingResolver
{
    /// <summary>
    /// Creates a <see cref="SuspensionSystem"/> based on <see cref="CalculateReactionsRequest"/>.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    SuspensionSystem MapFrom(CalculateReactionsRequest request);

    /// <summary>
    /// Craetes a <see cref="Wishbone{TProfile}"/> based on <see cref="RunStaticAnalysisRequest{TProfile}"/> and <see cref="CalculateReactionsResponseData"/>.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    /// <param name="runStaticAnalysisRequest"></param>
    /// <param name="calculateReactionsResponseData"></param>
    /// <returns></returns>
    SuspensionSystem<TProfile> MapFrom<TProfile>(RunStaticAnalysisRequest<TProfile> runStaticAnalysisRequest, CalculateReactionsResponseData calculateReactionsResponseData)
        where TProfile : Profile;

    /// <summary>
    /// Creates a <see cref="DynamicAnalysisResult"/> based on <see cref="NumericalMethodResult"/>.
    /// </summary>
    /// <param name="numericalMethodResult"></param>
    /// <returns></returns>
    DynamicAnalysisResult MapFrom(NumericalMethodResult numericalMethodResult);
}