using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectAllForMusic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Response = ProjectAllForMusic.Model.Response;

namespace ProjectAllForMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningPackageController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;

        public LearningPackageController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        // Add Learning Package
        [HttpPost]
        [Route("AddLearningPackage")]
        public IActionResult AddLearningPackage([FromBody] LearningPackage learningPackage)
        {
            if (learningPackage == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddLearningPackage(learningPackage, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Get All Learning Packages
        [HttpGet]
        [Route("GetLearningPackages")]
        public IActionResult GetLearningPackages()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetLearningPackages(connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Update Learning Package by ID
        [HttpPut]
        [Route("UpdateLearningPackage/{id}")]
        public IActionResult UpdateLearningPackage(int id, [FromBody] LearningPackage updatedLearningPackage)
        {
            if (updatedLearningPackage == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            updatedLearningPackage.PackageID = id; // Ensure the ID is set correctly

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.UpdateLearningPackage(updatedLearningPackage, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        [HttpDelete]
        [Route("RemoveLearningPackage/{id}")]
        public IActionResult RemoveLearningPackage(int id)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.RemoveLearningPackageById(id, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        [HttpGet]
        [Route("SearchLearningPackagesByName")]
        public IActionResult SearchLearningPackagesByName([FromQuery] string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Package name cannot be empty" });
            }

            // Instantiate the DAL class
            var dal = new Dal();  // Assuming Dal is not static
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = dal.SearchLearningPackageByName(packageName, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }
        [HttpGet]
        [Route("GetLearningPackagesByInstructor/{instructorId}")]
        public IActionResult GetLearningPackagesByInstructor(int instructorId)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetLearningPackagesByInstructorID(instructorId, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }



    }
}
