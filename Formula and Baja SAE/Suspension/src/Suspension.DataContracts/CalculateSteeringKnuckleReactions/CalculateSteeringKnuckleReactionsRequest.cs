﻿using MudRunner.Commons.DataContracts.Operation;
using MudRunner.Suspension.DataContracts.CalculateReactions;
using MudRunner.Suspension.DataContracts.Models.Enums;
using MudRunner.Suspension.DataContracts.Models.SuspensionComponents.SteeringKnuckle;

namespace MudRunner.Suspension.DataContracts.CalculateSteeringKnuckleReactions
{
    /// <summary>
    /// It represents the request content to CalculateSteeringKnuckleReactions operation.
    /// </summary>
    public class CalculateSteeringKnuckleReactionsRequest : OperationRequestBase
    {
        /// <summary>
        /// The request content to CalculateReactions operation. 
        /// </summary>
        public CalculateReactionsRequest CalculateReactionsRequest { get; set; }

        /// <summary>
        /// The 'data' content of operation CalculateReactions response.
        /// </summary>
        public CalculateReactionsResponseData CalculateReactionsResponseData { get; set; }

        /// <summary>
        /// The inertial force is the force due to brake caliper inertia when coming into contact with the brake disc. 
        /// </summary>
        public string InertialForce { get; set; }

        /// <summary>
        /// The coordinate of inertial force.
        /// </summary>
        public string InertialForceCoordinate { get; set; }

        /// <summary>
        /// It represents the force that the driver uses when moving the steering wheel. 
        /// </summary>
        public string SteeringWheelForce { get; set; }

        /// <summary>
        /// The torque applied at bearing.
        /// </summary>
        public double BearingTorque { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BearingType BearingType { get; set; }

        /// <summary>
        /// The steering knuckle position.
        /// It can be <see cref="SuspensionPosition.Rear"/> or <see cref="SuspensionPosition.Front"/>. 
        /// </summary>
        public SuspensionPosition SuspensionPosition { get; set; }

        /// <summary>
        /// The steering knuckle points. 
        /// </summary>
        public SteeringKnucklePoint SteeringKnuckle { get; set; }
    }
}
