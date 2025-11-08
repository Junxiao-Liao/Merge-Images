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
        let opts = { Direction = MergeDirection.Vertical; Spacing = 10; Background = BackgroundFill.Transparent }
        let dims = calculateMergedDimensions imgs opts
        Assert.Equal(100, dims.Width)
        Assert.Equal(50 + 70 + 10, dims.Height)

    [<Fact>]
    let ``calculateMergedDimensions horizontal`` () =
        use i1 = mkImage 100 50 :> Image
        use i2 = mkImage 80 70 :> Image
        let imgs = [ (Guid.NewGuid(), i1); (Guid.NewGuid(), i2) ]
        let opts = { Direction = MergeDirection.Horizontal; Spacing = 10; Background = BackgroundFill.Transparent }
        let dims = calculateMergedDimensions imgs opts
        Assert.Equal(100 + 80 + 10, dims.Width)
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
            Options = { Direction = MergeDirection.Horizontal; Spacing = 5; Background = BackgroundFill.Transparent }
        }
        match MergeImages.Core.MergeEngine.mergeImages req with
        | MergeImages.Core.Types.MergeResult.Success (bytes, dims) ->
            Assert.True(bytes.Length > 0)
            Assert.Equal(40 + 20 + 5, dims.Width)
            Assert.Equal(30, dims.Height)
        | MergeImages.Core.Types.MergeResult.Error e -> Assert.Fail($"Unexpected error: {e}")
