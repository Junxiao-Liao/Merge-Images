namespace MergeImages.Core

open System
open System.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing
open Types

module MergeEngine =
    let calculateMergedDimensions (images: (Guid * Image) list) (options: MergeOptions) : ImageDimensions =
        let count = images.Length
        if count = 0 then { Width = 0; Height = 0 }
        else
            match options.Direction with
            | MergeDirection.Vertical ->
                let width = images |> List.maxBy (fun (_,i) -> i.Width) |> snd |> fun i -> i.Width
                let height = (images |> List.sumBy (fun (_,i) -> i.Height)) + options.Spacing * (count - 1)
                { Width = width; Height = height }
            | MergeDirection.Horizontal ->
                let height = images |> List.maxBy (fun (_,i) -> i.Height) |> snd |> fun i -> i.Height
                let width = (images |> List.sumBy (fun (_,i) -> i.Width)) + options.Spacing * (count - 1)
                { Width = width; Height = height }

    let private colorToRgba32 (c: Color) = Rgba32(c.R, c.G, c.B, c.A)

    let private backgroundColor (bg: BackgroundFill) =
        match bg with
        | BackgroundFill.Transparent -> Rgba32(0uy,0uy,0uy,0uy)
        | BackgroundFill.Solid c -> colorToRgba32 c

    let mergeImages (request: MergeRequest) : MergeResult =
        match Validation.validateMergeRequest request with
        | Result.Error errs ->
            let msg = errs |> List.map (fun e -> e.ToString()) |> String.concat "; "
            MergeResult.Error msg
        | Ok req ->
            match ImageLoader.loadImages req.Images with
            | Result.Error e -> MergeResult.Error e
            | Ok loaded ->
                try
                    let dims = calculateMergedDimensions loaded req.Options
                    use canvas = new Image<Rgba32>(dims.Width, dims.Height, backgroundColor req.Options.Background)
                    let mutable x = 0
                    let mutable y = 0
                    for (_, img) in loaded do
                        canvas.Mutate(fun ctx -> ctx.DrawImage(img, Point(x,y), 1.0f) |> ignore)
                        match req.Options.Direction with
                        | MergeDirection.Vertical ->
                            y <- y + img.Height + req.Options.Spacing
                        | MergeDirection.Horizontal ->
                            x <- x + img.Width + req.Options.Spacing
                    use ms = new MemoryStream()
                    canvas.SaveAsPng(ms)
                    ImageLoader.disposeImages loaded
                    MergeResult.Success(ms.ToArray(), dims)
                with ex ->
                    ImageLoader.disposeImages loaded
                    MergeResult.Error ex.Message

    let mergeImagesAsync (request: MergeRequest) : Async<MergeResult> =
        async { return mergeImages request }
