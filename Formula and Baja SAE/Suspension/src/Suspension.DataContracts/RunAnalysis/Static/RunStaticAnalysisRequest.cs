using MelloSilveiraTools.Application.Operations;
using MelloSilveiraTools.MechanicsOfMaterials.Models.Enums;
using MelloSilveiraTools.MechanicsOfMaterials.Models.Profiles;
using MudRunner.Suspension.DataContracts.Models.SuspensionComponents;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MudRunner.Suspension.DataContracts.RunAnalysis.Static;

/// <summary>
/// It represents the request content of RunStaticAnalysis operation.
/// </summary>
/// <typeparam name="TProfile"></typeparam>
public record RunStaticAnalysisRequest<TProfile> : OperationRequestBase
    where TProfile : Profile
{
    /// <summary>
    /// True, if result should be rounded. False, otherwise.
    /// </summary>
    public bool ShouldRoundResults { get; set; }

    /// <summary>
    /// The number of decimals that should be rounded in results.
    /// </summary>
    public int? NumberOfDecimalsToRound { get; set; }

    /// <summary>
    /// The material.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public MaterialType Material { get; set; }

    /// <summary>
    /// The origin considered to analysis.
    /// </summary>
    /// <example>x,y,z</example>
    public string Origin { get; set; }

    /// <summary>
    /// The applied force.
    /// Unit: N (Newton).
    /// </summary>
    /// <example>x,y,z</example>
    public string AppliedForce { get; set; }

    /// <summary>
    /// The shock absorber.
    /// </summary>
    public ShockAbsorber ShockAbsorber { get; set; }

    /// <summary>
    /// The suspension upper wishbone.
    /// </summary>
    public Wishbone<TProfile> UpperWishbone { get; set; }

    /// <summary>
    /// The suspension lower wishbone.
    /// </summary>
    public Wishbone<TProfile> LowerWishbone { get; set; }

    /// <summary>
    /// The tie rod.
    /// </summary>
    public TieRod<TProfile> TieRod { get; set; }
}
