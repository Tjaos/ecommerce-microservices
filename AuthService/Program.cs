using System.Text;
using AuthService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;



var builder = WebApplication.CreateBuilder(args);

//Ler variáveis de ambiente
var sqlServerHost = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost";
var sqlServerPort = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1433";
var sqlServerUser = Environment.GetEnvironmentVariable("SQLSERVER_USER") ?? "sa";
var sqlServerPassword = Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD") ?? "Str0ngP@ssword!";


builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => sqlOptions.EnableRetryOnFailure()));

AppContext.SetSwitch("System.Data.SqlClient.UseSystemDefaultSslProtocols", true);
AppContext.SetSwitch("Microsoft.Data.SqlClient.DisableCertificateValidation", true);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
    };
});


//Controllers e Swagger
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AuthService",
        Version = "v1",
        Description = "API de autenticação"
    });
});

var app = builder.Build();

//Middlewares
app.UseRouting();
app.UseAuthorization();

//Aplicar migrations automaticamente ao iniciar
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao migrar o banco de dados auth: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthService v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

