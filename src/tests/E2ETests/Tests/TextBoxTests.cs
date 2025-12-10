using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Playwright;
using E2ETests.Pages;


namespace E2ETests.Tests;

[TestClass]
public class TextBoxTests : PlaywrightBase
{
 private async Task CaptureScreenshotAsync(string tag)
{
    // BaseDirectory = bin/Debug/net10.0/ during test run.
    var projectDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

    var screenshotsDir = Path.Combine(projectDir, "Screenshots");
    Directory.CreateDirectory(screenshotsDir);

    // Make tag file-name friendly
    foreach (var c in Path.GetInvalidFileNameChars())
        tag = tag.Replace(c, '_');

    // Generate timestamp: yyyyMMdd_HHmmss
    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

    // Final filename format:
    // TestName_Tag_20251210_235959.png
    var fileName = $"{TestContext.TestName}_{tag}_{timestamp}.png";
    var fullPath = Path.Combine(screenshotsDir, fileName);

    await Page.ScreenshotAsync(new PageScreenshotOptions
    {
        Path = fullPath,
        FullPage = true
    });

    TestContext.WriteLine($"[Screenshot] Saved to: {fullPath}");

    TestContext.AddResultFile(fullPath);
}



    [TestMethod]
    public async Task TextBox_Submits_All_Fields_Correctly()
    {
        // Arrange
        var textBoxPage = new DemoQaTextBoxPage(Page);
        await textBoxPage.NavigateAsync();

        var fullName = "John Doe";
        var email = "john.doe@example.com";
        var currentAddress = "123 Current Street, City";
        var permanentAddress = "456 Permanent Avenue, Town";

        // Act
        await textBoxPage.FillFormAsync(fullName, email, currentAddress, permanentAddress);
        await textBoxPage.SubmitAsync();

        var (nameOut, emailOut, currentAddressOut, permanentAddressOut) =
            await textBoxPage.GetOutputValuesAsync();

        // Assert with screenshots on failure

    if (nameOut != fullName)
    {
        await CaptureScreenshotAsync("NameMismatch");
        Assert.AreEqual(fullName, nameOut, "Full name output did not match input.");
    }

    if (emailOut != email)
    {
        await CaptureScreenshotAsync("EmailMismatch");
        Assert.AreEqual(email, emailOut, "Email output did not match input.");
    }

    if (currentAddressOut != currentAddress)
    {
        await CaptureScreenshotAsync("CurrentAddressMismatch");
        Assert.AreEqual(currentAddress, currentAddressOut, "Current address output did not match input.");
    }

    if (permanentAddressOut != permanentAddress)
    {
        await CaptureScreenshotAsync("PermanentAddressMismatch");
        Assert.AreEqual(permanentAddress, permanentAddressOut, "Permanent address output did not match input.");
    }

     // Always capture a final "result" screenshot even if all asserts pass
    await CaptureScreenshotAsync("FinalResult");

    }
}