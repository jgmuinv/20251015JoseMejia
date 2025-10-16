using Aplicacion.Productos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;
[ApiController]
[Route("[controller]/[action]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IProductosService _service;
    private readonly Api.Services.IFileStorage _files;

    public ProductosController(IProductosService service, Api.Services.IFileStorage files)
    {
        _service = service;
        _files = files;
    }

    [HttpPost("imagen")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<object>> SubirImagen([FromForm] IFormFile archivo, CancellationToken ct)
    {
        if (archivo == null || archivo.Length == 0) return BadRequest("Archivo vacío");
        var rutaRelativa = await _files.SaveProductImageAsync(archivo, ct);
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var ruta = rutaRelativa.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? rutaRelativa
            : baseUrl + (rutaRelativa.StartsWith("/") ? rutaRelativa : "/" + rutaRelativa);
        return Ok(new { ruta });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductoDto>>> Listar(CancellationToken ct, string Nombre = "", string Descripcion = "", bool Eliminado = false )
    {
        var lista = await _service.ListarAsync(Nombre, Descripcion, Eliminado, ct);
        return Ok(lista);
    }

    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Crear([FromBody] CrearProductoDto dto, CancellationToken ct)
    {
        var creado = await _service.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId([FromRoute] int id, CancellationToken ct)
    {
        if (id <= 0) return BadRequest("El id debe ser mayor a cero");
        var item = await _service.ObtenerPorIdAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductoDto>> Editar([FromRoute] int id, [FromBody] EditarProductoDto dto, CancellationToken ct)
    {
        var actualizado = await _service.EditarAsync(id, dto, ct);
        return actualizado is null ? NotFound() : Ok(actualizado);
    }

    [HttpPut("{id}/precio")]
    public async Task<ActionResult<ProductoDto>> ActualizarPrecio([FromRoute] int id, [FromBody] ActualizarPrecioDto dto, CancellationToken ct)
    {
        try
        {
            var actualizado = await _service.ActualizarPrecioAsync(id, dto, ct);
            return actualizado is null ? NotFound() : Ok(actualizado);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "precio"] = new[] { ex.Message }
            }) { Status = StatusCodes.Status400BadRequest });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar([FromRoute] int id, CancellationToken ct)
    {
        var ok = await _service.EliminarAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}