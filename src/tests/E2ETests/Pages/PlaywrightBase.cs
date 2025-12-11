using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;   
using Microsoft.Playwright.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace E2ETests.Pages
{
    /// <summary>
    /// A minimal base class for Playwright tests.
    /// TestCleanup will always take a final screenshot and save it
    /// to the PROJECT Screenshots folder (src/tests/E2Tests/Screenshots).
    /// This is deterministic and easy to find.
    /// </summary>
    public abstract class PlaywrightBase : PageTest
    {
        // Do not redeclare TestContext if inherited from PageTest.
        // PageTest provides Page and Test lifecycle already.

        [TestInitialize]
        public void Initialize()
        {
            // Optional per-test initialization can go here.
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            try
            {
                // Build a stable project folder path from the assembly base directory.
                // AppContext.BaseDirectory is typically:
                //  ...\src\tests\E2ETests\bin\Debug\net10.0\
                // Walk up to the project folder.
                var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
                var screenshotsDir = Path.Combine(projectDir, "Screenshots");
                Directory.CreateDirectory(screenshotsDir);

                // Create safe tag and timestamp
                var tag = "FinalResult";
                foreach (var c in Path.GetInvalidFileNameChars())
                    tag = tag.Replace(c, '_');

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"{TestContext.TestName}_{tag}_{timestamp}.png";
                var fullPath = Path.Combine(screenshotsDir, fileName);

                // Take full-page screenshot and save to the stable project path
                await Page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = fullPath,
                    FullPage = true
                });

                // Make it easy to find: write to test output and attach to test results
                TestContext.WriteLine($"[Screenshot] Saved to: {fullPath}");
                TestContext.AddResultFile(fullPath);
            }
            catch (Exception ex)
            {
                // Never throw from cleanup — log the error and continue
                try
                {
                    TestContext.WriteLine("Screenshot capture error: " + ex.Message);
                }
                catch
                {
                    // ignore if TestContext not available
                }
            }
        }
    }
}
