using Dominio.Productos;

namespace Dominio.Productos;

public interface IProductoRepository
{
    Task<IReadOnlyList<Producto>> ListarPorStoredProcedureAsync(string Nombre, string Descripcion, bool Eliminado, CancellationToken ct = default);
    Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task CrearAsync(Producto producto, CancellationToken ct = default);
    Task ActualizarAsync(Producto producto, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}
