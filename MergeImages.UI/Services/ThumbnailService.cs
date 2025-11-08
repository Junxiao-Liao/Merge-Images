using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace MergeImages.UI.Services
{
    public class ThumbnailService : IThumbnailService
    {
        public Task<Bitmap?> GenerateThumbnailAsync(string filePath, int maxWidth, int maxHeight)
        {
            try
            {
                var bmp = new Bitmap(filePath);
                // For now, return original. Scaling could be added later.
                return Task.FromResult<Bitmap?>(bmp);
            }
            catch
            {
                return Task.FromResult<Bitmap?>(null);
            }
        }
    }
}
