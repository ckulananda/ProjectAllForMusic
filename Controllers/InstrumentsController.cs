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
    public class InstrumentsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;

        public InstrumentsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        // Add Instrument
        [HttpPost]
        [Route("AddInstrument")]
        public IActionResult AddInstrument([FromBody] Instruments instrument)
        {
            if (instrument == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddInstrument(instrument, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Get All Instruments
        [HttpGet]
        [Route("GetInstruments")]
        public IActionResult GetInstruments()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetInstruments(connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Update Instrument by ID
        [HttpPut]
        [Route("UpdateInstrument/{id}")]
        public IActionResult UpdateInstrument(int id, [FromBody] Instruments updatedInstrument)
        {
            if (updatedInstrument == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            updatedInstrument.InstrumentID = id; // Ensure the ID is set correctly

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.UpdateInstrument(updatedInstrument, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Remove Instrument by ID
        [HttpDelete]
        [Route("RemoveInstrument/{id}")]
        public IActionResult RemoveInstrument(int id)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.RemoveInstrumentById(id, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        // Search Instruments by Name
        [HttpGet]
        [Route("SearchInstrumentsByName")]
        public IActionResult SearchInstrumentsByName([FromQuery] string instrumentName)
        {
            if (string.IsNullOrEmpty(instrumentName))
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Instrument name cannot be empty" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.SearchInstrumentsByName(instrumentName, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        // Get Instruments by Seller ID
        [HttpGet]
        [Route("GetInstrumentsBySeller/{sellerID}")]
        public IActionResult GetInstrumentsBySeller(int sellerID)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetInstrumentsBySellerID(sellerID, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }
    }
}
