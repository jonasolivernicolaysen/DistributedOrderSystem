using PaymentService.Data;
using Microsoft.EntityFrameworkCore;
using PaymentService.Services;
using PaymentService.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PaymentDbContext>(options => 
    options.UseSqlite("Data source=payments.db"));

builder.Services.AddScoped<PaymentLogic>();
builder.Services.AddHostedService<OrderCreatedConsumer>();
builder.Services.AddSingleton<PaymentPublisher>();


var app = builder.Build();

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
