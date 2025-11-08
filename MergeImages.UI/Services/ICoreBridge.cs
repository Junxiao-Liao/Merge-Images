using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using MergeImages.UI.Models;

namespace MergeImages.UI.Services
{
    // Minimal bridge interface to backend core for tests and VM logic
    public interface ICoreBridge
    {
        Task<Bitmap> MergeAsync(IReadOnlyList<string> orderedImagePaths, MergeOptionsViewModel options);
        Task<bool> ExportAsync(Bitmap image, ExportFormat format, int? quality, string outputPath);
    }
}
