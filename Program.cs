using Halero.Models;
using Halero.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseConfigModel>( builder.Configuration.GetSection("CredentialsManager") );
builder.Services.AddSingleton<MongoDBSessionManager>();
builder.Services.AddTransient<IPasswordHasher, SHA512PasswordHasher>();

// builder.Services.AddSingleton<WeatherSavingService>();


// Add services to the container.

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

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
