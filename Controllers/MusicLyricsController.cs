using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectAllForMusic.Model;
using System;

namespace ProjectAllForMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MusicLyricsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;

        public MusicLyricsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        // Add Music Lyric
        [HttpPost]
        [Route("AddMusicLyrics")]
        public IActionResult AddMusicLyrics([FromBody] MusicLyrics musicLyrics)
        {
            if (musicLyrics == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddMusicLyrics(musicLyrics, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Get All Music Lyrics
        [HttpGet]
        [Route("GetMusicLyrics")]
        public IActionResult GetMusicLyrics()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetMusicLyrics(connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Get Music Lyrics by Author ID
        [HttpGet]
        [Route("GetMusicLyricsByAuthorId/{authorId}")]
        public IActionResult GetMusicLyricsByAuthorId(int authorId)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetMusicLyricsByAuthorId(authorId, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        // Update Music Lyric by ID
        [HttpPut]
        [Route("UpdateMusicLyrics/{id}")]
        public IActionResult UpdateMusicLyrics(int id, [FromBody] MusicLyrics updatedMusicLyrics)
        {
            if (updatedMusicLyrics == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            updatedMusicLyrics.LyricID = id;

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.UpdateMusicLyrics(updatedMusicLyrics, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Remove Music Lyric by ID
        [HttpDelete]
        [Route("RemoveMusicLyrics/{id}")]
        public IActionResult RemoveMusicLyrics(int id)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.RemoveMusicLyricsById(id, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        // Search Music Lyrics by Title
        [HttpGet]
        [Route("SearchMusicLyricsByTitle")]
        public IActionResult SearchMusicLyricsByTitle([FromQuery] string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Title cannot be empty" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.SearchMusicLyricsByTitle(title, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }
    }
}
