using System.Text;
using AuthService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;



var builder = WebApplication.CreateBuilder(args);

//Ler variáveis de ambiente
var sqlServerHost = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost";
var sqlServerPort = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1433";
var sqlServerUser = Environment.GetEnvironmentVariable("SQLSERVER_USER") ?? "sa";
var sqlServerPassword = Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD") ?? "Str0ngP@ssword!";

//Montar connection string dinamicamente
var connectionString = $"Server={sqlServerHost},{sqlServerPort};Database=AuthDb;User Id={sqlServerUser};Password={sqlServerPassword};";

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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// moved app creation up to allow migration after building

var app = builder.Build();

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

