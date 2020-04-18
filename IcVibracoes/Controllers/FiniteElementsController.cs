using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.Beam.Circular;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.Beam.Rectangular;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithDva.Circular;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithDva.Rectangular;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithPiezoelectric.Circular;
using IcVibracoes.Core.Operations.FiniteElements.CalculateVibration.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.DataContracts.FiniteElements;
using IcVibracoes.DataContracts.FiniteElements.Beam;
using IcVibracoes.DataContracts.FiniteElements.BeamWithDynamicVibrationAbsorber;
using IcVibracoes.DataContracts.FiniteElements.BeamWithPiezoelectric;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IcVibracoes.Controllers
{

    [Route("api/v1/beam")]
    public class FiniteElementsController : ControllerBase
    {
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

            return Ok(response);
        }

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

        [HttpPost("circular/dynamic-vibration-absorber")]
        public async Task<ActionResult<FiniteElementsResponse>> CalculateVibration(
            [FromServices] ICalculateCircularBeamWithDvaVibration calculateVibration,
            [FromBody] BeamWithDvaRequest<CircularProfile> request)
        {
            FiniteElementsResponse response = await calculateVibration.Process(request).ConfigureAwait(false);

            if(!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

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