using Aplicacion.Productos;
using Infraestructura.Productos;
using FluentAssertions;

namespace Pruebas
{
    public class UnitTest1
    {
        [Fact]
        public async Task CrearProducto_PrecioInvalido_DebeFallar()
        {
            var repo = new InMemoryProductoRepository();
            var svc = new ProductosService(repo);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await svc.CrearAsync(new CrearProductoDto("A", "desc", 0m, null, null));
            });
        }

        [Fact]
        public async Task ActualizarPrecio_ConDescuentoOpcional_Valido()
        {
            var repo = new InMemoryProductoRepository();
            var svc = new ProductosService(repo);
            var p = await svc.CrearAsync(new CrearProductoDto("Prod", "desc", 100m, null, null));

            var actualizado = await svc.ActualizarPrecioAsync(p.Id, new ActualizarPrecioDto(120m, 90m));

            actualizado!.PrecioBase.Should().Be(120m);
            actualizado.PrecioConDescuento.Should().Be(90m);
        }

        [Fact]
        public async Task Listar_UsaProcedimientoAlmacenado_SimulaOk()
        {
            var repo = new InMemoryProductoRepository();
            var svc = new ProductosService(repo);
            await svc.CrearAsync(new CrearProductoDto("P1", "d", 10m, null, null));
            var items = await svc.ListarAsync("P", "d", false);
            items.Should().NotBeEmpty();
        }
        
        [Fact]
        public async Task ObtenerPorId_Existente_DevuelveProducto()
        {
            var repo = new InMemoryProductoRepository();
            var svc = new ProductosService(repo);
            var creado = await svc.CrearAsync(new CrearProductoDto("P1", "d", 10m, null, null));

            var res = await svc.ObtenerPorIdAsync(creado.Id);
            Assert.NotNull(res);
            Assert.Equal(creado.Id, res!.Id);
        }
    }
}
