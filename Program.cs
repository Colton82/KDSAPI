using KDSAPI.Data;
using KDSAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUsersDAO, UsersDAO>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<SecurityService>();
builder.Services.AddScoped<UsersDAO>();
builder.Logging.AddConsole();
builder.WebHost.UseUrls("https://localhost:7121/");

// ? Move Authentication Setup ABOVE `builder.Build()`
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// ? Now Build the App AFTER Registering Services
var app = builder.Build();

app.UseWebSockets();
Console.WriteLine("WebSocket server running on wss://localhost:7121/ws/orders");
var orderHandler = new WebSocketOrderService();

app.Map("/wss/orders", async context =>
{
    Console.WriteLine("Received WebSocket request...");
    await orderHandler.HandleWebSocketAsync(context);
    Console.WriteLine("WebSocket handler executed.");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
