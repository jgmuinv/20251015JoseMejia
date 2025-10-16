var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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
