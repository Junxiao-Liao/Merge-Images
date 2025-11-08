using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace MergeImages.UI.Services
{
    public class FileDialogOptions
    {
        public string Title { get; init; } = string.Empty;
        public string[]? Filters { get; init; }
        public bool AllowMultiple { get; init; }
        public string? SuggestedFileName { get; init; }
        public string? InitialDirectory { get; init; }
    }

    public interface IDialogService
    {
        Task<string[]?> ShowOpenFileDialogAsync(FileDialogOptions options);
        Task<string?> ShowSaveFileDialogAsync(FileDialogOptions options);
        Task<bool> ShowConfirmationAsync(string title, string message);
    }

    public interface IThumbnailService
    {
        Task<Bitmap?> GenerateThumbnailAsync(string filePath, int maxWidth, int maxHeight);
    }

    public interface INavigationService
    {
        void NavigateToPreview(Bitmap mergedImage);
        void NavigateToMain();
    }
}
