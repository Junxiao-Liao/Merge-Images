# MergeImages

A desktop app for merging images into one file with custom layouts and formats.

## Overview

MergeImages is a cross-platform desktop app built with Avalonia UI. Combine images vertically or horizontally with drag-and-drop support. Export as PNG, JPEG, WEBP, BMP, or TIFF.

## How to Use

1. **Add Images**: Click "Select Images" or drag files into the window
2. **Arrange**: Drag thumbnails to reorder them
3. **Set Options**: Pick vertical or horizontal layout, choose background color
4. **Merge**: Click "Merge Images" to combine them
5. **Preview**: Check the result in the preview window
6. **Save**: Pick format, then click "Save As" to export

## Main Window

**Layout Structure:**
```
+-----------------------------------------------------------------+
| Merge-Images                                           [-][+][X]|
|-----------------------------------------------------------------|
|                                                                 |
|  +-----------------------------------------------------------+  |
|  | [ Select Images ]   (or drag & drop files here)           |  |
|  +-----------------------------------------------------------+  |
|                                                                 |
|  +-----------------------------------------------------------+  |
|  | Selected Images (Drag to reorder):              [Clear]   |  |
|  | +-------+   +-------+   +-------+   +-------+             |  |
|  | | [img] |   | [img] |   | [img] |   | [img] |             |  |
|  | | 1.png |   | 2.jpg |   | 3.png |   | 4.jpg |             |  |
|  | |  [X]  |   |  [X]  |   |  [X]  |   |  [X]  |             |  |
|  | +-------+   +-------+   +-------+   +-------+             |  |
|  |                                                           |  |
|  | (When empty: "Drag and drop images here to get started")  |  |
|  +-----------------------------------------------------------+  |
|                                                                 |
|  +-----------------------------------------------------------+  |
|  | Merge Options:                                            |  |
|  | Direction:  [Vertical [V]]  [Horizontal]                  |  |
|  | Background: [Transparent [V]] [White] [Black]             |  |
|  +-----------------------------------------------------------+  |
|                                                                 |
|                          [Merge Images -->]                     |
|                                                                 |
+-----------------------------------------------------------------+
```

## Preview Window

**Layout Structure:**
```
+-----------------------------------------------------------------+
| Preview - Merge-Images                                 [-][+][X]|
|-----------------------------------------------------------------|
|                                                                 |
|  +-----------------------------------------------------------+  |
|  |                                                           |  |
|  |                                                           |  |
|  |                 [Merged Image Preview]                    |  |
|  |                    (Scrollable)                           |  |
|  |                                                           |  |
|  |                                                           |  |
|  +-----------------------------------------------------------+  |
|                                                                 |
|  +-----------------------------------------------------------+  |
|  | Export Options:                                           |  |
|  | Format:  [PNG [V]] [JPEG] [WEBP] [BMP] [TIFF]             |  |
|  +-----------------------------------------------------------+  |
|                                                                 |
|         [<-- Back]                       [Save As...]           |
|                                                                 |
+-----------------------------------------------------------------+
```

## Features

### Image Handling
- Select images via file dialog or drag-and-drop
- View thumbnails of all images
- Reorder by dragging
- Remove single images or clear all
- Auto-filter duplicates

### Merge Options
- Stack images vertically (top-to-bottom) or horizontally (left-to-right)
- Pick background: transparent, white, or black
- Auto-scale images to match size while keeping aspect ratio

### Export
- Preview merged image before saving
- Save as PNG, JPEG, WEBP, BMP, or TIFF

## Project Structure

```
MergeImages/
│
├── MergeImages.sln
│
├── MergeImages.Core/                        # F# Class Library
│   ├── MergeImages.Core.fsproj
│   ├── Types.fs                             # Core domain types
│   ├── Validation.fs                        # Input validation
│   ├── ImageLoader.fs                       # Load images from disk
│   ├── MergeEngine.fs                       # Core merge logic
│   ├── ImageExporter.fs                     # Save merged images
│   └── ThumbnailGenerator.fs                # Generate thumbnails
│
├── MergeImages.UI/                          # C# Avalonia Application
│   ├── MergeImages.UI.csproj
│   ├── App.axaml                            # App entry point
│   ├── App.axaml.cs
│   │
│   ├── Views/                               # UI layouts
│   │   ├── MainWindow.axaml
│   │   ├── MainWindow.axaml.cs
│   │   ├── PreviewView.axaml
│   │   └── PreviewView.axaml.cs
│   │
│   ├── ViewModels/                          # UI logic and state
│   │   ├── ViewModelBase.cs
│   │   ├── MainViewModel.cs                 # Main window logic
│   │   └── PreviewViewModel.cs              # Preview window logic
│   │
│   ├── Services/                            # Helper services
│   │   ├── DialogService.cs                 # File dialogs
│   │   ├── ThumbnailService.cs              # Thumbnail creation
│   │   ├── NavigationService.cs             # Window navigation
│   │   └── CoreBridge.cs                    # F# Core interop
│   │
│   └── Program.cs
│
├── MergeImages.Core.Tests/                  # F# Unit Tests
│   ├── MergeImages.Core.Tests.fsproj
│   ├── Library.fs                           # Test helpers
│   ├── ValidationTests.fs                   # Test validation logic
│   ├── MergeEngineTests.fs                  # Test merge operations
│   └── ImageExporterTests.fs                # Test export functions
│
└── MergeImages.UI.Tests/                    # C# Unit Tests
    ├── MergeImages.UI.Tests.csproj
    ├── CoreBridgeTests.cs                   # Test F# interop
    ├── MainViewModelTests.cs                # Test main window logic
    └── PreviewViewModelTests.cs             # Test preview logic
```
