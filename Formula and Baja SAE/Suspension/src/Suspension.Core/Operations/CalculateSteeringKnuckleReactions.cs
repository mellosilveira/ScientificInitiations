using MelloSilveiraTools.Application.Operations;
using MelloSilveiraTools.ExtensionMethods;
using MelloSilveiraTools.Infrastructure.Logger;
using MelloSilveiraTools.MechanicsOfMaterials.Models;
using MudRunner.Suspension.Core.Models.SuspensionComponents.SteeringKnuckle;
using MudRunner.Suspension.DataContracts.CalculateReactions;
using MudRunner.Suspension.DataContracts.CalculateSteeringKnuckleReactions;
using MudRunner.Suspension.DataContracts.Models.Enums;
using System.Threading.Tasks;

namespace MudRunner.Suspension.Core.Operations
{
    /// <summary>
    /// Calculate the reactions to steering knuckle. 
    /// </summary>
    public class CalculateSteeringKnuckleReactions(
        ILogger logger,
        CalculateReactions calculateReactions)
        : OperationBaseWithData<CalculateSteeringKnuckleReactionsRequest, CalculateSteeringKnuckleReactionsResponseData>(logger)
    {
        private readonly CalculateReactions _calculateReactions = calculateReactions;

        public Force CalculateTieRodReactions(Force tieRodReaction, string steeringWheelForce, SuspensionPosition suspensionPosition)
            => suspensionPosition == SuspensionPosition.Rear ? tieRodReaction : tieRodReaction.Sum(Force.Create(steeringWheelForce));

        public double CalculateBearingReaction(CalculateSteeringKnuckleReactionsRequest request)
        {
            var bearing = Bearing.Create(request.BearingType);
            return request.BearingTorque / bearing.EffectiveRadius * bearing.RadialLoadFactor;
        }

        public (Force Reaction1, Force Reaction2) CalculateBrakeCaliperReactions(CalculateSteeringKnuckleReactionsRequest request)
        {
            var brakeCaliperSupport = BrakeCaliperSupportPoint.Create(request.SteeringKnuckle.BrakeCaliperSupportPoint);

            var inertialForce = Vector3D.Create(request.InertialForce);
            var inertialForceCoordinate = Point3D.Create(request.InertialForceCoordinate);

            var vector1 = Vector3D.Create(brakeCaliperSupport.Point2, brakeCaliperSupport.Point1);
            var vector2 = Vector3D.Create(inertialForceCoordinate, brakeCaliperSupport.Point1);

            double[] effort =
            [
                inertialForce.X,
                inertialForce.Y,
                inertialForce.Z,
                -vector2.X * inertialForce.Y + vector2.Y * inertialForce.X,
                -vector2.Y * inertialForce.Z + vector2.Z * inertialForce.Y,
                -vector2.Z * inertialForce.X + vector2.X * inertialForce.Z
            ];

            double[,] displacement = new double[6, 6]
            {
                { 1, 0, 0, 1, 0, 0 },
                { 0, 1, 0, 0, 1, 0 },
                { 0, 0, 1, 0, 0, 1 },
                { 0, 0, 0, -vector1.Y, vector1.X, 0 },
                { 0, 0, 0, 0, -vector1.Z, vector1.Y },
                { 0, 0, 0, vector1.Z, 0, -vector1.X }
            };

            double[] result = displacement.InverseMatrix().Multiply(effort);

            return (new Force(result[0], result[1], result[2]), new Force(result[3], result[4], result[5]));
        }

        protected override async Task<OperationResponseBase<CalculateSteeringKnuckleReactionsResponseData>> ProcessOperationAsync(CalculateSteeringKnuckleReactionsRequest request)
        {
            var response = OperationResponse.CreateSuccessOk<OperationResponseBase<CalculateSteeringKnuckleReactionsResponseData>>();

            CalculateReactionsResponseData suspensionSystemEfforts;
            if (request.CalculateReactionsResponseData != null)
            {
                suspensionSystemEfforts = request.CalculateReactionsResponseData;
            }
            else
            {
                var calculateReactionsResponse = await _calculateReactions.ProcessAsync(request.CalculateReactionsRequest).ConfigureAwait(false);
                if (calculateReactionsResponse.Success == false)
                {
                    response.ErrorMessages.AddRange(calculateReactionsResponse.ErrorMessages);
                    response.SetInternalServerError("Occurred error while calculating reactions on suspension system.");
                    return response;
                }

                suspensionSystemEfforts = calculateReactionsResponse.Data;
            }

            // Step X - Calculates the reactions.
            response.Data.UpperWishboneReaction = suspensionSystemEfforts.UpperWishboneReaction1.Sum(suspensionSystemEfforts.UpperWishboneReaction2);
            response.Data.LowerWishboneReaction = suspensionSystemEfforts.LowerWishboneReaction1.Sum(suspensionSystemEfforts.LowerWishboneReaction2);
            response.Data.TieRodReaction = CalculateTieRodReactions(suspensionSystemEfforts.TieRodReaction, request.SteeringWheelForce, request.SuspensionPosition);
            response.Data.BearingReaction = CalculateBearingReaction(request);
            (response.Data.BrakeCaliperSupportReaction1, response.Data.BrakeCaliperSupportReaction2) = CalculateBrakeCaliperReactions(request);

            return response;
        }

        protected override Task<OperationResponseBase<CalculateSteeringKnuckleReactionsResponseData>> ValidateOperationAsync(CalculateSteeringKnuckleReactionsRequest request) 
            => OperationResponse
                .CreateSuccessOk<OperationResponseBase<CalculateSteeringKnuckleReactionsResponseData>>()
                .AddErrorIf(request.CalculateReactionsRequest == null && request.CalculateReactionsResponseData == null, "The forces applied to the steering knukle or the suspension points must be passed on request")
                .AsTask();
    }
}
