namespace MergeImages.Core

open System

module Types =
    type ImageFormat =
        | PNG
        | JPEG
        | WEBP
        | BMP
        | TIFF

    type MergeDirection =
        | Vertical
        | Horizontal

    type Color = {
        R: byte
        G: byte
        B: byte
        A: byte
    }

    type BackgroundFill =
        | Transparent
        | Solid of Color

    type MergeOptions = {
        Direction: MergeDirection
        Spacing: int
        Background: BackgroundFill
    }

    type ImageSource = {
        Id: Guid
        FilePath: string
        Order: int
    }

    type MergeRequest = {
        Images: ImageSource list
        Options: MergeOptions
    }

    type ExportRequest = {
        ImageData: byte array
        Format: ImageFormat
        OutputPath: string
    }

    type ImageDimensions = {
        Width: int
        Height: int
    }

    type MergeResult =
        | Success of imageData: byte array * dimensions: ImageDimensions
        | Error of errorMessage: string

    type ExportResult =
        | Success of filePath: string
        | Error of errorMessage: string

    type ValidationError =
        | EmptyImageList
        | FileNotFound of filePath: string
        | InvalidImageFormat of filePath: string
        | UnsupportedFormat of format: string
