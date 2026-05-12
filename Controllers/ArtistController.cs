using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectAllForMusic.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace ProjectAllForMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;

        public ArtistController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        [HttpPost]
        [Route("AddArtist")]
        public IActionResult AddArtist([FromBody] Artist artist)
        {
            if (artist == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddArtist(artist, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        [HttpGet]
        [Route("GetAllArtists")]
        public IActionResult GetAllArtists()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            List<Artist> artists = _dal.GetAllArtists(connection);

            return artists.Count > 0 ? Ok(artists) : NotFound(new Response { StatusCode = 404, StatusMessage = "No artists found." });
        }

        [HttpPut]
        [Route("UpdateArtist/{id}")]
        public IActionResult UpdateArtist(string id, [FromBody] Artist artist)
        {
            if (artist == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            artist.ArtistID = id;
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.UpdateArtist(artist, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        [HttpDelete]
        [Route("DeleteArtist/{id}")]
        public IActionResult DeleteArtist(string id)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.DeleteArtist(id, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }
    }
}