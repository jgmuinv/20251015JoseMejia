using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class ProductosController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult Index(string Nombre, string Descripcion, bool Eliminado)
    {
        return View();
    }
}