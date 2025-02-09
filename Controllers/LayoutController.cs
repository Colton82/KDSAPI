using KDSAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KDSAPI.Controllers
{
    /// <summary>
    /// Controller for handling layout data.
    /// </summary>
    [Route("api/[controller]")]
    public class LayoutController : Controller
    {
        LayoutDAO _layoutDAO = new LayoutDAO();

        /// <summary>
        /// Retrieves the layout data for the specified user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetLayoutByUserId(int id)
        {
            string[] stations = _layoutDAO.GetLayoutByUserId(id);

            return Ok(stations);
        }

        /// <summary>
        /// Saves the layout data for the specified user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="layout"></param>
        /// <returns></returns>
        [HttpPost("save/{id}")]
        public IActionResult SaveLayout(int id, [FromBody] List<string> layout)
        {
            if (layout == null || layout.Count == 0)
            {
                return BadRequest("Layout data is required.");
            }

            string layoutString = string.Join(",", layout);

            bool success = _layoutDAO.SaveLayout(id, layoutString);

            if (success)
            {
                return Ok("Layout saved successfully.");
            }
            else
            {
                return StatusCode(500, "Failed to save layout.");
            }
        }
    }
}
