using System;
using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;

namespace MergeImages.UI.Models
{
    // Represents the data for a single image card in the UI
    public record ImageCardViewModel(
        Guid Id,
        string FilePath,
        Bitmap? Thumbnail,
        int Order
    );

    // Represents the direction of the merge operation
    public enum MergeDirection
    {
        Vertical,
        Horizontal
    }

    // Represents the background color choice
    public enum BackgroundColorChoice
    {
        Transparent,
        White,
        Black
    }

    // Represents all user-configurable merge settings
    public record MergeOptionsViewModel(
        MergeDirection Direction,
        int Spacing,
        BackgroundColorChoice Background
    );

    // Represents the supported export formats
    public enum ExportFormat
    {
        PNG,
        JPEG,
        WEBP,
        BMP,
        TIFF
    }
}
