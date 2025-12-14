using Kidzgo.Application.Abstraction.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Kidzgo.Infrastructure.Services;

public class ImageUploader : IImageUploader
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ImageUploader(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string folderName)
    {
        // Ensure WebRootPath is set - it should point to wwwroot in output directory
        var rootPath = _env.WebRootPath;
        
        if (string.IsNullOrEmpty(rootPath))
        {
            // Fallback: create wwwroot in current directory (output directory)
            rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var uploadsFolder = Path.Combine(rootPath, folderName);
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid() + Path.GetExtension(fileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(stream);

        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = request != null
            ? $"{request.Scheme}://{request.Host}"
            : "";

        return $"{baseUrl}/{folderName}/{uniqueFileName}";
    }
}