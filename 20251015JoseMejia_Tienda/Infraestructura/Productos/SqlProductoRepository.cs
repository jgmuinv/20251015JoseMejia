using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Dominio.Productos;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infraestructura.Productos;

public class SqlProductoRepository : IProductoRepository
{
    private readonly string _connectionString;
    public SqlProductoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConn() => new SqlConnection(_connectionString);

    public async Task<IReadOnlyList<Producto>> ListarPorStoredProcedureAsync(string Nombre, string Descripcion, bool Eliminado, CancellationToken ct = default)
    {
        using var conn = CreateConn();
        var p = new DynamicParameters();
        p.Add("@Nombre", Nombre, DbType.String);
        p.Add("@Descripcion", Descripcion, DbType.String);
        p.Add("@Eliminado", Eliminado, DbType.Boolean);
        var data = await conn.QueryAsync<dynamic>("20251015JoseMejia.dbo.sp_ListaDeProductos", p, commandType: CommandType.StoredProcedure);
        var list = new List<Producto>();
        foreach (var row in data)
        {
            var prod = new Producto(
                (string)(row.Nombre ?? string.Empty),
                (string)(row.Descripcion ?? string.Empty),
                (decimal)row.PrecioBase,
                (bool)row.Eliminado,
                (string?)row.Imagen,
                (decimal?)row.PrecioConDescuento);
            prod.Id = (int)row.ProductoId;
            list.Add(prod);
        }
        return list;
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        using var conn = CreateConn();
        var sql = "SELECT TOP 1 ProductoId, Nombre, Descripcion, PrecioBase, Eliminado, Imagen, PrecioConDescuento FROM dbo.Productos WHERE Eliminado = 0 AND ProductoId = @id";
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { id });
        if (row == null) return null;
        var prod = new Producto((string)row.Nombre, (string?)row.Descripcion ?? string.Empty, (decimal)row.PrecioBase,  (bool)row.Eliminado, (string?)row.Imagen, (decimal?)row.PrecioConDescuento ?? (decimal?)row.PrecioBase)
        {
            Id = (int)row.ProductoId
        };
        return prod;
    }

    public async Task CrearAsync(Producto producto, CancellationToken ct = default)
    {
        using var conn = CreateConn();
        var sql = @"INSERT INTO dbo.Productos (Nombre, Descripcion, PrecioBase, Imagen, Eliminado, FechaHoraCreacion)
                    VALUES (@Nombre, @Descripcion, @PrecioBase, @Imagen, 0, GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() as int);";
        var newId = await conn.ExecuteScalarAsync<int>(sql, new
        {
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            PrecioBase = producto.PrecioBase,
            Imagen = producto.ImagenUrl
        });
        producto.Id = newId;
    }

    public async Task ActualizarAsync(Producto producto, CancellationToken ct = default)
    {
        string imagen = "";
        using var conn = CreateConn();
        if (producto.ImagenUrl != null) imagen = ", Imagen=@Imagen ";
        var sql = @$"UPDATE dbo.Productos SET Nombre=@Nombre, Descripcion=@Descripcion, PrecioBase=@PrecioBase, PrecioConDescuento=@PrecioConDescuento {imagen}, FechaHoraEdicion=GETDATE()
                    WHERE ProductoId=@Id";
        await conn.ExecuteAsync(sql, new
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            PrecioBase = producto.PrecioBase,
            PrecioConDescuento = producto.PrecioConDescuento,
            Imagen = producto.ImagenUrl
        });
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        using var conn = CreateConn();
        var sql = "UPDATE dbo.Productos SET Eliminado = 1, FechaHoraEdicion=GETDATE() WHERE ProductoId=@Id";
        await conn.ExecuteAsync(sql, new { Id = id });
    }
}
