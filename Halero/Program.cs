using Halero.Models;
using Halero.Services;
using Halero.Services.UserManagement;
using Halero.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseConfigModel>( builder.Configuration.GetSection("DatabaseConfig") );
builder.Services.AddSingleton<MongoDBSessionManager>();
builder.Services.AddTransient<IPasswordHasher, SHA512PasswordHasher>();
builder.Services.AddTransient<ITokenSigner, TokenHMACSHA256Signer>();
builder.Services.AddTransient<ITokenGenerator, TokenGenerator>();
builder.Services.AddSingleton<IUserManager, UserManager>();

// builder.Services.AddSingleton<WeatherSavingService>();


// Add services to the container.

builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
    string swaggerBasePath = "api/app";
    app.UseSwagger(c =>
            {
                c.RouteTemplate = swaggerBasePath+"/swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/{swaggerBasePath}/swagger/v1/swagger.json", $"APP API");
                c.RoutePrefix = $"{swaggerBasePath}/swagger";
            });
}

// app.UseHttpsRedirection();

// app.UseAuthorization();

app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "/api/{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");;


// app.MapControllers();

app.Run();
