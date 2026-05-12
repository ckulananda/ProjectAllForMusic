using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectAllForMusic.Model;


namespace ProjectAllForMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dal _dal;

        public TransactionController(IConfiguration configuration)
        {
            _configuration = configuration;
            _dal = new Dal();
        }

        // Add a new transaction
        [HttpPost]
        [Route("AddTransaction")]
        public IActionResult AddTransaction([FromBody] Transaction transaction)
        {
            if (transaction == null)
            {
                return BadRequest(new Response { StatusCode = 400, StatusMessage = "Invalid Data" });
            }

            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.AddTransaction(transaction, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                400 => BadRequest(response),
                _ => StatusCode(500, response)
            };
        }

        // Get all transactions
        [HttpGet]
        [Route("GetTransactions")]
        public IActionResult GetTransactions()
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetTransactions(connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        // Get transactions by BuyerID
        [HttpGet]
        [Route("GetTransactionsByBuyerId")]
        public IActionResult GetTransactionsByBuyerId([FromQuery] int buyerId)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.GetTransactionsByBuyerId(buyerId, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        // Remove transaction by ID
        [HttpDelete]
        [Route("RemoveTransaction/{id}")]
        public IActionResult RemoveTransaction(int id)
        {
            using SqlConnection connection = new DBConnection().GetConn();
            Response response = _dal.RemoveTransactionById(id, connection);

            return response.StatusCode switch
            {
                200 => Ok(response),
                404 => NotFound(response),
                _ => StatusCode(500, response)
            };
        }
    }
}
