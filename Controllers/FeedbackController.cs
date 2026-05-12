using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectAllForMusic.Model;


namespace ProjectAllForMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;

        public FeedbackController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        // Submit feedback
        [HttpPost]
        [Route("SubmitFeedback")]
        public IActionResult SubmitFeedback([FromBody] Feedback feedback)
        {
            if (feedback == null || string.IsNullOrEmpty(feedback.FeedbackText))
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid feedback data." });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddFeedback(feedback, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                _ => StatusCode(500, response)
            };
        }

        // Get all feedbacks
        [HttpGet]
        [Route("GetAllFeedbacks")]
        public IActionResult GetAllFeedbacks()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetAllFeedbacks(connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                _ => StatusCode(500, response)
            };
        }

        // Get feedbacks by UserID
        [HttpGet]
        [Route("GetFeedbackByUserId/{userId}")]
        public IActionResult GetFeedbackByUserId(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid User ID." });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetFeedbackByUserId(userId, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }
    }
}
