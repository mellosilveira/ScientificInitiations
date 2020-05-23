using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeFreedom;
using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesFreedom;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;
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
        /// <response code="201">Returns the newly created file.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("one-degree-freedom")]
        public async Task<ActionResult<OneDegreeFreedomResponse>> CalculateVibration(
            [FromServices] ICalculateVibrationToOneDegreeFreedom calculateVibration,
            [FromBody] OneDegreeFreedomRequest request)
        {
            OneDegreeFreedomResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

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
        /// <response code="201">Returns the newly created file.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("two-degree-freedom")]
        public async Task<ActionResult<TwoDegreesFreedomResponse>> CalculateVibration(
            [FromServices] ICalculateVibrationToTwoDegreesFreedom calculateVibration,
            [FromBody] TwoDegreesFreedomRequest request)
        {
            TwoDegreesFreedomResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
