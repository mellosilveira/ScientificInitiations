using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.Beam.Circular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.Beam.Rectangular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithDva.Circular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithDva.Rectangular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithPiezoelectric.Circular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElement.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.DataContracts.FiniteElement;
using IcVibracoes.DataContracts.FiniteElement.Beam;
using IcVibracoes.DataContracts.FiniteElement.BeamWithDynamicVibrationAbsorber;
using IcVibracoes.DataContracts.FiniteElement.BeamWithPiezoelectric;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IcVibracoes.Controllers
{
    /// <summary>
    /// This controller executes analysis using finite elements concepts.
    /// The object is divided in elements and the mechanical properties are matricially calculated.
    /// </summary>
    [Route("api/v1/beam")]
    public class FiniteElementController : ControllerBase
    {
        /// <summary>
        /// Calculates the vibration to a rectangular profile beam.
        /// </summary>
        /// <param name="calculateVibration">The operation responsible to calculate the vibration.</param>
        /// <param name="request">The request content for this operation.</param>
        /// <returns>A file with analysis result.</returns>
        /// <response code="201">Returns the newly created files.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("rectangular")]
        public async Task<ActionResult<FiniteElementResponse>> CalculateVibration(
            [FromServices] ICalculateRectangularBeamVibration calculateVibration,
            [FromBody] BeamRequest<RectangularProfile> request)
        {
            FiniteElementResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            // TODO - Alterar Ok para Created e passar a URI do arquivo.
            return Ok(response);
        }

        /// <summary>
        /// Calculates the vibration to a rectangular profile beam with dynamic vibration absorbers.
        /// </summary>
        /// <param name="calculateVibration">The operation responsible to calculate the vibration.</param>
        /// <param name="request">The request content for this operation.</param>
        /// <returns>A file with analysis result.</returns>
        /// <response code="201">Returns the newly created files.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("rectangular/dynamic-vibration-absorber")]
        public async Task<ActionResult<FiniteElementResponse>> CalculateVibration(
            [FromServices] ICalculateRectangularBeamWithDvaVibration calculateVibration,
            [FromBody] BeamWithDvaRequest<RectangularProfile> request)
        {
            FiniteElementResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Calculates the vibration to a rectangular profile beam with piezoelectrics.
        /// </summary>
        /// <param name="calculateVibration">The operation responsible to calculate the vibration.</param>
        /// <param name="request">The request content for this operation.</param>
        /// <returns>A file with analysis result.</returns>
        /// <response code="201">Returns the newly created files.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("rectangular/piezoelectric")]
        public async Task<ActionResult<FiniteElementResponse>> CalculateVibration(
            [FromServices] ICalculateRectangularBeamWithPiezoelectricVibration calculateVibration,
            [FromBody] BeamWithPiezoelectricRequest<RectangularProfile> request)
        {
            FiniteElementResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Calculates the vibration to a circular profile beam.
        /// </summary>
        /// <param name="calculateVibration">The operation responsible to calculate the vibration.</param>
        /// <param name="request">The request content for this operation.</param>
        /// <returns>A file with analysis result.</returns>
        /// <response code="201">Returns the newly created files.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("circular")]
        public async Task<ActionResult<FiniteElementResponse>> CalculateVibration(
            [FromServices] ICalculateCircularBeamVibration calculateVibration,
            [FromBody] BeamRequest<CircularProfile> request)
        {
            FiniteElementResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Calculates the vibration to a circular profile beam with dynamic vibration absorbers.
        /// </summary>
        /// <param name="calculateVibration">The operation responsible to calculate the vibration.</param>
        /// <param name="request">The request content for this operation.</param>
        /// <returns>A file with analysis result.</returns>
        /// <response code="201">Returns the newly created files.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("circular/dynamic-vibration-absorber")]
        public async Task<ActionResult<FiniteElementResponse>> CalculateVibration(
            [FromServices] ICalculateCircularBeamWithDvaVibration calculateVibration,
            [FromBody] BeamWithDvaRequest<CircularProfile> request)
        {
            FiniteElementResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Calculates the vibration to a circular profile beam with piezoelectrics.
        /// </summary>
        /// <param name="calculateVibration">The operation responsible to calculate the vibration.</param>
        /// <param name="request">The request content for this operation.</param>
        /// <returns>A file with analysis result.</returns>
        /// <response code="201">Returns the newly created files.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("circular/piezoelectric")]
        public async Task<ActionResult<FiniteElementResponse>> CalculateVibration(
            [FromServices] ICalculateCircularBeamWithPiezoelectricVibration calculateVibration,
            [FromBody] BeamWithPiezoelectricRequest<CircularProfile> request)
        {
            FiniteElementResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}