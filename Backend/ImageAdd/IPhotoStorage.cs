namespace CrudApi.ImageAdd
{
    public interface IPhotoStorage
    {
        Task<(string url, string fileName)> SaveAsync(IFormFile file, string employeeId, CancellationToken ct = default);
        Task DeleteAsync(string employeeId, string fileName, CancellationToken ct = default);
        bool IsValidImage(IFormFile file, long maxBytes, string[] allowedExtensions);
    }
}
