using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectAllForMusic.Model;
using System;
using System.Collections.Generic;

namespace ProjectAllForMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MusicianController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;
        public MusicianController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        [HttpPost]
        [Route("AddMusician")]
        public IActionResult AddMusician([FromBody] Musician musician)
        {
            if (musician == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddMusician(musician, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        [HttpGet]
        [Route("GetAllMusicians")]
        public IActionResult GetAllMusicians()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            List<Musician> musicians = _dal.GetAllMusicians(connection);

            return musicians.Count > 0 ? Ok(musicians) : NotFound(new Response { StatusCode = 404, StatusMessage = "No musicians found." });
        }

        [HttpPut]
        [Route("UpdateMusician/{id}")]
        public IActionResult UpdateMusician(string id, [FromBody] Musician musician)
        {
            if (musician == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            musician.MusicianID = id;
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.UpdateMusician(musician, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        [HttpDelete]
        [Route("DeleteMusician/{id}")]
        public IActionResult DeleteMusician(string id)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.DeleteMusician(id, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }
    }
}
