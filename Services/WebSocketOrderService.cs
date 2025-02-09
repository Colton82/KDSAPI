using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using KDSAPI.Data;

/// <summary>
/// Websocket service for handling order messages.
/// </summary>
public class WebSocketOrderService
{
    private static readonly List<WebSocket> _clients = new();
    private readonly OrderDAO _orderDAO = new();

    /// <summary>
    /// Handles the WebSocket connection for the KDS UI.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task HandleWebSocketAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        _clients.Add(webSocket);
        Console.WriteLine("New KDS UI WebSocket client connected.");

        await Receive(webSocket);
    }

    /// <summary>
    /// Receives the order message from the POS system, broadcasts order to UI, and saves to DB.
    /// </summary>
    /// <param name="webSocket"></param>
    /// <returns></returns>
    private async Task Receive(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("WebSocket connection closed.");
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                _clients.Remove(webSocket);
                return;
            }

            string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Received order: {json}");

            // **Broadcast to all connected clients (KDS UIs)**
            await BroadcastAsync(json);

            // **Save order to database asynchronously**
            _ = Task.Run(() => _orderDAO.SaveOrder(json));
        }
    }

    /// <summary>
    /// Broadcasts the order message to the KDS UI.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task BroadcastAsync(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        var tasks = new List<Task>();

        foreach (var socket in _clients)
        {
            if (socket.State == WebSocketState.Open)
            {
                tasks.Add(socket.SendAsync(new ArraySegment<byte>(data),
                                            WebSocketMessageType.Text,
                                            true,
                                            CancellationToken.None));
            }
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("Order broadcasted to KDS UI.");
    }
}
