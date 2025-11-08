using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia;
using MergeImages.UI.Models;
using MergeImages.UI.Services;
using MergeImages.UI.ViewModels;
using Moq;
using Xunit;

namespace MergeImages.UI.Tests
{
    public class MainViewModelTests
    {
        // AvaloniaTestApp module initializer sets up headless environment.
        private static MainViewModel CreateVm(
            out Mock<IDialogService> dialog,
            out Mock<IThumbnailService> thumbnails,
            out Mock<INavigationService> nav,
            out Mock<ICoreBridge> core)
        {
            dialog = new Mock<IDialogService>();
            thumbnails = new Mock<IThumbnailService>();
            nav = new Mock<INavigationService>();
            core = new Mock<ICoreBridge>();

            thumbnails
                .Setup(t => t.GenerateThumbnailAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((Bitmap?)null);

            return new MainViewModel(dialog.Object, thumbnails.Object, nav.Object, core.Object);
        }

        [Fact]
        public async Task SelectImagesAsync_populates_ImageCards_in_order()
        {
            // Arrange
            var vm = CreateVm(out var dialog, out var thumbnails, out var nav, out var core);
            var files = new[] { "a.png", "b.jpg", "c.webp" };
            dialog.Setup(d => d.ShowOpenFileDialogAsync(It.IsAny<FileDialogOptions>())).ReturnsAsync(files);

            // Act
            await vm.SelectImagesAsync();

            // Assert
            Assert.Equal(3, vm.ImageCards.Count);
            Assert.True(vm.CanMerge);
            Assert.Equal(files, vm.ImageCards.OrderBy(c => c.Order).Select(c => c.FilePath).ToArray());
        }

        [Fact]
        public async Task MergeAsync_calls_core_with_ordered_paths_and_navigates()
        {
            // Arrange
            var vm = CreateVm(out var dialog, out var thumbnails, out var nav, out var core);
            var files = new[] { "1.png", "2.png" };
            dialog.Setup(d => d.ShowOpenFileDialogAsync(It.IsAny<FileDialogOptions>())).ReturnsAsync(files);
            await vm.SelectImagesAsync();

            // Minimal 1x1 PNG bytes to avoid platform render interface requirement
            byte[] png1x1 = new byte[] {
                0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,
                0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x01,0x08,0x06,0x00,0x00,0x00,0x1F,0x15,0xC4,
                0x89,0x00,0x00,0x00,0x0A,0x49,0x44,0x41,0x54,0x78,0x9C,0x63,0x00,0x01,0x00,0x00,
                0x05,0x00,0x01,0x0D,0x0A,0x2D,0xB4,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,
                0x42,0x60,0x82 };
            Bitmap stubBitmap;
            using (var ms = new System.IO.MemoryStream(png1x1))
            {
                stubBitmap = new Bitmap(ms);
            }
            core
                .Setup(c => c.MergeAsync(It.IsAny<string[]>(), It.IsAny<MergeOptionsViewModel>()))
                .ReturnsAsync(stubBitmap);

            // Act
            await vm.MergeAsync();

            // Assert
            core.Verify(c => c.MergeAsync(
                It.Is<string[]>(arr => arr.SequenceEqual(files)),
                It.IsAny<MergeOptionsViewModel>()
            ), Times.Once);

            nav.Verify(n => n.NavigateToPreview(It.IsAny<Bitmap>()), Times.Once);
        }
    }
}
