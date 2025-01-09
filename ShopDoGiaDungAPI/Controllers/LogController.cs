using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDoGiaDungAPI.Attributes;
using ShopDoGiaDungAPI.Services.Interfaces;

namespace ShopDoGiaDungAPI.Controllers
{
    [Authorize]
    [Permission("Access", "Xem")]
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }


        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var result = await _logService.GetLogsAsync(page, pageSize);
            return Ok(new
            {
                data = result.Logs,
                totalCount = result.TotalCount,
                page,
                pageSize
            });
        }
    }
}
