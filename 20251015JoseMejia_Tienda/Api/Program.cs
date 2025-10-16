using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;

using Aplicacion.Productos;
using Infraestructura.Productos;
using Dominio.Productos;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers()    
    .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

// App Services + Repository (SQL)
var connStr = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddSingleton<IProductoRepository>(sp => new SqlProductoRepository(connStr));
builder.Services.AddSingleton<IProductosService, ProductosService>();

// File storage
builder.Services.AddSingleton<Api.Services.IFileStorage, Api.Services.FileStorage>();

// OpenAPI + Security (document transformer)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Components ??= new();
        // a) Security Scheme tipo HTTP Bearer (JWT)
        doc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Autenticación JWT. En Authorize ingrese SOLO el token (sin 'Bearer')."
        };

        // Security Requirement global (aplica a todos los endpoints salvo que se anule)
        doc.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }
            ] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

// Autenticación JWT (desde configuración)
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;
var secret = jwtSection.GetValue<string>("Secret") ?? string.Empty;
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Política por rol/claim
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin")); // o .RequireClaim("permission", "x")
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // 4) Documento OpenAPI + UI de Scalar
    app.MapOpenApi(); // expone /openapi/v1.json
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("API de tienda Web")
               .WithOpenApiRoutePattern("/openapi/{documentName}.json"); // asegura la ruta
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 5) Middleware de auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint protegido (Al usar minimal APIs):
// app.MapGet("/secure", () => "OK").RequireAuthorization();

app.Run();
