using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using E2ETests.Pages;

namespace E2ETests.Tests;

[TestClass]
public class SampleTests : PlaywrightBase
{
    [TestMethod]
    public async Task Playwright_Site_Has_Title()
    {
        var home = new HomePage(Page);

        await home.NavigateAsync("https://playwright.dev/");
        var title = await home.GetTitleAsync();

        Assert.IsTrue(title.Contains("Playwright"),
            $"Expected title to contain 'Playwright' but was '{title}'");
    }
}

