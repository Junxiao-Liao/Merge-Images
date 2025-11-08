using System.Threading.Tasks;
using MergeImages.UI.Models;

namespace MergeImages.UI.ViewModels
{
    public interface IMainViewModel
    {
        Task SelectImagesAsync();
        Task AddImagesAsync(string[] filePaths);
        void RemoveImage(System.Guid id);
        void ClearAllImages();
        void ReorderImages(int oldIndex, int newIndex);
        Task MergeAsync();
        void UpdateMergeOptions(MergeOptionsViewModel options);
    }
}
