using MediCare.Services.Interfaces;

namespace MediCare.Web.Infrastructure;

// Keeps uploaded medical documents on disk, OUTSIDE wwwroot, so they're only reachable through the
// authorized Documents/Download action. Folder: "Storage:UploadsPath" (default: <app>/App_Data/uploads).
// Files get a random name; the user's file name is only kept in the database.
public class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IConfiguration config, IWebHostEnvironment env)
    {
        var configured = config["Storage:UploadsPath"];
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.ContentRootPath, "App_Data", "uploads")
            : configured;
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Stream content, string extension)
    {
        var name = $"{Guid.NewGuid():N}{extension}";
        await using var file = File.Create(PathFor(name));
        await content.CopyToAsync(file);
        return name;
    }

    public Task<Stream?> OpenReadAsync(string storedFileName)
    {
        var path = PathFor(storedFileName);
        return Task.FromResult<Stream?>(File.Exists(path) ? File.OpenRead(path) : null);
    }

    public Task DeleteAsync(string storedFileName)
    {
        var path = PathFor(storedFileName);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    // Only plain generated names are accepted, so a stored name can never point outside the folder.
    private string PathFor(string storedFileName)
    {
        var safe = Path.GetFileName(storedFileName);
        if (string.IsNullOrEmpty(safe) || safe != storedFileName)
            throw new ArgumentException("Invalid file name.", nameof(storedFileName));
        return Path.Combine(_root, safe);
    }
}
