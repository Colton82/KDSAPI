using KDSAPI.Data;
using MySqlX.XDevAPI;
using System.Net.WebSockets;
using System.Text;

namespace KDSAPI.Services
{
    public class WebSocketOrderService
    {
        private static readonly List<WebSocket> clients = new();
        private readonly OrderDAO orderDAO = new();

        public async Task HandleWebSocketAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            System.Diagnostics.Debug.WriteLine("Still listening");
            clients.Add(webSocket);
            Console.WriteLine("Client connected.");
            await Recieve(webSocket);
        }

        private async Task Recieve(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            while(webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if(result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    clients.Remove(webSocket);
                    Console.WriteLine("WebSocket connection closed.");
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received order: {json}");

                    // Broadcast order to all connected KDS UIs
                    await BroadcastAsync(json);

                    // Asynchronously save to the database
                    _ = Task.Run(() => orderDAO.SaveOrder(json));
                }
            }
        }
        private async Task BroadcastAsync(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            var tasks = new List<Task>();

            foreach (var socket in clients)
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
        }

    }
}
