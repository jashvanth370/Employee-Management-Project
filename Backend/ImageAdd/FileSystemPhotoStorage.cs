using Microsoft.Extensions.Options;

namespace CrudApi.ImageAdd
{
    public sealed class FileSystemPhotoStorage : IPhotoStorage
    {
        private readonly PhotoStorageOptions _opt;
        private readonly IWebHostEnvironment _env;

        public FileSystemPhotoStorage(IOptions<PhotoStorageOptions> opt, IWebHostEnvironment env)
        {
            _opt = opt.Value; _env = env;
        }

        public bool IsValidImage(IFormFile file, long maxBytes, string[] allowedExt)
        {
            if (file == null || file.Length == 0 || file.Length > maxBytes) return false;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExt.Contains(ext))
            {
                Console.WriteLine($"Invalid extension: {ext}. Allowed: {string.Join(",", allowedExt)}");
                return false;
            }
            // Optional: quick magic number check for JPEG/PNG/WEBP for extra safety
            return true;
        }

        public async Task<(string url, string fileName)> SaveAsync(IFormFile file, string employeeId, CancellationToken ct = default)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var newName = $"{Guid.NewGuid():N}{ext}";
            var folder = Path.Combine(_env.ContentRootPath, _opt.RootPath.Replace("wwwroot/", ""), employeeId);
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, newName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream, ct);

            var url = $"{_opt.BaseUrl}/{employeeId}/{newName}".Replace("\\", "/");
            return (url, newName);
        }

        public Task DeleteAsync(string employeeId, string fileName, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(fileName)) return Task.CompletedTask;
            var folder = Path.Combine(_env.ContentRootPath, _opt.RootPath.Replace("wwwroot/", ""), employeeId);
            var path = Path.Combine(folder, fileName);
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }
    }
}
