using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace E2ETests.Helpers
{
    public static class ScreenshotHelper
    {
        public static async Task CaptureAsync(TestContext context, IPage page, string tag)
        {
            if (page == null)
                return;

            // Create run directory
            var runDir = context.TestRunDirectory ?? Directory.GetCurrentDirectory();
            var folder = Path.Combine(runDir, "Screenshots");
            Directory.CreateDirectory(folder);

            // Create filename
            string safeTag = string.Join("_", tag.Split(Path.GetInvalidFileNameChars()));
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"{context.TestName}_{safeTag}_{timestamp}.png";

            string fullPath = Path.Combine(folder, filename);

            // Capture screenshot
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = fullPath,
                FullPage = true
            });

            // Attach to MSTest output
            context.WriteLine($"[Screenshot] Saved to: {fullPath}");
        }
    }
}
