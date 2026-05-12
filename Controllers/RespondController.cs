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
    public class RespondController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;

        public RespondController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        // Add a new response
        [HttpPost]
        [Route("AddResponse")]
        public IActionResult AddResponse([FromBody] Respond respond)
        {
            if (respond == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddResponse(respond, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Get all responses
        [HttpGet]
        [Route("GetAllResponses")]
        public IActionResult GetAllResponses()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            List<Respond> responses = _dal.GetAllResponses(connection);

            return responses.Count > 0 ? Ok(responses) : NotFound(new Response { StatusCode = 404, StatusMessage = "No responses found." });
        }

        // Get responses by RequesterID
        [HttpGet]
        [Route("GetResponsesByRequester/{requesterId}")]
        public IActionResult GetResponsesByRequester(string requesterId)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            List<Respond> responses = _dal.GetResponsesByRequester(requesterId, connection);

            return responses.Count > 0 ? Ok(responses) : NotFound(new Response { StatusCode = 404, StatusMessage = "No responses found for the given RequesterID." });
        }

        // Get responses by ResponderID
        [HttpGet]
        [Route("GetResponsesByResponder/{responderId}")]
        public IActionResult GetResponsesByResponder(string responderId)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            List<Respond> responses = _dal.GetResponsesByResponder(responderId, connection);

            return responses.Count > 0 ? Ok(responses) : NotFound(new Response { StatusCode = 404, StatusMessage = "No responses found for the given ResponderID." });
        }

        // Update an existing response
        [HttpPut]
        [Route("UpdateResponse/{id}")]
        public IActionResult UpdateResponse(string id, [FromBody] Respond respond)
        {
            if (respond == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            respond.ResponseID = Convert.ToInt32(id); // assuming ResponseID is an integer
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.UpdateResponse(respond, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Remove a response
        [HttpDelete]
        [Route("RemoveResponse/{id}")]
        public IActionResult RemoveResponse(string id)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.RemoveResponse(id, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }
    }
}
