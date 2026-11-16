using Microsoft.Extensions.FileProviders;

namespace KeinishkataKushta.Infrastructure;

public static class RoomPhoto
{
    // An association alone does not guarantee the uploaded file is still present.
    // This checks storage availability, not authenticity or decodability of the image.
    public static bool Exists(IFileProvider webRoot, int roomId, string? fileName)
    {
        if (roomId <= 0 || string.IsNullOrWhiteSpace(fileName)
            || fileName.IndexOfAny(new[] { '/', '\\', ':' }) >= 0
            || fileName is "." or "..") return false;

        var file = webRoot.GetFileInfo($"uploads/rooms/{roomId}/{fileName}");
        return file.Exists && !file.IsDirectory && file.Length > 0;
    }
}
