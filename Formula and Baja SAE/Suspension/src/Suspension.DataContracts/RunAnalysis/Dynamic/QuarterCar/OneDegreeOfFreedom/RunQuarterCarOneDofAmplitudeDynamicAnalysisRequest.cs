using System.Collections.Generic;

namespace MudRunner.Suspension.DataContracts.RunAnalysis.Dynamic.QuarterCar.OneDegreeOfFreedom;

/// <summary>
/// It represents the request content of RunQuarterCarOneDofAmplitudeDynamicAnalysis operation.
/// </summary>
public record RunQuarterCarOneDofAmplitudeDynamicAnalysisRequest : RunQuarterCarOneDofGenericDynamicAnalysisRequest<List<double>> { }
