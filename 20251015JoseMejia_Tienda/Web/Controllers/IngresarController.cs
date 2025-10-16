using Contratos.DTOs.Ingresar;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace Web.Controllers;

public class IngresarController : Controller
{
    private readonly IAuthApiClient _auth;
    public IngresarController(IAuthApiClient auth)
    {
        _auth = auth;
    }

    [HttpGet]
    public IActionResult Index(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var resp = await _auth.LoginAsync(vm.Usuario, vm.Clave);
        //// Deserialice SIEMPRE el cuerpo (éxito o error)
        //var dto = await resp.Content.ReadFromJsonAsync<LoginResponse>(error);
        if (resp.Ok && !string.IsNullOrWhiteSpace(resp.Token) && !string.IsNullOrWhiteSpace(resp.Usuario))
        {
            HttpContext.Session.SetString("auth_user", resp.Usuario);
            HttpContext.Session.SetString("auth_token", resp.Token);
            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);
            return RedirectToAction("Index", "Productos");
        }
        ModelState.AddModelError(string.Empty, resp.Error ?? "No se pudo iniciar sesión");
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Salir()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
}