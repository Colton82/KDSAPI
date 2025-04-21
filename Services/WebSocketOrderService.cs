using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using KDSAPI.Data;
using KDSAPI.Models;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

public class WebSocketOrderService
{
    private static readonly Dictionary<WebSocket, int> _clientUsers = new();
    private readonly OrderDAO _orderDAO = new();
    private readonly string _jwtSecret = "D84!jf^@ghGHkP1*9]Lm#T%3OqXsZa7Y"; // You can move this to IConfiguration

    public async Task HandleWebSocketAsync(HttpContext context)
    {
        Console.WriteLine("Incoming WebSocket connection...");

        if (!context.WebSockets.IsWebSocketRequest)
        {
            Console.WriteLine("Not a WebSocket request.");
            context.Response.StatusCode = 400;
            return;
        }

        var path = context.Request.Path.ToString();
        Console.WriteLine($"WebSocket path: {path}");

        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

        if (path.Contains("/wss/orders"))
        {
            string authHeader = context.Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                Console.WriteLine("Missing or invalid Authorization header.");
                context.Response.StatusCode = 401;
                return;
            }

            string token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var user = ValidateJwtToken(token);
                int userId = int.Parse(user.FindFirst("userId").Value);
                _clientUsers[webSocket] = userId;
                Console.WriteLine($"WebSocket connected and authenticated for user {userId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation failed: {ex.Message}");
                context.Response.StatusCode = 401;
                return;
            }

            await ListenForMessages(webSocket, expectIncoming: false);
        }
        else if (path.Contains("/wss/pos"))
        {
            Console.WriteLine("POS WebSocket connected.");
            await ListenForMessages(webSocket, expectIncoming: true);
        }
        else
        {
            context.Response.StatusCode = 404;
        }
    }

    private async Task ListenForMessages(WebSocket socket, bool expectIncoming)
    {
        var buffer = new byte[1024 * 4];

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("WebSocket disconnected.");
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                _clientUsers.Remove(socket);
                return;
            }

            if (!expectIncoming)
            {
                continue;
            }

            string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Received order: {json}");

            try
            {
                var order = JsonConvert.DeserializeObject<DynamicOrderModel>(json);
                if (order != null && order.Items != null)
                {
                    await BroadcastToUserAsync(json, order.Users_id);
                    _ = Task.Run(() => _orderDAO.SaveOrder(order));
                }
                else
                {
                    Console.WriteLine("Malformed order received.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process order: {ex.Message}");
            }
        }
    }

    private async Task BroadcastToUserAsync(string message, int userId)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        var tasks = new List<Task>();

        foreach (var kvp in _clientUsers)
        {
            if (kvp.Value == userId && kvp.Key.State == WebSocketState.Open)
            {
                tasks.Add(kvp.Key.SendAsync(new ArraySegment<byte>(data),
                                            WebSocketMessageType.Text,
                                            true,
                                            CancellationToken.None));
            }
        }

        await Task.WhenAll(tasks);
        Console.WriteLine($"Order sent to user {userId}");
    }

    private ClaimsPrincipal ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };

        return tokenHandler.ValidateToken(token, validationParameters, out _);
    }
}
