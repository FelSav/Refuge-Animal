
using ApiMuniChien_V1_Proprietaire.models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Services

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

// BD
builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Serves the Swagger JSON
    app.UseSwaggerUI(); // Serves Swagger UI
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//app.Urls.Add("http://0.0.0.0:5000");

app.Run();
