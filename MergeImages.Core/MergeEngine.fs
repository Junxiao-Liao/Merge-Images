namespace MergeImages.Core

open System
open System.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing
open Types

module MergeEngine =
    // Helper record for easier C# interop without needing to deconstruct DU cases
    type MergeSuccess = { ImageData: byte array; Dimensions: ImageDimensions }
    let calculateMergedDimensions (images: (Guid * Image) list) (options: MergeOptions) : ImageDimensions =
        let count = images.Length
        if count = 0 then { Width = 0; Height = 0 }
        else
            match options.Direction with
            | MergeDirection.Vertical ->
                // All images will be scaled to match the maximum width
                let maxWidth = images |> List.maxBy (fun (_,i) -> i.Width) |> snd |> fun i -> i.Width
                // Calculate total height after scaling each image proportionally
                let totalHeight = 
                    images 
                    |> List.sumBy (fun (_,img) ->
                        if img.Width = maxWidth then img.Height
                        else 
                            let scale = float maxWidth / float img.Width
                            int (float img.Height * scale)
                    )
                { Width = maxWidth; Height = totalHeight }
            | MergeDirection.Horizontal ->
                // All images will be scaled to match the maximum height
                let maxHeight = images |> List.maxBy (fun (_,i) -> i.Height) |> snd |> fun i -> i.Height
                // Calculate total width after scaling each image proportionally
                let totalWidth = 
                    images 
                    |> List.sumBy (fun (_,img) ->
                        if img.Height = maxHeight then img.Width
                        else 
                            let scale = float maxHeight / float img.Height
                            int (float img.Width * scale)
                    )
                { Width = totalWidth; Height = maxHeight }

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
                    let bg = backgroundColor req.Options.Background

                    // Build normalized images: scale images to match largest dimension in merge direction
                    let normalized : (Guid * Image<Rgba32>) list =
                        match req.Options.Direction with
                        | MergeDirection.Vertical ->
                            // Scale all images to match the maximum width, preserving aspect ratio
                            let maxWidth = loaded |> List.maxBy (fun (_,i) -> i.Width) |> snd |> fun i -> i.Width
                            loaded
                            |> List.map (fun (id, img) ->
                                if img.Width = maxWidth then
                                    (id, img.CloneAs<Rgba32>())
                                else
                                    // Calculate new height preserving aspect ratio
                                    let scale = float maxWidth / float img.Width
                                    let newHeight = int (float img.Height * scale)
                                    let scaled = img.CloneAs<Rgba32>()
                                    scaled.Mutate(fun ctx -> 
                                        ctx.Resize(maxWidth, newHeight, KnownResamplers.Lanczos3) |> ignore)
                                    (id, scaled)
                            )
                        | MergeDirection.Horizontal ->
                            // Scale all images to match the maximum height, preserving aspect ratio
                            let maxHeight = loaded |> List.maxBy (fun (_,i) -> i.Height) |> snd |> fun i -> i.Height
                            loaded
                            |> List.map (fun (id, img) ->
                                if img.Height = maxHeight then
                                    (id, img.CloneAs<Rgba32>())
                                else
                                    // Calculate new width preserving aspect ratio
                                    let scale = float maxHeight / float img.Height
                                    let newWidth = int (float img.Width * scale)
                                    let scaled = img.CloneAs<Rgba32>()
                                    scaled.Mutate(fun ctx -> 
                                        ctx.Resize(newWidth, maxHeight, KnownResamplers.Lanczos3) |> ignore)
                                    (id, scaled)
                            )

                    // Calculate final canvas size using normalized dimensions
                    let dims =
                        match req.Options.Direction with
                        | MergeDirection.Vertical ->
                            let width = normalized |> List.head |> snd |> fun i -> i.Width
                            let height = normalized |> List.sumBy (fun (_,i) -> i.Height)
                            { Width = width; Height = height }
                        | MergeDirection.Horizontal ->
                            let height = normalized |> List.head |> snd |> fun i -> i.Height
                            let width = normalized |> List.sumBy (fun (_,i) -> i.Width)
                            { Width = width; Height = height }

                    use canvas = new Image<Rgba32>(dims.Width, dims.Height, bg)
                    let mutable x = 0
                    let mutable y = 0
                    for (_, img) in normalized do
                        canvas.Mutate(fun ctx -> ctx.DrawImage(img, Point(x,y), 1.0f) |> ignore)
                        match req.Options.Direction with
                        | MergeDirection.Vertical ->
                            y <- y + img.Height
                        | MergeDirection.Horizontal ->
                            x <- x + img.Width

                    use ms = new MemoryStream()
                    canvas.SaveAsPng(ms)

                    // Dispose both original and normalized images
                    ImageLoader.disposeImages loaded
                    normalized |> List.iter (fun (_,i) -> (i :> Image).Dispose())

                    MergeResult.Success(ms.ToArray(), dims)
                with ex ->
                    ImageLoader.disposeImages loaded
                    MergeResult.Error ex.Message

    let mergeImagesAsync (request: MergeRequest) : Async<MergeResult> =
        async { return mergeImages request }

    // Flatten MergeResult to a simpler Result for C# consumption
    let mergeImagesRaw (request: MergeRequest) : Result<MergeSuccess, string> =
        match mergeImages request with
        | MergeResult.Success (bytes, dims) -> Result.Ok { ImageData = bytes; Dimensions = dims }
        | MergeResult.Error e -> Result.Error e

    // Return only the merged image bytes or throw on error (for simplest C# interop)
    let mergeImagesBytes (request: MergeRequest) : byte array =
        match mergeImages request with
        | MergeResult.Success (bytes, _) -> bytes
        | MergeResult.Error e -> failwith e
