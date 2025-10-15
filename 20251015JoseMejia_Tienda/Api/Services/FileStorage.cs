using Microsoft.AspNetCore.Http;

namespace Api.Services;

public interface IFileStorage
{
    Task<string> SaveProductImageAsync(IFormFile file, CancellationToken ct = default);
}

public class FileStorage : IFileStorage
{
    private readonly string _root;
    private readonly IWebHostEnvironment _env;

    public FileStorage(IConfiguration cfg, IWebHostEnvironment env)
    {
        _env = env;
        var relative = cfg.GetSection("Images").GetValue<string>("ProductImagesFolder") ?? "wwwroot\\images\\productos";
        _root = Path.IsPathRooted(relative) ? relative : Path.Combine(env.ContentRootPath, relative);
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveProductImageAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0) throw new ArgumentException("Archivo de imagen vacío", nameof(file));
        var ext = Path.GetExtension(file.FileName);
        var name = $"prod_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(_root, name);
        await using (var stream = new FileStream(fullPath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream, ct);
        }
        // devolver ruta relativa desde wwwroot si aplica
        var wwwroot = Path.Combine(_env.ContentRootPath, "wwwroot");
        if (fullPath.StartsWith(wwwroot, StringComparison.OrdinalIgnoreCase))
        {
            var rel = fullPath.Substring(wwwroot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return "/" + rel.Replace("\\", "/");
        }
        return fullPath;
    }
}
