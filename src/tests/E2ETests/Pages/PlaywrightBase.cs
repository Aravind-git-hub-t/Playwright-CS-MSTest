using Microsoft.Playwright.MSTest;

namespace E2ETests.Pages;

public abstract class PlaywrightBase : PageTest
{
    // All Playwright setup is handled by PageTest.
    // We keep this class as a base for all test classes.
}
