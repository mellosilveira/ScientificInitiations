﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MudRunner.Commons.DataContracts.Operation;
using MudRunner.Suspension.Application.Extensions;
using MudRunner.Suspension.Core.Operations.CalculateReactions;
using MudRunner.Suspension.Core.Operations.CalculateStearingKnuckleReactions;
using MudRunner.Suspension.DataContracts.CalculateReactions;
using MudRunner.Suspension.DataContracts.CalculateSteeringKnuckleReactions;
using System.Threading.Tasks;

namespace MudRunner.Suspension.Application.Controllers
{
    [Route("api/v1/suspension-reactions")]
    public class ReactionController : Controller
    {
        /// <summary>
        /// This operation calculates the reactions to suspension system.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="request"></param>
        /// <response code="200">Returns the reactions value.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="500">If occurred some error in process.</response>
        /// <response code="501">If some resource is not implemented.</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("calculate")]
        public async Task<ActionResult<OperationResponse<CalculateSteeringKnuckleReactionsResponseData>>> CalculateReactions(
            [FromServices] ICalculateReactions operation,
            [FromBody] CalculateReactionsRequest request)
        {
            var response = await operation.ProcessAsync(request).ConfigureAwait(false);
            return response.BuildHttpResponse();
        }

        /// <summary>
        /// This operation calculates the reactions to steering knuckle.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="request"></param>
        /// <response code="200">Returns the reactions value.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="500">If occurred some error in process.</response>
        /// <response code="501">If some resource is not implemented.</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("steering-knuckle/calculate")]
        public async Task<ActionResult<OperationResponse<CalculateSteeringKnuckleReactionsResponseData>>> CalculateSteeringKnuckleReactions(
            [FromServices] ICalculateSteeringKnuckleReactions operation,
            [FromBody] CalculateSteeringKnuckleReactionsRequest request)
        {
            var response = await operation.ProcessAsync(request).ConfigureAwait(false);
            return response.BuildHttpResponse();
        }
    }
}
