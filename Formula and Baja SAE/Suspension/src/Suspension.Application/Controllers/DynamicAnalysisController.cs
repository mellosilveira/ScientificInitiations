﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MudRunner.Commons.DataContracts.Operation;
using MudRunner.Suspension.Application.Extensions;
using MudRunner.Suspension.Core.Operations.RunAnalysis.Dynamic.HalfCar.SixDegreeOfFreedom;
using MudRunner.Suspension.DataContracts.RunAnalysis.Dynamic;
using MudRunner.Suspension.DataContracts.RunAnalysis.Dynamic.HalfCar.SixDegreeOfFreedom;
using System.Threading.Tasks;

namespace MudRunner.Suspension.Application.Controllers
{
    [Route("api/v1/dynamic-analysis")]
    public class DynamicAnalysisController : Controller
    {
        /// <summary>
        /// This operation runs the dynamic analysis to suspension system considering half car and six degrees of freedom.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="request"></param>
        /// <response code="201">Returns the file with the results.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="500">If occurred some error in process.</response>
        /// <response code="501">If some resource is not implemented.</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("half-car/six-degrees-of-freedom")]
        public async Task<ActionResult<OperationResponse<RunDynamicAnalysisResponseData>>> RunAnalysis(
            [FromServices] IRunHalfCarSixDofDynamicAnalysis operation,
            [FromBody] RunHalfCarSixDofDynamicAnalysisRequest request)
        {
            var response = await operation.ProcessAsync(request).ConfigureAwait(false);
            return response.BuildHttpResponse();
        }

        /// <summary>
        /// This operation runs the dynamic analysis to suspension system focusing in the amplitude of the system 
        /// considering half car and six degrees of freedom.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="request"></param>
        /// <response code="201">Returns the file with the results.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="500">If occurred some error in process.</response>
        /// <response code="501">If some resource is not implemented.</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("half-car/six-degrees-of-freedom/amplitude")]
        public async Task<ActionResult<OperationResponse<RunDynamicAnalysisResponseData>>> RunAmplitudeAnalysis(
            [FromServices] IRunHalfCarSixDofAmplitudeDynamicAnalysis operation,
            [FromBody] RunHalfCarSixDofAmplitudeDynamicAnalysisRequest request)
        {
            var response = await operation.ProcessAsync(request).ConfigureAwait(false);
            return response.BuildHttpResponse();
        }
    }
}
