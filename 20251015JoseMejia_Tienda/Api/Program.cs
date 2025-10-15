using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1) Controllers
builder.Services.AddControllers();

// 2) OpenAPI + Security (document transformer)
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

        // b) Security Requirement global (aplica a todos los endpoints salvo que se anule)
        doc.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }
            ] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

// 3) Autenticación JWT en ASP.NET Core
// (Clave de ejemplo: use su clave/Issuer/Audience reales desde configuración segura)
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("dk5tn[8i[LJbcU`rC9$jJ0/6f@u9O$J-BzZDR4-D~!+mg*]J5;"));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "JoseMejia",
            ValidateAudience = true,
            ValidAudience = "TiendaWeb",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Ejemplo de política por rol/claim
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
        options.WithTitle("API de ejemplo")
               .WithOpenApiRoutePattern("/openapi/{documentName}.json"); // asegura la ruta
    });
}

app.UseHttpsRedirection();

// 5) Middleware de auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ejemplo de endpoint protegido (si usa minimal APIs):
// app.MapGet("/secure", () => "OK").RequireAuthorization();

app.Run();
