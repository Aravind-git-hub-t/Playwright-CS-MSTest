using AventStack.ExtentReports;
using System;

namespace E2ETests.Reporting
{
    public static class ReportHelper
    {
        /// <summary>
        /// Log an info step to the provided ExtentTest.
        /// </summary>
        public static void LogInfo(ExtentTest test, string message)
        {
            test?.Info(message);
        }

        /// <summary>
        /// Log a passing step and optionally attach a screenshot (path).
        /// </summary>
        public static void LogPass(ExtentTest test, string message, string? screenshotPath = null)
        {
            if (test == null) return;

            if (!string.IsNullOrEmpty(screenshotPath))
            {
                try
                {
                    var media = AventStack.ExtentReports.MediaEntityBuilder.CreateScreenCaptureFromPath(screenshotPath).Build();
                    test.Pass(message, media);
                }
                catch
                {
                    // if attaching fails, still mark pass with text
                    test.Pass(message + " (screenshot attach failed)");
                }
            }
            else
            {
                test.Pass(message);
            }
        }

        /// <summary>
        /// Log a failing step and optionally attach a screenshot (path).
        /// </summary>
        public static void LogFail(ExtentTest test, string message, string? screenshotPath = null)
        {
            if (test == null) return;

            if (!string.IsNullOrEmpty(screenshotPath))
            {
                try
                {
                    var media = AventStack.ExtentReports.MediaEntityBuilder.CreateScreenCaptureFromPath(screenshotPath).Build();
                    test.Fail(message, media);
                }
                catch
                {
                    test.Fail(message + " (screenshot attach failed)");
                }
            }
            else
            {
                test.Fail(message);
            }
        }

        /// <summary>
        /// Log a warning step.
        /// </summary>
        public static void LogWarning(ExtentTest test, string message)
        {
            test?.Warning(message);
        }

        /// <summary>
        /// Attach a screenshot to the report with an informational message.
        /// </summary>
        public static void AttachScreenshot(ExtentTest test, string message, string? screenshotPath)
        {
            if (test == null) return;

            if (!string.IsNullOrEmpty(screenshotPath))
            {
                try
                {
                    var media = AventStack.ExtentReports.MediaEntityBuilder.CreateScreenCaptureFromPath(screenshotPath).Build();
                    test.Info(message, media);
                }
                catch
                {
                    test.Info(message + " (screenshot attach failed)");
                }
            }
            else
            {
                test.Info(message + " (no screenshot)");
            }
        }
    }
}
