namespace MergeImages.Core

open System
open System.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Processing
open SixLabors.ImageSharp.PixelFormats

module ThumbnailGenerator =
    let generateThumbnail (filePath: string) (maxWidth: int) (maxHeight: int) : Result<byte array, string> =
        try
            use image = Image.Load filePath
            let ratio = min (float maxWidth / float image.Width) (float maxHeight / float image.Height)
            let newW = int (float image.Width * ratio)
            let newH = int (float image.Height * ratio)
            image.Mutate(fun ctx -> ctx.Resize(newW, newH) |> ignore)
            use ms = new MemoryStream()
            image.SaveAsPng(ms)
            Ok (ms.ToArray())
        with ex -> Error ex.Message

    let generateThumbnailAsync (filePath: string) (maxWidth: int) (maxHeight: int) : Async<Result<byte array, string>> =
        async { return generateThumbnail filePath maxWidth maxHeight }
