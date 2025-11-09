namespace MergeImages.Core

open System
open System.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Formats.Jpeg
open SixLabors.ImageSharp.Formats.Webp
open SixLabors.ImageSharp.Formats.Png
open SixLabors.ImageSharp.Formats.Bmp
open SixLabors.ImageSharp.Formats.Tiff
open Types

module ImageExporter =
    let encodeImage (imageData: byte array) (format: ImageFormat) : Result<byte array, string> =
        try
            use input = new MemoryStream(imageData)
            use image = Image.Load input
            use output = new MemoryStream()
            match format with
            | ImageFormat.PNG -> image.Save(output, PngEncoder())
            | ImageFormat.JPEG -> image.Save(output, JpegEncoder(Quality=95))
            | ImageFormat.WEBP -> image.Save(output, WebpEncoder(Quality=95))
            | ImageFormat.BMP -> image.Save(output, BmpEncoder())
            | ImageFormat.TIFF -> image.Save(output, TiffEncoder())
            Ok (output.ToArray())
        with ex -> Result.Error ex.Message

    let exportImage (request: ExportRequest) : ExportResult =
        match Validation.validateExportRequest request with
        | Result.Error e -> ExportResult.Error (string e)
        | Ok req ->
            match encodeImage req.ImageData req.Format with
            | Result.Error e -> ExportResult.Error e
            | Ok encoded ->
                try
                    File.WriteAllBytes(req.OutputPath, encoded)
                    ExportResult.Success req.OutputPath
                with ex -> ExportResult.Error ex.Message

    let exportImageAsync (request: ExportRequest) : Async<ExportResult> =
        async { return exportImage request }

    let getSupportedFormats () = [ ImageFormat.PNG; ImageFormat.JPEG; ImageFormat.WEBP; ImageFormat.BMP; ImageFormat.TIFF ]

