using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DataController : ControllerBase
    {

        [HttpGet("get-data")]
        public IActionResult GetData()
        {
            var colors = new List<string>(){
                "Xanh", "Vàng", "ABC",
            };

            return Ok(colors);
        }
    }
}