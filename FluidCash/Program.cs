
using FluidCash.Extensions;
using FluidCash.LogConfigs;
using FluidCash.Middlewares;

LogConfigurator.ConfigureLogger(); 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.RegisterDbContext(builder.Configuration);
builder.Services.RegisterBaseRpositories();
builder.Services.ConfigureAspNetIdentity(builder.Configuration);
builder.Services.ConfigureTokenService(builder.Configuration);
builder.Services.RegisterContainerServices();
builder.Services.ConfigureRedisCache();
builder.Services.ConfigureEmailService(builder.Configuration);
builder.Services.ConfigurePayStackServices();
builder.Services.ConfigureCloudinary(builder.Configuration);
builder.Services.ConfigureAuthServices(builder.Configuration);

builder.Services.ConfigureControllers();
builder.Services.ConfigureApiVersioning();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwaggerGen();

var app = builder.Build();
app.ConfigureExceptionHandler();
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
