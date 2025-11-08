using System.Threading.Tasks;
using MergeImages.UI.Models;

namespace MergeImages.UI.ViewModels
{
    public interface IMainViewModel
    {
        Task SelectImagesAsync();
        void RemoveImage(System.Guid id);
        Task MergeAsync();
        void UpdateMergeOptions(MergeOptionsViewModel options);
    }
}
