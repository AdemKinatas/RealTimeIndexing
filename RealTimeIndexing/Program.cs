using ElasticNetCore.Services;
using Microsoft.EntityFrameworkCore;
using RealTimeIndexing.Entities;
using RealTimeIndexing.Extensions;
using RealTimeIndexing.Hubs;
using RealTimeIndexing.Services.ElasticSearch;
using RealTimeIndexing.SubscribeTableDependency;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddSingleton<SubscribeProductTableDependency<Product>>();
builder.Services.AddSingleton<SubscribeProductTableDependency<Category>>();
builder.Services.AddSingleton(typeof(IElasticsearchService<>), typeof(ElasticsearchService<>));

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<NorthwindContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Singleton);

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

app.MapHub<IndexingHub>("/indexingHub");

app.UseSqlTableDependency<SubscribeProductTableDependency<Product>>(connectionString, "Products");
app.UseSqlTableDependency<SubscribeProductTableDependency<Category>>(connectionString, "Categories");

app.Run();
