using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Swagger "normal" primeiro
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API Gateway", Version = "v1" });
});

// SwaggerForOcelot depois
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// Ocelot por último
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// 🔹 Swagger "normal" do gateway
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("swagger.json", "API Gateway v1");
    });
}

// 🔹 Swagger unificado (Ocelot)
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

// 🔹 Middleware Ocelot
await app.UseOcelot();

app.Run();
