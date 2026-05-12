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
    public class InstructorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;

        public InstructorController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        [HttpPost]
        [Route("AddInstructor")]
        public IActionResult AddInstructor([FromBody] Instructor instructor)
        {
            if (instructor == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddInstructor(instructor, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        [HttpGet]
        [Route("GetAllInstructors")]
        public IActionResult GetAllInstructors()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            List<Instructor> instructors = _dal.GetAllInstructors(connection);

            return instructors.Count > 0 ? Ok(instructors) : NotFound(new Response { StatusCode = 404, StatusMessage = "No instructors found." });
        }

        [HttpPut]
        [Route("UpdateInstructor/{id}")]
        public IActionResult UpdateInstructor(string id, [FromBody] Instructor instructor)
        {
            if (instructor == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            instructor.InstructorID = id;
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.UpdateInstructor(instructor, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        [HttpDelete]
        [Route("DeleteInstructor/{id}")]
        public IActionResult DeleteInstructor(string id)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.DeleteInstructor(id, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }
    }
}