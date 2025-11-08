using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Skia;

namespace MergeImages.UI.Tests;

internal class TestApp : Application { }

internal static class AvaloniaTestApp
{
    private static bool _initialized;

    [ModuleInitializer]
    public static void Init()
    {
        if (_initialized) return;
        AppBuilder.Configure<TestApp>()
            .UseSkia()
            .UseHeadless(new Avalonia.Headless.AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true })
            .SetupWithoutStarting();
        _initialized = true;
    }
}
