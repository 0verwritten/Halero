using Halero.Models;
using Halero.Services;
using Halero.Services.UserManagement;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseConfigModel>( builder.Configuration.GetSection("DatabaseConfig") );
builder.Services.AddSingleton<MongoDBSessionManager>();
builder.Services.AddTransient<IPasswordHasher, SHA512PasswordHasher>();
builder.Services.AddTransient<ITokenSigner, TokenHMACSHA256Signer>();
builder.Services.AddTransient<ITokenGenerator, TokenGenerator>();
builder.Services.AddSingleton<IUserManager, UserManager>();

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
