﻿using MudRunner.Commons.DataContracts.Models;

namespace MudRunner.Suspension.DataContracts.CalculateSteeringKnuckleReactions
{
    /// <summary>
    /// It represents the 'data' content of operation CalculateSteeringKnuckleReactions response.
    /// </summary>
    public class CalculateSteeringKnuckleReactionsResponseData
    {
        /// <summary>
        /// The reaction from suspension upper wishbone.
        /// Unit: N (Newton).
        /// </summary>
        public Force UpperWishboneReaction { get; set; }

        /// <summary>
        /// The reaction from suspension lower wishbone.
        /// Unit: N (Newton).
        /// </summary>
        public Force LowerWishboneReaction { get; set; }

        /// <summary>
        /// The reaction from tie rod.
        /// Unit: N (Newton).
        /// </summary>
        public Force TieRodReaction { get; set; }

        /// <summary>
        /// The reaction from brake caliper support.
        /// This component has two reactions.
        /// </summary>
        public Force BrakeCaliperSupportReaction1 { get; set; }

        /// <summary>
        /// The reaction from brake caliper support.
        /// This component has two reactions.
        /// </summary>
        public Force BrakeCaliperSupportReaction2 { get; set; }

        /// <summary>
        /// The reaction from bearing.
        /// </summary>
        public double BearingReaction { get; set; }
    }
}
