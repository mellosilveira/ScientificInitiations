using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.OneDegreeFreedom;
using IcVibracoes.Core.Operations.RigidBody.CalculateVibration.TwoDegreesFreedom;
using IcVibracoes.DataContracts.RigidBody.OneDegreeFreedom;
using IcVibracoes.DataContracts.RigidBody.TwoDegreesFreedom;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IcVibracoes.Controllers
{
    [Route("api/v1/rigid-body")]
    public class RigidBodyController : ControllerBase
    {
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
