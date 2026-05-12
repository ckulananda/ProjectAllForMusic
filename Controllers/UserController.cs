using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectAllForMusic.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Response = ProjectAllForMusic.Model.Response;

namespace ProjectAllForMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;
        private readonly string _uploadFolder = "wwwroot/uploads"; // Directory for profile pictures

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        // Add User
        [HttpPost]
        [Route("AddUser")]
        public IActionResult AddUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest(new Model.Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            DBConnection dbc = new DBConnection();
            Model.Response response = _dal.AddUser(user, dbc.GetConn());

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // User Login
        [HttpPost]
        [Route("Login")]
        public IActionResult Login([FromBody] UserLogin loginDetails)
        {
            if (loginDetails == null || string.IsNullOrEmpty(loginDetails.Email) || string.IsNullOrEmpty(loginDetails.Password))
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid request" });
            }

            DBConnection dbc = new DBConnection();
            Response response = _dal.UserLogin(loginDetails, dbc.GetConn());

            return response.StatusCode == 200 ? Ok(response) : Unauthorized(response);
        }

        // View Users
        [HttpGet]
        [Route("ViewUsers")]
        public IActionResult ViewUsers()
        {
            DBConnection dbc = new DBConnection();
            Response response = _dal.GetUsers(dbc.GetConn());

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Delete User by ID
        [HttpDelete]
        [Route("RemoveUser/{id}")]
        public IActionResult RemoveUser(int id)
        {
            DBConnection dbc = new DBConnection();
            Response response = _dal.RemoveUserById(id, dbc.GetConn());

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Update User by ID
        [HttpPut]
        [Route("UpdateUser/{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (updatedUser == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data: User information is required." });
            }

            updatedUser.UserID = id; // Ensure the ID is set correctly

            try
            {
                using SqlConnection connection = new DBConnection().GetConn();
                Response response = _dal.UpdateUser(updatedUser, connection);

                return response.StatusCode switch
                {
                    200 => Ok(response),
                    400 => BadRequest(response),
                    _ => StatusCode(500, response)
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response
                {
                    StatusCode = 500,
                    StatusMessage = $"An error occurred while updating the user: {ex.Message}"
                });
            }
        }

        
        // Search User by Name
        [HttpGet]
        [Route("SearchUsersByName")]
        public IActionResult SearchUsersByName([FromQuery] string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Username cannot be empty" });
            }

            DBConnection dbc = new DBConnection();
            List<User> users = _dal.SearchUserByName(username, dbc.GetConn());

            if (users.Any())
            {
                return Ok(new Response { StatusCode = 200, StatusMessage = "Users found", Data = users });
            }
            else
            {
                return NotFound(new Response { StatusCode = 404, StatusMessage = "No users found" });
            }
        }

        // Get User by ID
        [HttpGet]
        [Route("GetUserById/{id}")]
        public IActionResult GetUserById(int id)
        {
            DBConnection dbc = new DBConnection();
            User user = _dal.GetUserById(id, dbc.GetConn());

            if (user != null)
            {
                return Ok(new Response { StatusCode = 200, StatusMessage = "User found", Data = user });
            }
            else
            {
                return NotFound(new Response { StatusCode = 404, StatusMessage = "User not found" });
            }
        }

        // Get Users by Role
        [HttpGet]
        [Route("GetUsersByRole")]
        public IActionResult GetUsersByRole([FromQuery] string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Role cannot be empty" });
            }

            DBConnection dbc = new DBConnection();
            List<User> users = _dal.GetUsersByRole(role, dbc.GetConn());

            if (users.Any())
            {
                return Ok(new Response { StatusCode = 200, StatusMessage = "Users found", Data = users });
            }
            else
            {
                return NotFound(new Response { StatusCode = 404, StatusMessage = "No users found for the specified role" });
            }
        }
    }
}
