using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Operations.Beam.Circular;
using IcVibracoes.Core.Operations.Beam.Rectangular;
using IcVibracoes.Core.Operations.BeamWithDva.Circular;
using IcVibracoes.Core.Operations.BeamWithDva.Rectangular;
using IcVibracoes.Core.Operations.BeamWithPiezoelectric.Circular;
using IcVibracoes.Core.Operations.BeamWithPiezoelectric.Rectangular;
using IcVibracoes.DataContracts.CalculateVibration;
using IcVibracoes.DataContracts.CalculateVibration.Beam;
using IcVibracoes.DataContracts.CalculateVibration.BeamWithDynamicVibrationAbsorber;
using IcVibracoes.DataContracts.CalculateVibration.BeamWithPiezoelectric;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IcVibracoes.Controllers
{

    [Route("api/v1/beam")]
    public class VibrationController : ControllerBase
    {
        [HttpPost("rectangular")]
        public async Task<ActionResult<CalculateVibrationResponse>> Calculate(
            [FromServices] ICalculateRectangularBeamVibration calculateVibration,
            [FromBody] CalculateBeamVibrationRequest<RectangularProfile> request)
        {
            CalculateVibrationResponse response = await calculateVibration.Process(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("rectangular/dynamic-vibration-absorber")]
        public async Task<ActionResult<CalculateVibrationResponse>> Calculate(
            [FromServices] ICalculateRectangularBeamWithDvaVibration calculateVibration,
            [FromBody] CalculateBeamWithDvaVibrationRequest<RectangularProfile> request)
        {
            CalculateVibrationResponse response = await calculateVibration.Process(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("rectangular/piezoelectric")]
        public async Task<ActionResult<CalculateVibrationResponse>> Calculate(
            [FromServices] ICalculateRectangularBeamWithPiezoelectricVibration calculateVibration,
            [FromBody] CalculateBeamWithPiezoelectricVibrationRequest<RectangularProfile> request)
        {
            CalculateVibrationResponse response = await calculateVibration.Process(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("circular")]
        public async Task<ActionResult<CalculateVibrationResponse>> Calculate(
            [FromServices] ICalculateCircularBeamVibration calculateVibration,
            [FromBody] CalculateBeamVibrationRequest<CircularProfile> request)
        {
            CalculateVibrationResponse response = await calculateVibration.Process(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("circular/dynamic-vibration-absorber")]
        public async Task<ActionResult<CalculateVibrationResponse>> Calculate(
            [FromServices] ICalculateCircularBeamWithDvaVibration calculateVibration,
            [FromBody] CalculateBeamWithDvaVibrationRequest<CircularProfile> request)
        {
            CalculateVibrationResponse response = await calculateVibration.Process(request);

            if(!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("circular/piezoelectric")]
        public async Task<ActionResult<CalculateVibrationResponse>> Calculate(
            [FromServices] ICalculateCircularBeamWithPiezoelectricVibration calculateVibration,
            [FromBody] CalculateBeamWithPiezoelectricVibrationRequest<CircularProfile> request)
        {
            CalculateVibrationResponse response = await calculateVibration.Process(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}