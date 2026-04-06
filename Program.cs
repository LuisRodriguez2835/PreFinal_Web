using GymApiFinal.Data;
using GymApiFinal.Middleware;
using GymApiFinal.Security;
using GymApiFinal.Services;
using GymApiFinal.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        return new BadRequestObjectResult(new
        {
            title = "Validación fallida",
            status = 400,
            errors,
            traceId = context.HttpContext.TraceIdentifier
        });
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GymApiFinal", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Pegar token JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddDbContext<GimnasioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSettings = jwtSection.Get<JwtSettings>()!;
builder.Services.Configure<JwtSettings>(jwtSection);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISociosService, SociosService>();
builder.Services.AddScoped<IEntrenadoresService, EntrenadoresService>();
builder.Services.AddScoped<IMembresiasService, MembresiasService>();
builder.Services.AddScoped<IAsistenciasService, AsistenciasService>();
builder.Services.AddScoped<IRutinasService, RutinasService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GimnasioDbContext>();
    await PasswordBootstrap.SeedDemoHashesAsync(db);
}
catch (Exception ex)
{
    Console.WriteLine("⚠️ No se pudo inicializar la BD: " + ex.Message);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

// raíz → dashboard html
app.MapGet("/", () => Results.Redirect("/dashboard.html"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// links en consola
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine();
    Console.WriteLine("======================================");
    Console.WriteLine("✅ GymApiFinal ejecutándose en:");
    foreach (var url in app.Urls)
        Console.WriteLine(url);

    Console.WriteLine("📘 Swagger:");
    foreach (var url in app.Urls)
        Console.WriteLine($"{url}/swagger");

    Console.WriteLine("🧭 Dashboard:");
    foreach (var url in app.Urls)
        Console.WriteLine($"{url}/dashboard.html");

    Console.WriteLine("======================================");
    Console.WriteLine();
});

app.Run();