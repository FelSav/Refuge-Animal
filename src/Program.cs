
using ApiMuniChien_V1_Proprietaire.models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<ProprietairesContext>(opt =>opt.UseNpgsql(connectionString));
builder.Services.AddDbContext<TrainContext>(opt =>opt.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Urls.Add("http://0.0.0.0:5000");

app.Run();
