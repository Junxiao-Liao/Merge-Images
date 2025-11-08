namespace MergeImages.Core.Tests

open System
open System.IO
open Xunit
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open MergeImages.Core.Types
open MergeImages.Core.ImageExporter

module ImageExporterTests =
    let private makeBaseImageBytes () =
        use img = new Image<Rgba32>(2,2)
        use ms = new MemoryStream()
        img.SaveAsPng(ms)
        ms.ToArray()

    [<Fact>]
    let ``encodeImage returns bytes for all formats`` () =
        let formats = [ ImageFormat.PNG; ImageFormat.JPEG; ImageFormat.WEBP; ImageFormat.BMP; ImageFormat.TIFF ]
        let src = makeBaseImageBytes()
        for f in formats do
            match encodeImage src f with
            | Ok outBytes -> Assert.True(outBytes.Length > 0)
            | Result.Error e -> Assert.Fail($"Encoding failed for {f}: {e}")

    [<Fact>]
    let ``getDefaultQuality returns Some for lossy formats`` () =
        Assert.Equal(Some 95, getDefaultQuality ImageFormat.JPEG)
        Assert.Equal(Some 95, getDefaultQuality ImageFormat.WEBP)
        Assert.Equal(None, getDefaultQuality ImageFormat.PNG)
