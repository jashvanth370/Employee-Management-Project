namespace CrudApi.ImageAdd
{
    public sealed class PhotoStorageOptions
    {
        public string RootPath { get; set; } = "";
        public string BaseUrl { get; set; } = "";
        public long MaxBytes { get; set; } = 20 * 1024 * 1024;
        public string[] AllowedExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    }
}
