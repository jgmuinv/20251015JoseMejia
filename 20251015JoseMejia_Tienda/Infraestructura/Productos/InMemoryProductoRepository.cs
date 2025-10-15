using Dominio.Productos;

namespace Infraestructura.Productos;

// Repositorio en memoria para fines de prueba. Simula una llamada a SP para listar.
public class InMemoryProductoRepository : IProductoRepository
{
    private readonly Dictionary<int, Producto> _db = new();
        private int _seq = 0;

    public Task<IReadOnlyList<Producto>> ListarPorStoredProcedureAsync(string Nombre, string Descripcion, bool Eliminado, CancellationToken ct = default)
    {
        // Aquí se "simula" la ejecución de un procedimiento almacenado en BD
        IReadOnlyList<Producto> items = _db.Values.OrderBy(p => p.Nombre).ToList();
        return Task.FromResult(items);
    }

    public Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        _db.TryGetValue(id, out var prod);
        return Task.FromResult(prod);
    }

    public Task CrearAsync(Producto producto, CancellationToken ct = default)
    {
        producto.Id = ++_seq;
        _db[producto.Id] = producto;
        return Task.CompletedTask;
    }

    public Task ActualizarAsync(Producto producto, CancellationToken ct = default)
    {
        _db[producto.Id] = producto;
        return Task.CompletedTask;
    }

    public Task EliminarAsync(int id, CancellationToken ct = default)
    {
        _db.Remove(id);
        return Task.CompletedTask;
    }
}
