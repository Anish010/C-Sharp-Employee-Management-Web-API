using System.Text;
using EMS.API.Controllers;
using EMS.BAL;
using EMS.BAL.Interfaces;
using EMS.DAL;
using EMS.DAL.Interfaces;
using EMS.DB.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog first
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/EMSLogs.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();


builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders(); // Remove other logging providers
    loggingBuilder.AddSerilog(); // Add Serilog as the logging provider
});

// Register services
builder.Services.AddScoped<IDropdownBAL, DropdownBAL>();
builder.Services.AddScoped<IDropdownDAL, DropdownDAL>();

builder.Services.AddScoped<IEmployeeBAL, EmployeeBAL>();
builder.Services.AddScoped<IEmployeeDAL, EmployeeDAL>();

builder.Services.AddScoped<IRoleBAL, RoleBAL>();
builder.Services.AddScoped<IRoleDAL, RoleDAL>();

builder.Services.AddScoped<IAuthBAL, AuthBAL>();
builder.Services.AddScoped<IAuthDAL, AuthDAL>();




builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger); // Register Serilog ILogger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(swagger =>
{
    //This is to generate the Default UI of Swagger Documentation
    swagger.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "JWT Token Authentication API",
        Description = ".NET 8 Web API"
    });
    
    // To Enable authorization using Swagger (JWT)
    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
    });
    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}

                    }
                });
});

builder.Services.AddDbContext<EMSContext>(options =>
{
    //options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseSqlServer(b => b.MigrationsAssembly("EMS.API"));  

});


// Add JWT authentication
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
        };
    });

builder.Services.AddMvc();

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseAuthentication();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

app.Run();