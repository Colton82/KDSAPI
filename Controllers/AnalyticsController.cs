using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KDSAPI.Data;
using Newtonsoft.Json;

namespace KDSAPI.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly OrderDAO _orderDAO;

        public AnalyticsController()
        {
            _orderDAO = new OrderDAO();
        }

        [HttpPost("performance")]
        public async Task<IActionResult> GetAnalytics([FromBody] AnalyticsRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.StartDate))
            {
                return BadRequest("Invalid request data.");
            }

            if (!DateTime.TryParse(request.StartDate, out DateTime parsedDate))
            {
                return BadRequest("Invalid date format.");
            }

            try
            {
                string username = request.Username;

                var analyticsData = await _orderDAO.GetAnalyticsAsync(parsedDate, username);

                if (analyticsData == null)
                {
                    return StatusCode(500, "Error retrieving analytics.");
                }

                return Ok(analyticsData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving analytics: {ex.Message}");
            }
        }

    }

    public class AnalyticsRequest
    {
        public string StartDate { get; set; }
        public string Username { get; set; }
    }

}
