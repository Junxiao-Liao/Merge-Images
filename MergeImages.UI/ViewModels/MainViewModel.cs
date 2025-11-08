using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using MergeImages.UI.Models;
using MergeImages.UI.Services;

namespace MergeImages.UI.ViewModels
{
    public partial class MainViewModel : ViewModelBase, IMainViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IThumbnailService _thumbnailService;
        private readonly INavigationService _navigationService;
        private readonly ICoreBridge _coreBridge;

        public ObservableCollection<ImageCardViewModel> ImageCards { get; } = new();

        [ObservableProperty]
        private MergeOptionsViewModel _options = new(MergeDirection.Vertical, 0, BackgroundColorChoice.Transparent);

        // Expose enum values for binding
        public IReadOnlyList<MergeDirection> AvailableDirections { get; } = Enum.GetValues<MergeDirection>();

        // Wrapper properties for immutable record fields to enable two-way binding
        public MergeDirection Direction
        {
            get => Options.Direction;
            set
            {
                if (Options.Direction != value)
                {
                    Options = Options with { Direction = value };
                    OnPropertyChanged(nameof(Direction));
                    OnPropertyChanged(nameof(Options));
                }
            }
        }

        public int Spacing
        {
            get => Options.Spacing;
            set
            {
                if (Options.Spacing != value)
                {
                    Options = Options with { Spacing = value };
                    OnPropertyChanged(nameof(Spacing));
                    OnPropertyChanged(nameof(Options));
                }
            }
        }

        public bool CanMerge => ImageCards.Count > 1;

        public MainViewModel(
            IDialogService dialogService,
            IThumbnailService thumbnailService,
            INavigationService navigationService,
            ICoreBridge coreBridge)
        {
            _dialogService = dialogService;
            _thumbnailService = thumbnailService;
            _navigationService = navigationService;
            _coreBridge = coreBridge;
        }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    public async Task SelectImagesAsync()
        {
            var files = await _dialogService.ShowOpenFileDialogAsync(new Services.FileDialogOptions
            {
                Title = "Select Images",
                AllowMultiple = true,
                Filters = new[] { ".png", ".jpg", ".jpeg", ".webp", ".bmp", ".tiff" }
            });

            if (files == null || files.Length == 0) return;

            int baseOrder = ImageCards.Count;
            int index = 0;
            foreach (var file in files)
            {
                Bitmap? thumb = await _thumbnailService.GenerateThumbnailAsync(file, 200, 200);
                ImageCards.Add(new ImageCardViewModel(Guid.NewGuid(), file, thumb, baseOrder + index));
                index++;
            }

            OnPropertyChanged(nameof(CanMerge));
        }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    public void RemoveImage(Guid id)
        {
            var item = ImageCards.FirstOrDefault(x => x.Id == id);
            if (item is not null)
            {
                ImageCards.Remove(item);
                // Reassign order to keep it sequential
                for (int i = 0; i < ImageCards.Count; i++)
                {
                    var ic = ImageCards[i];
                    ImageCards[i] = ic with { Order = i };
                }
                OnPropertyChanged(nameof(CanMerge));
            }
        }

        public void UpdateMergeOptions(MergeOptionsViewModel options)
        {
            Options = options;
            OnPropertyChanged(nameof(Direction));
            OnPropertyChanged(nameof(Spacing));
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public async Task MergeAsync()
        {
            if (!CanMerge) return;

            var ordered = ImageCards
                .OrderBy(c => c.Order)
                .Select(c => c.FilePath)
                .ToArray();

            var merged = await _coreBridge.MergeAsync(ordered, Options);
            _navigationService.NavigateToPreview(merged);
        }
    }
}
