using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace MergeImages.UI.Services
{
    public class DialogService : IDialogService
    {
        public async Task<string[]?> ShowOpenFileDialogAsync(FileDialogOptions options)
        {
            var window = GetTopLevel();
            if (window is null)
                return null;

            var fileTypes = BuildFileTypes(options.Filters, "Images");
            var result = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = options.Title,
                AllowMultiple = options.AllowMultiple,
                FileTypeFilter = fileTypes
            });

            if (result is null || result.Count == 0)
                return null;

            return result
                .Select(f => f.TryGetLocalPath() ?? f.Path.LocalPath)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();
        }

        public async Task<string?> ShowSaveFileDialogAsync(FileDialogOptions options)
        {
            var window = GetTopLevel();
            if (window is null)
                return null;

            var fileTypes = BuildFileTypes(options.Filters, "Image");
            var result = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = options.Title,
                SuggestedFileName = options.SuggestedFileName,
                FileTypeChoices = fileTypes,
                DefaultExtension = options.Filters?.FirstOrDefault()?.TrimStart('.')
            });

            if (result is null)
                return null;

            return result.TryGetLocalPath() ?? result.Path.LocalPath;
        }

        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            // Simplified confirmation (always yes for now)
            return Task.FromResult(true);
        }

        private static Window? GetTopLevel() =>
            Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow : null;

        private static IReadOnlyList<FilePickerFileType>? BuildFileTypes(string[]? filters, string name)
        {
            if (filters is null || filters.Length == 0)
                return null;

            var patterns = filters
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Select(f => f.StartsWith("*.", StringComparison.Ordinal) ? f : (f.StartsWith(".") ? "*" + f : "*." + f))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new[] { new FilePickerFileType(name) { Patterns = patterns } };
        }
    }
}
