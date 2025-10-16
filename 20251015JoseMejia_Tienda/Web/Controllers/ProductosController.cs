using Microsoft.AspNetCore.Mvc;
using Web.Filters;
using Web.Models;
using Web.Services;

namespace Web.Controllers;

[RequireLogin]
public class ProductosController : Controller
{
    private readonly IProductosApiClient _api;
    private readonly Microsoft.Extensions.Logging.ILogger<ProductosController> _logger;
    public ProductosController(IProductosApiClient api, Microsoft.Extensions.Logging.ILogger<ProductosController> logger)
    {
        _api = api;
        _logger = logger;
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
        try
        {
            if (!ModelState.IsValid) return View(vm);

            // Validaciones locales de precio: si no pasan, no ejecutar cambios en la base de datos
            if (vm.PrecioConDescuento.HasValue)
            {
                if (vm.PrecioConDescuento.Value <= 0)
                {
                    ModelState.AddModelError(nameof(vm.PrecioConDescuento), "El descuento debe ser mayor a 0");
                    return View(vm);
                }
                if (vm.PrecioConDescuento.Value > vm.PrecioBase)
                {
                    ModelState.AddModelError(nameof(vm.PrecioConDescuento), "El precio con descuento no puede ser mayor que el precio base");
                    return View(vm);
                }
            }
            if (vm.PrecioBase <= 0)
            {
                ModelState.AddModelError(nameof(vm.PrecioBase), "El precio base debe ser mayor que cero");
                return View(vm);
            }

            // Subir imagen si hay
            if (imagen != null && imagen.Length > 0)
            {
                var ruta = await _api.SubirImagenAsync(imagen.OpenReadStream(), imagen.FileName, Token);
                vm.ImagenUrl = ruta;
            }

            // Primero actualizar datos básicos
            var actualizado = await _api.EditarAsync(id, vm, Token);
            if (actualizado == null) return NotFound();

            // Luego actualizar precio si corresponde
            if (vm.PrecioBase > 0 || (vm.PrecioConDescuento.HasValue && vm.PrecioConDescuento.Value > 0))
            {
                try
                {
                    var retorno = await _api.ActualizarPrecioAsync(id, vm.PrecioBase, vm.PrecioConDescuento, Token);
                    if (retorno == null)
                    {
                        ModelState.AddModelError("", "No se pudo actualizar el precio del producto.");
                        return View(vm);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    // Mensaje de validación desde API
                    ModelState.AddModelError("", ex.Message);
                    return View(vm);
                }
            }

            TempData["msg"] = "Producto actualizado";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en edición de producto {Id}", id);
            ModelState.AddModelError("", "Ocurrió un error inesperado al actualizar el producto.");
            return View(vm);
        }
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