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
    public class PreviewViewModelTests
    {
        // AvaloniaTestApp module initializer sets up headless environment.

        private static PreviewViewModel CreateVm(
            out Mock<INavigationService> nav,
            out Mock<IDialogService> dialog,
            out Mock<ICoreBridge> core)
        {
            nav = new Mock<INavigationService>();
            dialog = new Mock<IDialogService>();
            core = new Mock<ICoreBridge>();

            byte[] png1x1 = new byte[] {
                0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,
                0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x01,0x08,0x06,0x00,0x00,0x00,0x1F,0x15,0xC4,
                0x89,0x00,0x00,0x00,0x0A,0x49,0x44,0x41,0x54,0x78,0x9C,0x63,0x00,0x01,0x00,0x00,
                0x05,0x00,0x01,0x0D,0x0A,0x2D,0xB4,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,
                0x42,0x60,0x82 };
            Bitmap bmp;
            using (var ms = new System.IO.MemoryStream(png1x1))
            {
                bmp = new Bitmap(ms);
            }
            return new PreviewViewModel(bmp, nav.Object, dialog.Object, core.Object);
        }

        [Fact]
        public void Initialization_sets_bitmap_and_can_export_true()
        {
            var vm = CreateVm(out var nav, out var dialog, out var core);
            Assert.NotNull(vm.MergedImageSource);
            Assert.True(vm.CanExport);
        }

        [Fact]
        public async Task SaveAsAsync_calls_export_for_lossy_formats()
        {
            var vm = CreateVm(out var nav, out var dialog, out var core);
            vm.UpdateFormat(ExportFormat.JPEG);
            dialog.Setup(d => d.ShowSaveFileDialogAsync(It.IsAny<FileDialogOptions>())).ReturnsAsync("out.jpg");
            core.Setup(c => c.ExportAsync(vm.MergedImageSource!, ExportFormat.JPEG, "out.jpg")).ReturnsAsync(true);

            await vm.SaveAsAsync();

            core.Verify(c => c.ExportAsync(vm.MergedImageSource!, ExportFormat.JPEG, "out.jpg"), Times.Once);
        }

        [Fact]
        public async Task SaveAsAsync_exports_lossless_format()
        {
            var vm = CreateVm(out var nav, out var dialog, out var core);
            vm.UpdateFormat(ExportFormat.PNG); // lossless
            dialog.Setup(d => d.ShowSaveFileDialogAsync(It.IsAny<FileDialogOptions>())).ReturnsAsync("out.png");
            core.Setup(c => c.ExportAsync(vm.MergedImageSource!, ExportFormat.PNG, "out.png")).ReturnsAsync(true);

            await vm.SaveAsAsync();

            core.Verify(c => c.ExportAsync(vm.MergedImageSource!, ExportFormat.PNG, "out.png"), Times.Once);
        }

        [Fact]
        public void GoBack_navigates_to_main()
        {
            var vm = CreateVm(out var nav, out var dialog, out var core);
            vm.GoBack();
            nav.Verify(n => n.NavigateToMain(), Times.Once);
        }
    }
}
