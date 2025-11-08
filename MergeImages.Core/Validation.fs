namespace MergeImages.Core

open System
open System.IO

module Validation =
    open Types

    let private supportedExtensions =
        set [ ".png"; ".jpg"; ".jpeg"; ".webp"; ".bmp"; ".tiff"; ".tif" ]

    /// Validate an image file path by extension and existence.
    let validateImageFile (filePath: string) : Result<unit, Types.ValidationError> =
        if String.IsNullOrWhiteSpace filePath then
            Result.Error (Types.ValidationError.InvalidImageFormat filePath)
        else
            let ext = Path.GetExtension filePath
            if not (supportedExtensions.Contains(ext.ToLowerInvariant())) then
                Result.Error (Types.ValidationError.InvalidImageFormat filePath)
            elif not (File.Exists filePath) then
                Result.Error (Types.ValidationError.FileNotFound filePath)
            else Ok ()

    let validateMergeRequest (req: Types.MergeRequest) : Result<Types.MergeRequest, Types.ValidationError list> =
        let errors = ResizeArray<Types.ValidationError>()
        match req.Images with
        | [] -> errors.Add Types.ValidationError.EmptyImageList
        | _ -> ()
        if req.Options.Spacing < 0 then
            errors.Add (Types.ValidationError.UnsupportedFormat "Negative spacing not allowed")
        if errors.Count = 0 then Ok req else Result.Error (List.ofSeq errors)

    let validateExportRequest (req: Types.ExportRequest) : Result<Types.ExportRequest, Types.ValidationError> =
        if req.ImageData = null || req.ImageData.Length = 0 then
            Result.Error (Types.ValidationError.UnsupportedFormat "ImageData is empty")
        elif String.IsNullOrWhiteSpace req.OutputPath then
            Result.Error (Types.ValidationError.UnsupportedFormat "OutputPath is empty")
        else
            Ok req
