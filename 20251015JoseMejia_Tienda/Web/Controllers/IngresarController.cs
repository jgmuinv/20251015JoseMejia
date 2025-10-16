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
        var (ok, token, usuario, error) = await _auth.LoginAsync(vm.Usuario, vm.Clave);
        //// Deserialice SIEMPRE el cuerpo (éxito o error)
        //var dto = await resp.Content.ReadFromJsonAsync<LoginResponse>(error);
        if (ok && !string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(usuario))
        {
            HttpContext.Session.SetString("auth_user", usuario);
            HttpContext.Session.SetString("auth_token", token);
            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);
            return RedirectToAction("Index", "Productos");
        }
        ModelState.AddModelError(string.Empty, error ?? "No se pudo iniciar sesión");
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