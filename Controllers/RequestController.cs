using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectAllForMusic.Model;


namespace ProjectAllForMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;

        public RequestController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        // Add a new request
        [HttpPost]
        [Route("AddRequest")]
        public IActionResult AddRequest([FromBody] Request request)
        {
            if (request == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddRequest(request, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Get all requests
        [HttpGet]
        [Route("GetRequests")]
        public IActionResult GetRequests()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetRequests(connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        // Update request status
        [HttpPut]
        [Route("UpdateRequestStatus/{id}")]
        public IActionResult UpdateRequestStatus(int id, [FromBody] string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Status" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.UpdateRequestStatus(id, status, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        // Remove request by ID
        [HttpDelete]
        [Route("RemoveRequest/{id}")]
        public IActionResult RemoveRequest(int id)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.RemoveRequestById(id, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        // Get requests by RequesterID
        [HttpGet]
        [Route("GetRequestsByRequesterId")]
        public IActionResult GetRequestsByRequesterId([FromQuery] int requesterId)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetRequestsByRequesterId(requesterId, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        // Get requests by RequestedEntityID
        [HttpGet]
        [Route("GetRequestsByRequestedEntityId")]
        public IActionResult GetRequestsByRequestedEntityId([FromQuery] int requestedEntityId)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetRequestsByRequestedEntityId(requestedEntityId, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

    }
}
