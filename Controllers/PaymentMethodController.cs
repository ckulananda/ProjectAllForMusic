using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectAllForMusic.Model;


namespace ProjectAllForMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;

        public PaymentMethodController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        // Add a new payment method
        [HttpPost]
        [Route("AddPaymentMethod")]
        public IActionResult AddPaymentMethod([FromBody] PaymentMethod paymentMethod)
        {
            if (paymentMethod == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddPaymentMethod(paymentMethod, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Get all payment methods
        [HttpGet]
        [Route("GetPaymentMethods")]
        public IActionResult GetPaymentMethods()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetPaymentMethods(connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        // Get payment method by ID
        [HttpGet]
        [Route("GetPaymentMethod/{id}")]
        public IActionResult GetPaymentMethodById(int id)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetPaymentMethodById(id, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        // Remove payment method by ID
        [HttpDelete]
        [Route("RemovePaymentMethod/{id}")]
        public IActionResult RemovePaymentMethod(int id)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.RemovePaymentMethodById(id, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }
        // Update an existing payment method
        [HttpPut]
        [Route("UpdatePaymentMethod")]
        public IActionResult UpdatePaymentMethod([FromBody] PaymentMethod paymentMethod)
        {
            // Validate the input
            if (paymentMethod == null || paymentMethod.PaymentMethodID <= 0)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid data." });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.UpdatePaymentMethod(paymentMethod, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }
    }
}
