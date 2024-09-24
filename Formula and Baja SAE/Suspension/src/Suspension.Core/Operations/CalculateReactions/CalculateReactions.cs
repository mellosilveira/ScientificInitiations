﻿using MudRunner.Commons.Core.ExtensionMethods;
using MudRunner.Commons.Core.Operation;
using MudRunner.Commons.DataContracts.Models;
using MudRunner.Commons.DataContracts.Operation;
using MudRunner.Suspension.Core.ExtensionMethods;
using MudRunner.Suspension.Core.Mapper;
using MudRunner.Suspension.Core.Models.SuspensionComponents;
using MudRunner.Suspension.DataContracts.CalculateReactions;
using System;
using System.Threading.Tasks;

namespace MudRunner.Suspension.Core.Operations.CalculateReactions
{
    // TODO: para formula SAE precisa pensar melhor como fazer no caso da traseira já que o eixo pode ser estrutural.
    /// <summary>
    /// It is responsible to calculate the reactions to suspension system.
    /// </summary>
    public class CalculateReactions : OperationBase<CalculateReactionsRequest, OperationResponse<CalculateReactionsResponseData>>, ICalculateReactions
    {
        private readonly double _precision = 1e-3;
        private readonly IMappingResolver _mappingResolver;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="mappingResolver"></param>
        public CalculateReactions(IMappingResolver mappingResolver)
        {
            this._mappingResolver = mappingResolver;
        }

        /// <summary>
        /// Asynchronously, this method calculates the reactions to suspension system.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Task<OperationResponse<CalculateReactionsResponseData>> ProcessOperationAsync(CalculateReactionsRequest request)
        {
            OperationResponse<CalculateReactionsResponseData> response = new();
            response.SetSuccessOk();

            // Step 1 - Creates the necessary informations about the coordinates of the suspension system components.
            SuspensionSystem suspensionSystem = this._mappingResolver.MapFrom(request);

            // Step 2 - Calculates the matrix with:
            //    The normalized direction of reactions at suspension system.
            //    The displacements between origin and suspension system points.
            double[,] displacement = this.BuildDisplacementMatrix(suspensionSystem, Point3D.Create(request.Origin));

            // Step 3 - Calculates the applied efforts.
            Vector3D forceApplied = Vector3D.Create(request.AppliedForce);
            double[] effort = this.BuildEffortsVector(forceApplied);

            double[] result;
            try
            {
                // Step 4 - Calculates the reactions on suspension system.
                // The equation used: 
                //    [Displacement] * [Reactions] = [Efforts]
                //    [Reactions] = inv([Displacement]) * [Efforts]
                result = displacement
                    .InverseMatrix()
                    .Multiply(effort);
            }
            catch (DivideByZeroException ex)
            {
                response.SetInternalServerError($"Occurred error while inversing displacement matrix. It happens because exists some error in suspension geometry. '{ex.Message}'.");

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                response.SetInternalServerError($"Ocurred error while calculating result. '{ex}'.");

                return Task.FromResult(response);
            }

            // Step 5 - Map the results to response.
            response.Data = this.MapToResponseData(suspensionSystem, result, request.ShouldRoundResults, request.NumberOfDecimalsToRound.GetValueOrDefault());

            // Step 6 - Check if sum of forces is equals to zero, indicanting that the structure is static.
            // This method was commented because some error ocurred and must be investigated.
            //this.CheckForceAndMomentSum(response, forceApplied);

            return Task.FromResult(response);
        }

        /// <summary>
        /// Asynchronously, this method validates the request sent to operation.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Task<OperationResponse<CalculateReactionsResponseData>> ValidateOperationAsync(CalculateReactionsRequest request)
        {
            OperationResponse<CalculateReactionsResponseData> response = new();
            response.SetSuccessOk();

            if (Vector3D.Create(request.AppliedForce).IsZero())
            {
                response.SetBadRequestError("The applied force must have at least one coordinate different than zero.");
            }

            return Task.FromResult(response);
        }

        /// <summary>
        /// This method builds the efforts vector.
        /// </summary>
        /// <param name="force"></param>
        /// <returns></returns>
        public double[] BuildEffortsVector(Vector3D force) => new double[6] { force.X, force.Y, force.Z, 0, 0, 0 };

        /// <summary>
        /// This method builds the matrix with normalized force directions and displacements.
        /// </summary>
        /// <param name="suspensionSystem"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public double[,] BuildDisplacementMatrix(SuspensionSystem suspensionSystem, Point3D origin)
        {
            Vector3D u1 = suspensionSystem.LowerWishbone.NormalizedDirection1;
            Vector3D u2 = suspensionSystem.LowerWishbone.NormalizedDirection2;
            Vector3D u3 = suspensionSystem.UpperWishbone.NormalizedDirection1;
            Vector3D u4 = suspensionSystem.UpperWishbone.NormalizedDirection2;
            Vector3D u5 = suspensionSystem.ShockAbsorber.NormalizedDirection;
            Vector3D u6 = suspensionSystem.TieRod.NormalizedDirection;

            Vector3D r1 = Vector3D.Create(origin, suspensionSystem.LowerWishbone.FrontPivot);
            Vector3D r2 = Vector3D.Create(origin, suspensionSystem.LowerWishbone.RearPivot);
            Vector3D r3 = Vector3D.Create(origin, suspensionSystem.UpperWishbone.FrontPivot);
            Vector3D r4 = Vector3D.Create(origin, suspensionSystem.UpperWishbone.RearPivot);
            Vector3D r5 = Vector3D.Create(origin, suspensionSystem.ShockAbsorber.PivotPoint);
            Vector3D r6 = Vector3D.Create(origin, suspensionSystem.TieRod.PivotPoint);

            return new double[6, 6]
            {
                { u1.X, u2.X, u3.X, u4.X, u5.X, u6.X },
                { u1.Y, u2.Y, u3.Y, u4.Y, u5.Y, u6.Y },
                { u1.Z, u2.Z, u3.Z, u4.Z, u5.Z, u6.Z },
                { r1.CrossProduct(u1).X, r2.CrossProduct(u2).X, r3.CrossProduct(u3).X, r4.CrossProduct(u4).X, r5.CrossProduct(u5).X, r6.CrossProduct(u6).X },
                { r1.CrossProduct(u1).Y, r2.CrossProduct(u2).Y, r3.CrossProduct(u3).Y, r4.CrossProduct(u4).Y, r5.CrossProduct(u5).Y, r6.CrossProduct(u6).Y },
                { r1.CrossProduct(u1).Z, r2.CrossProduct(u2).Z, r3.CrossProduct(u3).Z, r4.CrossProduct(u4).Z, r5.CrossProduct(u5).Z, r6.CrossProduct(u6).Z }
            };
        }

        /// <summary>
        /// This method maps the analysis result to response data.
        /// </summary>
        /// <param name="suspensionSystem"></param>
        /// <param name="result"></param>
        /// <param name="shouldRoundResults"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public CalculateReactionsResponseData MapToResponseData(SuspensionSystem suspensionSystem, double[] result, bool shouldRoundResults, int? decimals)
        {
            // The code below changes the sinal of result (-result) to attest that is passed the component force and not the reactions at chassis.
            var responseData = new CalculateReactionsResponseData
            {
                LowerWishboneReaction1 = Force.Create(-result[0], suspensionSystem.LowerWishbone.NormalizedDirection1),
                LowerWishboneReaction2 = Force.Create(-result[1], suspensionSystem.LowerWishbone.NormalizedDirection2),
                UpperWishboneReaction1 = Force.Create(-result[2], suspensionSystem.UpperWishbone.NormalizedDirection1),
                UpperWishboneReaction2 = Force.Create(-result[3], suspensionSystem.UpperWishbone.NormalizedDirection2),
                ShockAbsorberReaction = Force.Create(-result[4], suspensionSystem.ShockAbsorber.NormalizedDirection),
                TieRodReaction = Force.Create(-result[5], suspensionSystem.TieRod.NormalizedDirection)
            };

            return shouldRoundResults ? responseData.Round(decimals.Value) : responseData;
        }

        /// <summary>
        /// This method checks if sum of forces and moment is equals to zero, indicanting that the structure is static.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="appliedForce"></param>
        public void CheckForceAndMomentSum(OperationResponse<CalculateReactionsResponseData> response, Vector3D appliedForce)
        {
            if (Math.Abs(response.Data.CalculateForceXSum(appliedForce.X)) > this._precision)
            {
                response.SetInternalServerError("The sum of forces at axis X is not equals to zero.");
            }

            if (Math.Abs(response.Data.CalculateForceYSum(appliedForce.Y)) > this._precision)
            {
                response.SetInternalServerError("The sum of forces at axis Y is not equals to zero.");
            }

            if (Math.Abs(response.Data.CalculateForceZSum(appliedForce.Z)) > this._precision)
            {
                response.SetInternalServerError("The sum of forces at axis Z is not equals to zero.");
            }
        }
    }
}
