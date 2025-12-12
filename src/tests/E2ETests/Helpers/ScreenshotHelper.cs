using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace E2ETests.Helpers
{
    /// <summary>
    /// Helper to capture screenshots and return the saved full path for attaching to reports.
    /// </summary>
    public static class ScreenshotHelper
    {
        /// <summary>
        /// Capture screenshot for the provided page and return the saved file path.
        /// Returns null if capture failed.
        /// </summary>
        // Old signature:
// public static async Task<string?> CaptureAsync(TestContext? context, IPage page, string tag)

// New signature (add optional screenshotsDir)
public static async Task<string?> CaptureAsync(TestContext? context, IPage page, string tag, string? screenshotsDir = null)
{
    if (page == null) return null;

    try
    {
        // If a screenshotsDir is provided, use it; otherwise default to project-root/Screenshots
        if (string.IsNullOrEmpty(screenshotsDir))
        {
            var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            screenshotsDir = Path.Combine(projectDir, "Screenshots");
        }

        Directory.CreateDirectory(screenshotsDir);

        // Make tag file-name friendly
        foreach (var c in Path.GetInvalidFileNameChars())
            tag = tag.Replace(c, '_');

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var testName = context?.TestName ?? "UnknownTest";
        var fileName = $"{testName}_{tag}_{timestamp}.png";
        var fullPath = Path.Combine(screenshotsDir, fileName);

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = fullPath,
            FullPage = true
        });

        try
        {
            context?.WriteLine($"[Screenshot] Saved to: {fullPath}");
            // Add result file only if using default MSTest results folder behavior
            context?.AddResultFile(fullPath);
        }
        catch { /* ignore */ }

        return fullPath;
    }
    catch (Exception ex)
    {
        try { context?.WriteLine($"Screenshot capture failed: {ex.Message}"); } catch { }
        return null;
    }
}
    }
    }
