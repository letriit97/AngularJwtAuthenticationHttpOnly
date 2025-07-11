using System.Text;
using API._Services;
using API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


builder.Services.AddDbContext<DBContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.

var jwtSetting = configuration.GetSection("JwtSettings");

builder.Services
    .AddAuthentication(x => {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSetting["validIssuer"],
            ValidAudience = jwtSetting["validAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting["SecretKey"]))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = x =>
            {
                x.Request.Cookies.TryGetValue("accessToken", out var accessToken);
                if (!string.IsNullOrEmpty(accessToken))
                    x.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddSingleton<JwtHandler>();
builder.Services.AddScoped<IAuthenticationServices, AuthenticationService>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
