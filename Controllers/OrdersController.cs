using KDSAPI.Data;
using KDSAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Text.Json;

namespace KDSAPI.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDAO _orderDAO = new OrderDAO();

        [HttpGet("{username}")]
        public IActionResult GetOrdersByUsername(string username)
        {
            try
            {
                OrderModel[] orders = _orderDAO.GetOrdersByUserName(username);
                foreach (var order in orders)
                {
                    Console.WriteLine($"Retrieved ItemsJson from DB: {order.ItemsJson}");
                }

                var processedOrders = orders.Select(o => new
                {
                    o.Id,
                    o.CustomerName,
                    o.Timestamp,
                    o.Users_id,
                    o.Station,

                    Items = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(o.ItemsJson)
                }).ToList();

                foreach (var order in processedOrders)
                {
                    Console.WriteLine($"Order ID: {order.Id}, Items: {JsonConvert.SerializeObject(order.Items, Formatting.Indented)}");
                }

                string jsonResponse = JsonConvert.SerializeObject(processedOrders, Formatting.Indented);
                Console.WriteLine($"Final API JSON Response:\n{jsonResponse}");

                return Ok(processedOrders);



            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching orders: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching orders.");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOrder([FromBody] OrderModel updatedOrder)
        {
            if (updatedOrder == null)
            {
                return BadRequest("Invalid order data.");
            }

            Console.WriteLine($"Received Order Update for ID {updatedOrder.Id}");
            Console.WriteLine($"CustomerName: {updatedOrder.CustomerName}");
            Console.WriteLine($"Station: {updatedOrder.Station}");
            Console.WriteLine($"TimeStamp: {updatedOrder.Timestamp}");
            Console.WriteLine($"ItemsJson: {updatedOrder.ItemsJson}");

            try
            {
                var existingOrder = _orderDAO.GetOrderById(updatedOrder.Id);
                if (existingOrder == null)
                {
                    return NotFound($"Order with ID {updatedOrder.Id} not found.");
                }

                existingOrder.CustomerName = updatedOrder.CustomerName;
                existingOrder.Station = updatedOrder.Station;
                existingOrder.Timestamp = updatedOrder.Timestamp;
                existingOrder.ItemsJson = updatedOrder.ItemsJson;

                bool success = await _orderDAO.UpdateOrderAsync(existingOrder);
                if (!success)
                {
                    return StatusCode(500, "Failed to update order.");
                }

                return NoContent(); // 204 No Content (success)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order {updatedOrder.Id}: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the order.");
            }
        }


    }
}
