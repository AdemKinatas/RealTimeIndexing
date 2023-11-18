using ElasticNetCore.Services;
using Microsoft.EntityFrameworkCore;
using RealTimeIndexing.Interceptors;
using RealTimeIndexing.Services.ElasticSearch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<NorthwindContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(new ChangeTrackingInterceptor());
}, ServiceLifetime.Singleton);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
