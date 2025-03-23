using KDSAPI.Data;
using KDSAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KDSAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDAO _orderDAO = new OrderDAO();

        /// <summary>
        /// Retrieves all orders for a given username.
        /// </summary>
        [HttpGet("{username}")]
        public async Task<IActionResult> GetOrdersByUsername(string username)
        {
            try
            {
                List<DynamicOrderModel> orders = await _orderDAO.GetOrdersByUserName(username);

                var processedOrders = orders.Select(o => new
                {
                    o.Id,
                    o.CustomerName,
                    o.Timestamp,
                    o.Station,
                    Items = o.Items // Already deserialized into List<OrderItem>
                }).ToList();

                Console.WriteLine($"Returning {processedOrders.Count} orders for user {username}");

                return Ok(processedOrders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching orders: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching orders.");
            }
        }

        /// <summary>
        /// Updates an existing order.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateOrder([FromBody] DynamicOrderModel updatedOrder)
        {
            if (updatedOrder == null)
            {
                return BadRequest("Invalid order data.");
            }

            Console.WriteLine($"Received Order Update for ID {updatedOrder.Id}");

            try
            {
                DynamicOrderModel existingOrder = await _orderDAO.GetOrderById(updatedOrder.Id);
                if (existingOrder == null)
                {
                    return NotFound($"Order with ID {updatedOrder.Id} not found.");
                }

                existingOrder.CustomerName = updatedOrder.CustomerName;
                existingOrder.Station = updatedOrder.Station;
                existingOrder.Timestamp = updatedOrder.Timestamp;
                existingOrder.Items = updatedOrder.Items; // Directly update Items list

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

        /// <summary>
        /// Archives a completed order.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ArchiveOrder([FromBody] DynamicOrderModel newOrder)
        {
            if (newOrder == null)
            {
                return BadRequest("Invalid order data.");
            }

            Console.WriteLine($"Archiving Completed Order: {newOrder.CustomerName}");

            try
            {
                bool success = await _orderDAO.ArchiveOrderAsync(newOrder);
                if (!success)
                {
                    return StatusCode(500, "Failed to archive order.");
                }

                // Delete the order after archiving
                await _orderDAO.DeleteOrderAsync(newOrder.Id);

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error archiving order: {ex.Message}");
                return StatusCode(500, "An error occurred while archiving the order.");
            }
        }
    }
}
