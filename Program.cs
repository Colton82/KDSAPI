using KDSAPI.Data;
using KDSAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUsersDAO, UsersDAO>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<SecurityService>();
builder.Services.AddScoped<UsersDAO>();
builder.Logging.AddConsole();
builder.WebHost.UseUrls("https://localhost:7121/");
var app = builder.Build();

app.UseWebSockets();
Console.WriteLine("WebSocket server running on wss://localhost:7121/ws/orders");
var orderHandler = new WebSocketOrderService();

app.Map("/wss/orders", async context =>
{
    Console.WriteLine("Received WebSocket request...");
    await orderHandler.HandleWebSocketAsync(context);
    Console.WriteLine("WebSocket handler executed.");
    Console.WriteLine("WebSocket handler executed.");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
