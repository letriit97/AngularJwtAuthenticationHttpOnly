using System.Text;
using API._Services;
using API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

//================ CORS ================
builder.Services.AddCors();

builder.Services.AddDbContext<DBContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.

var jwtSetting = configuration.GetSection("JwtSettings");

builder.Services
    .AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
    ).AddCookie(x =>
    {
        x.Cookie.Name = "Cookies";
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
                x.Request.Cookies.TryGetValue("X-Access-Token", out var accessToken);
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

//================ CORS ================
app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
