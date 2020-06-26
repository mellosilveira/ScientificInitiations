using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeOfFreedom;
using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesOfFreedom;
using IcVibracoes.DataContracts.RigidBody.OneDegreeOfFreedom;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesOfFreedom;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IcVibracoes.Controllers
{
    /// <summary>
    /// This controller execute analysis using rigid body concepts.
    /// The object is considered as a unique body, using the absolut value to mass, stiffness and others mechanical properties.
    /// </summary>
    [Route("api/v1/rigid-body")]
    public class RigidBodyController : ControllerBase
    {
        /// <summary>
        /// Calculates the vibration of system with one degree of freedom.
        /// </summary>
        /// <param name="calculateVibration">The operation responsible to calculate the vibration.</param>
        /// <param name="request">The request content for this operation.</param>
        /// <returns>A file with analysis result.</returns>
        /// <response code="201">Returns the newly created files.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="500">If occurred some error in process.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("one-degree-freedom")]
        public async Task<ActionResult<OneDegreeOfFreedomResponse>> CalculateVibration(
            [FromServices] ICalculateVibrationToOneDegreeFreedom calculateVibration,
            [FromBody] OneDegreeOfFreedomRequest request)
        {
            OneDegreeOfFreedomResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Calculates the vibration of system with two degrees of freedom.
        /// </summary>
        /// <param name="calculateVibration">The operation responsible to calculate the vibration.</param>
        /// <param name="request">The request content for this operation.</param>
        /// <returns>A file with analysis result.</returns>
        /// <response code="201">Returns the newly created files.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="500">If occurred some error in process.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("two-degree-freedom")]
        public async Task<ActionResult<TwoDegreesOfFreedomResponse>> CalculateVibration(
            [FromServices] ICalculateVibrationToTwoDegreesFreedom calculateVibration,
            [FromBody] TwoDegreesOfFreedomRequest request)
        {
            TwoDegreesOfFreedomResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
