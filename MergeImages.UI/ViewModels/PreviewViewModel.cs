using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MergeImages.UI.Models;
using MergeImages.UI.Services;
using Avalonia.Media.Imaging;

namespace MergeImages.UI.ViewModels
{
    public partial class PreviewViewModel : ViewModelBase, IPreviewViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ICoreBridge _coreBridge;

        [ObservableProperty]
        private Bitmap? _mergedImageSource;

        [ObservableProperty]
        private ExportFormat _selectedFormat = ExportFormat.PNG;


        public bool CanExport => MergedImageSource != null;

    // Provide enum values for binding in XAML
    public ExportFormat[] AvailableExportFormats { get; } = System.Enum.GetValues<ExportFormat>();

        public PreviewViewModel(
            Bitmap mergedImage,
            INavigationService navigationService,
            IDialogService dialogService,
            ICoreBridge coreBridge)
        {
            _mergedImageSource = mergedImage;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _coreBridge = coreBridge;
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public void GoBack()
        {
            _navigationService.NavigateToMain();
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public void UpdateFormat(ExportFormat format)
        {
            SelectedFormat = format;
        }


        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public async Task SaveAsAsync()
        {
            if (!CanExport) return;

            var path = await _dialogService.ShowSaveFileDialogAsync(new Services.FileDialogOptions
            {
                Title = "Save Merged Image",
                SuggestedFileName = "merged",
                Filters = new[] { SelectedFormat.ToString().ToLowerInvariant() }
            });

            if (string.IsNullOrWhiteSpace(path)) return;

            await _coreBridge.ExportAsync(MergedImageSource!, SelectedFormat, path);
        }
    }
}
