using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectAllForMusic.Model;


namespace ProjectAllForMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LatestSongNotationsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;

        public LatestSongNotationsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        // Add new song notation
        [HttpPost]
        [Route("AddNotation")]
        public IActionResult AddNotation([FromBody] LatestSongNotations songNotation)
        {
            if (songNotation == null || string.IsNullOrEmpty(songNotation.SongTitle) || string.IsNullOrEmpty(songNotation.Notation))
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid notation data." });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddSongNotation(songNotation, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                _ => StatusCode(500, response)
            };
        }

        // Get all song notations
        [HttpGet]
        [Route("GetAllNotations")]
        public IActionResult GetAllNotations()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetAllSongNotations(connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                _ => StatusCode(500, response)
            };
        }

        // Get song notation by title
        [HttpGet]
        [Route("GetNotationByTitle/{songTitle}")]
        public IActionResult GetNotationByTitle(string songTitle)
        {
            if (string.IsNullOrEmpty(songTitle))
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid song title." });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetNotationBySongTitle(songTitle, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }
    }
}
