using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using MergeImages.UI.ViewModels;
using MergeImages.UI.Models;

namespace MergeImages.UI.Views
{
    public partial class MainWindow : Window
    {
        private const string DragDataFormat = "application/x-imagecard-index";
        private int _draggedIndex = -1;
        private Border? _lastHighlightedBorder;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnDragOver(object? sender, DragEventArgs e)
        {
            // Handle file drops from external sources
            if (e.DataTransfer?.Contains(DataFormat.File) == true)
            {
                e.DragEffects = DragDropEffects.Copy;
                e.Handled = true;
                return;
            }

            // Handle internal card reordering
            if (_draggedIndex >= 0)
            {
                e.DragEffects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private async void OnDrop(object? sender, DragEventArgs e)
        {
            if (e.DataTransfer?.Contains(DataFormat.File) != true) return;

            // Retrieve files from the legacy IDataObject API (recommended by Avalonia for desktop); suppress obsolete warning for the property access only.
#pragma warning disable CS0618
            var storageItems = e.Data.GetFiles() ?? System.Array.Empty<Avalonia.Platform.Storage.IStorageItem>();
#pragma warning restore CS0618
            var fileArray = storageItems
                .OfType<IStorageFile>()
                .Select(f => f.TryGetLocalPath())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p!)
                .ToArray();
            if (fileArray.Length == 0) return;

            if (DataContext is IMainViewModel vm)
            {
                await vm.AddImagesAsync(fileArray);
            }
        }

        private void OnCardPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Border border) return;
            if (border.DataContext is not ImageCardViewModel card) return;
            if (DataContext is not MainViewModel vm) return;

            // Only start drag if pressed on the left side (drag handle area)
            var point = e.GetPosition(border);
            if (point.X > 30) return; // Only drag from the handle area

            _draggedIndex = vm.ImageCards.IndexOf(card);
            if (_draggedIndex < 0) return;

            // Start drag operation - we track the dragged index in the field
            var data = new DataTransfer();
            _ = DragDrop.DoDragDropAsync(e, data, DragDropEffects.Move);
            e.Handled = true;
        }

        private void OnCardDragEnter(object? sender, DragEventArgs e)
        {
            if (sender is not Border border) return;
            if (_draggedIndex < 0) return; // Not dragging a card

            // Highlight the drop target
            border.BorderBrush = Brushes.DodgerBlue;
            border.BorderThickness = new Thickness(2);
            _lastHighlightedBorder = border;

            e.DragEffects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void OnCardDragLeave(object? sender, DragEventArgs e)
        {
            if (sender is not Border border) return;
            if (_draggedIndex < 0) return;

            // Remove highlight
            border.BorderBrush = new SolidColorBrush(Color.Parse("#444"));
            border.BorderThickness = new Thickness(1);

            if (_lastHighlightedBorder == border)
                _lastHighlightedBorder = null;

            e.Handled = true;
        }

        private void OnCardDrop(object? sender, DragEventArgs e)
        {
            if (sender is not Border border) return;
            if (_draggedIndex < 0) return;
            if (border.DataContext is not ImageCardViewModel targetCard) return;
            if (DataContext is not MainViewModel vm) return;

            var targetIndex = vm.ImageCards.IndexOf(targetCard);
            if (targetIndex < 0) return;

            // Perform the reorder
            vm.ReorderImages(_draggedIndex, targetIndex);

            // Remove highlight and reset state
            border.BorderBrush = new SolidColorBrush(Color.Parse("#444"));
            border.BorderThickness = new Thickness(1);
            _lastHighlightedBorder = null;
            _draggedIndex = -1;

            e.Handled = true;
        }
    }
}