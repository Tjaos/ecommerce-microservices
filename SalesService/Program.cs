using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SalesService.Data;

var builder = WebApplication.CreateBuilder(args);

//Ler variáveis de ambiente
var sqlServerHost = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost";
var sqlServerPort = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1433";
var sqlServerUser = Environment.GetEnvironmentVariable("SQLSERVER_USER") ?? "sa";
var sqlServerPassword = Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD") ?? "Str0ngP@ssword!";

//Montar connection string dinamicamente
var connectionString = $"Server={sqlServerHost},{sqlServerPort};Database=SalesDb;User Id={sqlServerUser};Password={sqlServerPassword};";

builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlServer(connectionString));


var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key está faltando na configuração.");
}

//JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "AuthService",
        ValidAudience = "AuthService",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Key JWT não está configurado.")
            )
        )
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token aqui!"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new string[] { } }
    });
});
builder.Services.AddHttpClient();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao migrar o banco de dados sales: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
