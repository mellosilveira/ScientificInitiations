﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MudRunner.Commons.DataContracts.Models.Profiles;
using MudRunner.Commons.DataContracts.Operation;
using MudRunner.Suspension.Application.Extensions;
using MudRunner.Suspension.Core.Operations.RunAnalysis.Fatigue.CircularProfile;
using MudRunner.Suspension.Core.Operations.RunAnalysis.Fatigue.RectangularProfile;
using MudRunner.Suspension.DataContracts.RunAnalysis.Fatigue;
using System.Threading.Tasks;

namespace MudRunner.Suspension.Application.Controllers
{
    [Route("api/v1/fatigue-analysis")]
    public class FatigueAnalysisController : Controller
    {
        /// <summary>
        /// This operation run the fatigue analysis considering that all geometry uses a cicular beam profile.
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
        [HttpPost("circular-profile/run")]
        public async Task<ActionResult<OperationResponse<RunFatigueAnalysisResponseData>>> RunAnalysis(
            [FromServices] IRunCircularProfileFatigueAnalysis operation,
            [FromBody] RunFatigueAnalysisRequest<CircularProfile> request)
        {
            var response = await operation.ProcessAsync(request).ConfigureAwait(false);
            return response.BuildHttpResponse();
        }

        /// <summary>
        /// This operation run the fatigue analysis considering that all geometry uses a rectangular beam profile.
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
        [HttpPost("rectangular-profile/run")]
        public async Task<ActionResult<OperationResponse<RunFatigueAnalysisResponseData>>> RunAnalysis(
            [FromServices] IRunRectangularProfileFatigueAnalysis operation,
            [FromQuery] RunFatigueAnalysisRequest<RectangularProfile> request)
        {
            var response = await operation.ProcessAsync(request).ConfigureAwait(false);
            return response.BuildHttpResponse();
        }
    }
}
