using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Localization;
using RestSharp;
using Web.Services;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

// JSON options (Las usa RestSharp y, si quiere, también su propio código)
var json = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};

// Registrar RestClient como Singleton configurado
builder.Services.AddSingleton(sp =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7055/";
    var client = new RestClient(new RestClientOptions(baseUrl));
    return client;
});

// Registrar su servicio que habla con el API
builder.Services.AddScoped<AuthApiClient>();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Config
builder.Services.Configure<Web.Services.ApiOptions>(builder.Configuration.GetSection("Api"));
builder.Services.Configure<Web.Services.JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Services
builder.Services.AddSingleton<Web.Services.IProductosApiClient, Web.Services.ProductosApiClient>();
builder.Services.AddSingleton<Web.Services.IAuthApiClient, Web.Services.AuthApiClient>();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Culturas admitidas y por defecto
var supportedCultures = new[] { new CultureInfo("es-SV")};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-SV"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Ingresar}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
