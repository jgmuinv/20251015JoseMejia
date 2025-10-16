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
    private readonly Microsoft.Extensions.Logging.ILogger<ProductosController> _logger;

    public ProductosController(IProductosService service, Api.Services.IFileStorage files, Microsoft.Extensions.Logging.ILogger<ProductosController> logger)
    {
        _service = service;
        _files = files;
        _logger = logger;
    }

    [HttpPost("imagen")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<object>> SubirImagen([FromForm] IFormFile archivo, CancellationToken ct)
    {
        try
        {
            if (archivo == null || archivo.Length == 0) return BadRequest("Archivo vacío");
            var rutaRelativa = await _files.SaveProductImageAsync(archivo, ct);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var ruta = rutaRelativa.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? rutaRelativa
                : baseUrl + (rutaRelativa.StartsWith("/") ? rutaRelativa : "/" + rutaRelativa);
            return Ok(new { ruta });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir imagen del producto");
            return Problem("Error interno al subir la imagen", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductoDto>>> Listar(CancellationToken ct, string Nombre = "", string Descripcion = "", bool Eliminado = false )
    {
        try
        {
            var lista = await _service.ListarAsync(Nombre, Descripcion, Eliminado, ct);
            return Ok(lista);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar productos");
            return Problem("Error interno al listar productos", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Crear([FromBody] CrearProductoDto dto, CancellationToken ct)
    {
        try
        {
            var creado = await _service.CrearAsync(dto, ct);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "campo"] = new[] { ex.Message }
            }) { Status = StatusCodes.Status400BadRequest });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producto");
            return Problem("Error interno al crear producto", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            if (id <= 0) return BadRequest("El id debe ser mayor a cero");
            var item = await _service.ObtenerPorIdAsync(id, ct);
            return item is null ? NotFound() : Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto {Id}", id);
            return Problem("Error interno al obtener el producto", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductoDto>> Editar([FromRoute] int id, [FromBody] EditarProductoDto dto, CancellationToken ct)
    {
        try
        {
            var actualizado = await _service.EditarAsync(id, dto, ct);
            return actualizado is null ? NotFound() : Ok(actualizado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar producto {Id}", id);
            return Problem("Error interno al editar el producto", statusCode: StatusCodes.Status500InternalServerError);
        }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar precio del producto {Id}", id);
            return Problem("Error interno al actualizar el precio", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar([FromRoute] int id, CancellationToken ct)
    {
        try
        {
            var ok = await _service.EliminarAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar producto {Id}", id);
            return Problem("Error interno al eliminar el producto", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}