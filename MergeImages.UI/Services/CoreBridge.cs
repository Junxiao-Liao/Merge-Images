using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using MergeImages.UI.Models;
using Avalonia;
using Microsoft.FSharp.Collections;
using MergeImages.Core;
using BgFill = MergeImages.Core.Types.BackgroundFill;
using CoreColor = MergeImages.Core.Types.Color;
using CoreMergeDirection = MergeImages.Core.Types.MergeDirection;
using CoreImageSource = MergeImages.Core.Types.ImageSource;
using CoreMergeOptions = MergeImages.Core.Types.MergeOptions;
using CoreMergeRequest = MergeImages.Core.Types.MergeRequest;

namespace MergeImages.UI.Services
{
    public class CoreBridge : ICoreBridge
    {
        public Task<Bitmap> MergeAsync(IReadOnlyList<string> orderedImagePaths, MergeOptionsViewModel options)
        {
            // Offload merge work to a background thread to keep UI responsive
            return Task.Run(() =>
            {
                if (orderedImagePaths == null || orderedImagePaths.Count == 0)
                {
                    var rtb = new RenderTargetBitmap(new PixelSize(1, 1));
                    return (Bitmap)rtb;
                }

                // Map UI options to F# core types
                var direction = options.Direction == MergeImages.UI.Models.MergeDirection.Vertical
                    ? CoreMergeDirection.Vertical
                    : CoreMergeDirection.Horizontal;

                BgFill bg = options.Background switch
                {
                    BackgroundColorChoice.Transparent => BgFill.Transparent,
                    BackgroundColorChoice.White => BgFill.NewSolid(new CoreColor(255, 255, 255, 255)),
                    BackgroundColorChoice.Black => BgFill.NewSolid(new CoreColor(0, 0, 0, 255)),
                    _ => BgFill.Transparent
                };

                var mergeOptions = new CoreMergeOptions(direction, options.Spacing, bg);

                var images = orderedImagePaths
                    .Select((p, idx) => new CoreImageSource(Guid.NewGuid(), p, idx));

                // Convert IEnumerable -> F# list
                var imagesList = ListModule.OfSeq(images);

                var request = new CoreMergeRequest(imagesList, mergeOptions);

                var imageBytes = MergeEngine.mergeImagesBytes(request);
                using (var ms = new MemoryStream(imageBytes))
                {
                    return new Bitmap(ms);
                }
            });
        }

        public Task<bool> ExportAsync(Bitmap image, ExportFormat format, int? quality, string outputPath)
        {
            // Placeholder: write PNG regardless
            try
            {
                using var fs = System.IO.File.OpenWrite(outputPath);
                image.Save(fs);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}
