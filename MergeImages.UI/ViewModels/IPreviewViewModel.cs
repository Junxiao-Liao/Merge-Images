using System.Threading.Tasks;
using MergeImages.UI.Models;

namespace MergeImages.UI.ViewModels
{
    public interface IPreviewViewModel
    {
        void GoBack();
        void UpdateFormat(ExportFormat format);
        void UpdateQuality(int quality);
        Task SaveAsAsync();
    }
}
