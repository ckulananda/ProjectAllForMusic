using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectAllForMusic.Model;


namespace ProjectAllForMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgressTrackingController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;

        public ProgressTrackingController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        // Submit progress tracking
        [HttpPost]
        [Route("SubmitProgress")]
        public IActionResult SubmitProgress([FromBody] ProgressTracking progress)
        {
            if (progress == null || string.IsNullOrEmpty(progress.Details))
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid progress data." });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddProgressTracking(progress, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                _ => StatusCode(500, response)
            };
        }

        // Get all progress records
        [HttpGet]
        [Route("GetAllProgressTracking")]
        public IActionResult GetAllProgressTracking()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetAllProgressTracking(connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                _ => StatusCode(500, response)
            };
        }

        // Get progress by UserID
        [HttpGet]
        [Route("GetProgressByUserId/{userId}")]
        public IActionResult GetProgressByUserId(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid User ID." });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetProgressByUserId(userId, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }
    }
}
