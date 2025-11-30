using System.Collections.Generic;

namespace MudRunner.Suspension.DataContracts.RunAnalysis.Dynamic.HalfCar.SixDegreeOfFreedom;

/// <summary>
/// It represents the request content of RunHalfCarSixDofAmplitudeDynamicAnalysis operation.
/// </summary>
public record RunHalfCarSixDofAmplitudeDynamicAnalysisRequest : RunHalfCarSixDofGenericDynamicAnalysisRequest<List<double>> { }
