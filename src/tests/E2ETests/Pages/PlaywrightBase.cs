using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AventStack.ExtentReports;
using E2ETests.Reporting;
using E2ETests.Helpers;

namespace E2ETests.Pages
{
    public abstract class PlaywrightBase : PageTest
    {
        // Per-test Extent instance and test handle (one HTML file per test)
        private ExtentReports? _perTestExtent;
        protected ExtentTest ExtentTest { get; private set; } = null!;

        // Keep track of per-test folder used for results/screenshots
        private string? _perTestResultsDir;

        [TestInitialize]
        public void BaseTestInitialize()
        {
            // Determine test name and timestamp
            var testName = TestContext?.TestName ?? $"{GetType().Name}_UnknownTest";
            var safeTestName = MakeFileNameSafe(testName);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Create per-test results folder: <project-root>\Results\<TestName>_<timestamp>\
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            var resultsRoot = Path.Combine(projectRoot, "Results");
            Directory.CreateDirectory(resultsRoot);

            _perTestResultsDir = Path.Combine(resultsRoot, $"{safeTestName}_{timestamp}");
            Directory.CreateDirectory(_perTestResultsDir);

            // Create per-test HTML report file in this folder
            var reportFile = Path.Combine(_perTestResultsDir, $"{safeTestName}_{timestamp}.html");

            try
            {
                // Create a reporter using ExtentManager (reflection-safe)
                var reporter = ExtentManager.CreateHtmlReporter(reportFile);

                // Create a per-test ExtentReports instance and attach the reporter
                _perTestExtent = new ExtentReports();
                ExtentManager.AttachReporterTo(_perTestExtent, reporter!);

                // Create the ExtentTest for this test (per-test report)
                ExtentTest = _perTestExtent.CreateTest(testName);
                ExtentTest.Info($"Test started: {testName}");

            }
            catch (Exception ex)
            {
                try { TestContext?.WriteLine("Per-test report setup error: " + ex.Message); } catch { }
                // Fallback: create a dummy ExtentTest via assembly-level manager to preserve logging
                try
                {
                    ExtentTest = ExtentManager.Instance?.CreateTest(testName) ?? null!;
                }
                catch { ExtentTest = null!; }
            }
        }

        [TestCleanup]
        public async Task BaseTestCleanupAsync()
        {
            try
            {
                // Capture final screenshot into the per-test results folder if available,
                // otherwise default to the original behavior.
                string? screenshotPath = null;
                try
                {
                    screenshotPath = await ScreenshotHelper.CaptureAsync(TestContext, Page, "FinalResult", _perTestResultsDir);
                }
                catch (Exception ex)
                {
                    try { TestContext?.WriteLine("Final screenshot capture failed: " + ex.Message); } catch { }
                }

                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    try { ReportHelper.AttachScreenshot(ExtentTest, "Final screenshot", screenshotPath); } catch { }
                }

                // Log MSTest outcome
                var outcome = TestContext?.CurrentTestOutcome ?? UnitTestOutcome.Inconclusive;

// write to TestContext output only if available
                if (TestContext != null)
                {
    TestContext.WriteLine($"Test outcome: {outcome}");
            }

                if (outcome == UnitTestOutcome.Passed)
                    ExtentTest?.Pass("Test Passed");
                else if (outcome == UnitTestOutcome.Failed)
                    ExtentTest?.Fail("Test Failed");
                else if (outcome == UnitTestOutcome.Inconclusive)
                    ExtentTest?.Warning("Test Inconclusive");
                else
                    ExtentTest?.Skip($"Test ended with outcome: {outcome}");

                // Flush per-test report to disk
                try
                {
                    _perTestExtent?.Flush();
                }
                catch (Exception ex)
                {
                    try { TestContext?.WriteLine("Per-test report flush failed: " + ex.Message); } catch { }
                }
            }
            catch (Exception ex)
            {
                try { TestContext?.WriteLine("PlaywrightBase cleanup error: " + ex.Message); } catch { }
            }
        }

        // Helper to sanitize folder/file name
        private static string MakeFileNameSafe(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
