using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using MergeImages.UI.ViewModels;

namespace MergeImages.UI.Services
{
    public class NavigationService : INavigationService
    {
        private Window? _previewWindow;

        public void NavigateToPreview(Bitmap mergedImage)
        {
            var previewView = new Views.PreviewView
            {
                DataContext = new PreviewViewModel(mergedImage, this, new DialogService(), new CoreBridge())
            };

            _previewWindow = new Window
            {
                Title = "Preview",
                Width = 900,
                Height = 700,
                Content = previewView
            };
            _previewWindow.Show();
        }

        public void NavigateToMain()
        {
            _previewWindow?.Close();
            _previewWindow = null;
        }
    }
}
