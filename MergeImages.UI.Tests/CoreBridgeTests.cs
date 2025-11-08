using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using MergeImages.UI.Models;
using MergeImages.UI.Services;
using Xunit;

namespace MergeImages.UI.Tests
{
    public class CoreBridgeTests
    {
        private string CreateTempImage(int width, int height)
        {
            var path = Path.Combine(Path.GetTempPath(), $"merge_test_{Guid.NewGuid():N}.png");
            using (var rtb = new RenderTargetBitmap(new Avalonia.PixelSize(width, height)))
            {
                using var fs = File.OpenWrite(path);
                rtb.Save(fs);
            }
            return path;
        }

        [Fact(Skip="Disabled: Requires Avalonia rendering + ImageSharp load path; skipping in CI.")]
        public void MergeAsync_Vertical_CombinesAllHeights() { }

        [Fact(Skip="Disabled: Requires Avalonia rendering + ImageSharp load path; skipping in CI.")]
        public void MergeAsync_Horizontal_CombinesAllWidths() { }
    }
}