namespace MergeImages.Core

open System
open SixLabors.ImageSharp

module ImageLoader =
    let loadImage (filePath: string) : Result<Image, string> =
        try
            Ok (Image.Load filePath)
        with ex -> Error ex.Message

    let loadImages (sources: Types.ImageSource list) : Result<(Guid * Image) list, string> =
        let loaded = ResizeArray<Guid * Image>()
        let cleanup () =
            for (_, img) in loaded do img.Dispose()
        try
            let sorted = sources |> List.sortBy (fun s -> s.Order)
            let mutable failed : string option = None
            for s in sorted do
                match loadImage s.FilePath with
                | Ok img -> loaded.Add (s.Id, img)
                | Error e -> failed <- Some e
            match failed with
            | Some e -> cleanup(); Error e
            | None -> Ok (List.ofSeq loaded)
        with ex -> cleanup(); Error ex.Message

    let disposeImages (pairs: (Guid * Image) list) : unit =
        for (_, img) in pairs do img.Dispose()
