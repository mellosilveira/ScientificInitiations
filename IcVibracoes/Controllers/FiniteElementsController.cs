using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.Beam.Circular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.Beam.Rectangular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithDva.Circular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithDva.Rectangular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithPiezoelectric.Circular;
using IcVibracoes.Core.Operations.CalculateVibration.FiniteElements.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.DataContracts.FiniteElements;
using IcVibracoes.DataContracts.FiniteElements.Beam;
using IcVibracoes.DataContracts.FiniteElements.BeamWithDynamicVibrationAbsorber;
using IcVibracoes.DataContracts.FiniteElements.BeamWithPiezoelectric;
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
    public class FiniteElementsController : ControllerBase
    {
        /// <summary>
        /// Calculates the vibration to a rectangular profile beam.
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
        [HttpPost("rectangular")]
        public async Task<ActionResult<FiniteElementsResponse>> CalculateVibration(
            [FromServices] ICalculateRectangularBeamVibration calculateVibration,
            [FromBody] BeamRequest<RectangularProfile> request)
        {
            FiniteElementsResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            // TODO - Alterar Ok par Created e passar a URI do arquivo.
            return Ok(response);
        }

        /// <summary>
        /// Calculates the vibration to a rectangular profile beam with dynamic vibration absorbers.
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
        [HttpPost("rectangular/dynamic-vibration-absorber")]
        public async Task<ActionResult<FiniteElementsResponse>> CalculateVibration(
            [FromServices] ICalculateRectangularBeamWithDvaVibration calculateVibration,
            [FromBody] BeamWithDvaRequest<RectangularProfile> request)
        {
            FiniteElementsResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

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
        /// <response code="201">Returns the newly created file.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("rectangular/piezoelectric")]
        public async Task<ActionResult<FiniteElementsResponse>> CalculateVibration(
            [FromServices] ICalculateRectangularBeamWithPiezoelectricVibration calculateVibration,
            [FromBody] BeamWithPiezoelectricRequest<RectangularProfile> request)
        {
            FiniteElementsResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

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
        /// <response code="201">Returns the newly created file.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("circular")]
        public async Task<ActionResult<FiniteElementsResponse>> CalculateVibration(
            [FromServices] ICalculateCircularBeamVibration calculateVibration,
            [FromBody] BeamRequest<CircularProfile> request)
        {
            FiniteElementsResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

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
        /// <response code="201">Returns the newly created file.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("circular/dynamic-vibration-absorber")]
        public async Task<ActionResult<FiniteElementsResponse>> CalculateVibration(
            [FromServices] ICalculateCircularBeamWithDvaVibration calculateVibration,
            [FromBody] BeamWithDvaRequest<CircularProfile> request)
        {
            FiniteElementsResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

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
        /// <response code="201">Returns the newly created file.</response>
        /// <response code="400">If some validation do not passed.</response>
        /// <response code="501">If some resource is not implemented.</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        [HttpPost("circular/piezoelectric")]
        public async Task<ActionResult<FiniteElementsResponse>> CalculateVibration(
            [FromServices] ICalculateCircularBeamWithPiezoelectricVibration calculateVibration,
            [FromBody] BeamWithPiezoelectricRequest<CircularProfile> request)
        {
            FiniteElementsResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}