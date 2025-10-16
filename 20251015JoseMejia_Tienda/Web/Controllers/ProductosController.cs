using Microsoft.AspNetCore.Mvc;
using Web.Filters;
using Web.Models;
using Web.Services;

namespace Web.Controllers;

[RequireLogin]
public class ProductosController : Controller
{
    private readonly IProductosApiClient _api;
    public ProductosController(IProductosApiClient api)
    {
        _api = api;
    }

    private string? Token => HttpContext.Session.GetString("auth_token");

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ProductosFiltroViewModel filtro)
    {
        var lista = await _api.ListarAsync(filtro.Nombre, filtro.Descripcion, filtro.Eliminado, Token);
        filtro.Resultados = lista;
        return View(filtro);
    }

    [HttpPost]
    [ActionName("Index")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IndexPost(ProductosFiltroViewModel filtro)
    {
        var lista = await _api.ListarAsync(filtro.Nombre, filtro.Descripcion, filtro.Eliminado, Token);
        filtro.Resultados = lista;
        return View("Index", filtro);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProductoViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductoViewModel vm, IFormFile? imagen)
    {
        if (!ModelState.IsValid) return View(vm);
        if (imagen != null && imagen.Length > 0)
        {
            var ruta = await _api.SubirImagenAsync(imagen.OpenReadStream(), imagen.FileName, Token);
            vm.ImagenUrl = ruta;
        }
        var creado = await _api.CrearAsync(vm, Token);
        TempData["msg"] = $"Creado producto #{creado.Id}";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _api.ObtenerPorIdAsync(id, Token);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductoViewModel vm, IFormFile? imagen)
    {
        if (!ModelState.IsValid) return View(vm);
        if (imagen != null && imagen.Length > 0)
        {
            var ruta = await _api.SubirImagenAsync(imagen.OpenReadStream(), imagen.FileName, Token);
            vm.ImagenUrl = ruta;
        }
        var actualizado = await _api.EditarAsync(id, vm, Token);
        if (actualizado == null) return NotFound();
        // Además, si se envía cambios de precio, actualizar
        if (vm.PrecioBase > 0 || vm.PrecioConDescuento > 0)
        {
            var retorno = await _api.ActualizarPrecioAsync(id, vm.PrecioBase, vm.PrecioConDescuento, Token);
            var abc = "hi";
        }
        TempData["msg"] = "Producto actualizado";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _api.ObtenerPorIdAsync(id, Token);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _api.EliminarAsync(id, Token);
        TempData["msg"] = ok ? "Producto eliminado" : "No se pudo eliminar";
        return RedirectToAction(nameof(Index));
    }
}