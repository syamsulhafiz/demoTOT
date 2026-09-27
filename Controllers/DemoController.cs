using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Demo1.DTO.SampleDemoDTO;

namespace Demo1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] //Anonymous peringkat controller, bukan by endpoint. Jika endpoint ada [Authorize], ia akan override [AllowAnonymous] di peringkat controller.
    public class DemoController : ControllerBase
    {
        [HttpGet("GetData")]
        public IActionResult Get()
        {
            return Ok("Hello from DemoController!");
        }
        [HttpPost]
        public IActionResult Post(SampleDemoRequest demoRequest)
        {
            var sampleResponse = new SampleDemoResponse { Message = demoRequest.Name };
            return Ok(sampleResponse);
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Ok($"You deleted item with id: {id}");
        }

        /// <summary>
        /// Partial update
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPatch]
        public IActionResult Patch([FromBody] string value)
        {
            return Ok($"You patched: {value}");
        }

        /// <summary>
        /// Full update
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] string value)
        {
            return Ok($"You put: {value} with id: {id}");
        }
    }
}
