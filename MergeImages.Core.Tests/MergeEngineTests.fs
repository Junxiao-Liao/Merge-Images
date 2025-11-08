namespace MergeImages.Core.Tests

open System
open System.IO
open Xunit
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open MergeImages.Core.Types
open MergeImages.Core.MergeEngine

module MergeEngineTests =

    let private mkImage (w:int) (h:int) = new Image<Rgba32>(w, h)

    [<Fact>]
    let ``calculateMergedDimensions vertical`` () =
        use i1 = mkImage 100 50 :> Image
        use i2 = mkImage 80 70 :> Image
        let imgs = [ (Guid.NewGuid(), i1); (Guid.NewGuid(), i2) ]
        let opts = { Direction = MergeDirection.Vertical; Background = BackgroundFill.Transparent }
        let dims = calculateMergedDimensions imgs opts
        // All images scaled to maxWidth (100)
        // i1: 100x50 (unchanged)
        // i2: 80x70 -> scale to 100 wide: 100 x (70 * 100/80) = 100 x 87.5 = 100 x 87
        Assert.Equal(100, dims.Width)
        Assert.Equal(50 + 87, dims.Height)

    [<Fact>]
    let ``calculateMergedDimensions horizontal`` () =
        use i1 = mkImage 100 50 :> Image
        use i2 = mkImage 80 70 :> Image
        let imgs = [ (Guid.NewGuid(), i1); (Guid.NewGuid(), i2) ]
        let opts = { Direction = MergeDirection.Horizontal; Background = BackgroundFill.Transparent }
        let dims = calculateMergedDimensions imgs opts
        // All images scaled to maxHeight (70)
        // i1: 100x50 -> scale to 70 high: (100 * 70/50) x 70 = 140 x 70
        // i2: 80x70 (unchanged)
        Assert.Equal(140 + 80, dims.Width)
        Assert.Equal(70, dims.Height)

    [<Fact>]
    let ``mergeImages returns PNG bytes and correct dimensions`` () =
        // Prepare two temp images on disk
        use img1 = mkImage 40 30
        use img2 = mkImage 20 10
        let tmpDir = Path.Combine(Path.GetTempPath(), "mergeimages-tests")
        Directory.CreateDirectory(tmpDir) |> ignore
        let p1 = Path.Combine(tmpDir, "i1.png")
        let p2 = Path.Combine(tmpDir, "i2.png")
        img1.SaveAsPng(p1)
        img2.SaveAsPng(p2)

        let req = {
            Images = [
                { Id = Guid.NewGuid(); FilePath = p1; Order = 0 }
                { Id = Guid.NewGuid(); FilePath = p2; Order = 1 }
            ]
            Options = { Direction = MergeDirection.Horizontal; Background = BackgroundFill.Transparent }
        }
        match MergeImages.Core.MergeEngine.mergeImages req with
        | MergeImages.Core.Types.MergeResult.Success (bytes, dims) ->
            Assert.True(bytes.Length > 0)
            // All images scaled to maxHeight (30)
            // img1: 40x30 (unchanged)
            // img2: 20x10 -> scale to 30 high: (20 * 30/10) x 30 = 60 x 30
            Assert.Equal(40 + 60, dims.Width)
            Assert.Equal(30, dims.Height)
        | MergeImages.Core.Types.MergeResult.Error e -> Assert.Fail($"Unexpected error: {e}")

    [<Fact>]
    let ``mergeImages horizontal scales shorter images to match tallest`` () =
        // Create images of different heights
        use img1 = new Image<Rgba32>(100, 50, Rgba32(255uy, 0uy, 0uy, 255uy)) :> Image // Red 100x50
        use img2 = new Image<Rgba32>(80, 100, Rgba32(0uy, 255uy, 0uy, 255uy)) :> Image // Green 80x100
        let tmpDir = Path.Combine(Path.GetTempPath(), "mergeimages-tests")
        Directory.CreateDirectory(tmpDir) |> ignore
        let p1 = Path.Combine(tmpDir, "red.png")
        let p2 = Path.Combine(tmpDir, "green.png")
        img1.SaveAsPng(p1)
        img2.SaveAsPng(p2)

        let req = {
            Images = [
                { Id = Guid.NewGuid(); FilePath = p1; Order = 0 }
                { Id = Guid.NewGuid(); FilePath = p2; Order = 1 }
            ]
            Options = { Direction = MergeDirection.Horizontal; Background = BackgroundFill.Transparent }
        }
        
        match MergeImages.Core.MergeEngine.mergeImages req with
        | MergeImages.Core.Types.MergeResult.Success (bytes, dims) ->
            // All images scaled to maxHeight (100)
            // img1: 100x50 -> scale to 100 high: (100 * 100/50) x 100 = 200 x 100
            // img2: 80x100 (unchanged)
            Assert.Equal(200 + 80, dims.Width)
            Assert.Equal(100, dims.Height)
            
            // Load result and verify the scaled red image
            use resultStream = new MemoryStream(bytes)
            use result = Image.Load<Rgba32>(resultStream)
            
            // Check pixel at (100, 50) - should be red (scaled red image fills the first 200 pixels)
            let pixel = result.[100, 50]
            Assert.True(pixel.R > 200uy) // Should be predominantly red
            Assert.Equal(255uy, pixel.A)
            
            // Check pixel at (220, 50) - should be green (second image starts at x=200)
            let pixel2 = result.[220, 50]
            Assert.True(pixel2.G > 200uy) // Should be predominantly green
            Assert.Equal(255uy, pixel2.A)
        | MergeImages.Core.Types.MergeResult.Error e -> Assert.Fail($"Unexpected error: {e}")

    [<Fact>]
    let ``mergeImages vertical scales narrower images to match widest`` () =
        // Create images of different widths
        use img1 = new Image<Rgba32>(100, 50, Rgba32(255uy, 0uy, 0uy, 255uy)) :> Image // Red 100x50
        use img2 = new Image<Rgba32>(60, 40, Rgba32(0uy, 0uy, 255uy, 255uy)) :> Image // Blue 60x40
        let tmpDir = Path.Combine(Path.GetTempPath(), "mergeimages-tests")
        Directory.CreateDirectory(tmpDir) |> ignore
        let p1 = Path.Combine(tmpDir, "red-v.png")
        let p2 = Path.Combine(tmpDir, "blue-v.png")
        img1.SaveAsPng(p1)
        img2.SaveAsPng(p2)

        let req = {
            Images = [
                { Id = Guid.NewGuid(); FilePath = p1; Order = 0 }
                { Id = Guid.NewGuid(); FilePath = p2; Order = 1 }
            ]
            Options = { Direction = MergeDirection.Vertical; Background = BackgroundFill.Transparent }
        }
        
        match MergeImages.Core.MergeEngine.mergeImages req with
        | MergeImages.Core.Types.MergeResult.Success (bytes, dims) ->
            // All images scaled to maxWidth (100)
            // img1: 100x50 (unchanged)
            // img2: 60x40 -> scale to 100 wide: 100 x (40 * 100/60) = 100 x 66
            Assert.Equal(100, dims.Width)
            Assert.Equal(50 + 66, dims.Height)
            
            // Load result and verify the scaled blue image
            use resultStream = new MemoryStream(bytes)
            use result = Image.Load<Rgba32>(resultStream)
            
            // Check pixel at (50, 25) - should be red (first image)
            let pixel = result.[50, 25]
            Assert.True(pixel.R > 200uy) // Should be predominantly red
            Assert.Equal(255uy, pixel.A)
            
            // Check pixel at (50, 75) - should be blue (second scaled image starts at y=50)
            let pixel2 = result.[50, 75]
            Assert.True(pixel2.B > 200uy) // Should be predominantly blue
            Assert.Equal(255uy, pixel2.A)
        | MergeImages.Core.Types.MergeResult.Error e -> Assert.Fail($"Unexpected error: {e}")
