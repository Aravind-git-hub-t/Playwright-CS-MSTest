using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages;

public class DemoQaTextBoxPage : BasePage
{
    private const string PageUrl = "https://demoqa.com/text-box";

    public DemoQaTextBoxPage(IPage page) : base(page)
    {
    }

    // ------------------------------
    // Input locators
    // ------------------------------

    private ILocator FullNameInput => Page.Locator("#userName");
    private ILocator EmailInput => Page.Locator("#userEmail");
    private ILocator CurrentAddressInput => Page.Locator("#currentAddress");
    private ILocator PermanentAddressInput => Page.Locator("#permanentAddress");
    private ILocator SubmitButton => Page.Locator("#submit");

    // ------------------------------
    // Output locators
    // ------------------------------

    private ILocator OutputContainer => Page.Locator("#output");
    private ILocator OutputName => OutputContainer.Locator("#name");
    private ILocator OutputEmail => OutputContainer.Locator("#email");
    private ILocator OutputCurrentAddress => OutputContainer.Locator("#currentAddress");
    private ILocator OutputPermanentAddress => OutputContainer.Locator("#permanentAddress");

    // ------------------------------
    // Actions
    // ------------------------------

    /// <summary>Navigates to the DemoQA Text Box page.</summary>
    public async Task NavigateAsync()
{
    await Page.GotoAsync(PageUrl, new PageGotoOptions
    {
        // We only need the DOM to be loaded for our inputs to exist
        WaitUntil = WaitUntilState.DOMContentLoaded,
        // Give slower networks more time
        Timeout = 60000 // 60 seconds
    });
}


    /// <summary>Fills the form with the provided values.</summary>
    public async Task FillFormAsync(
        string fullName,
        string email,
        string currentAddress,
        string permanentAddress)
    {
        await FullNameInput.FillAsync(fullName);
        await EmailInput.FillAsync(email);
        await CurrentAddressInput.FillAsync(currentAddress);
        await PermanentAddressInput.FillAsync(permanentAddress);
    }

    /// <summary>Clicks the Submit button.</summary>
    public async Task SubmitAsync()
    {
        await SubmitButton.ClickAsync();
    }

    // ------------------------------
    // Extract output results
    // ------------------------------

    public async Task<(string name, string email, string currentAddress, string permanentAddress)>
        GetOutputValuesAsync()
    {
        await OutputContainer.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible
        });

        var nameText = await OutputName.InnerTextAsync();                // "Name: John Doe"
        var emailText = await OutputEmail.InnerTextAsync();              // "Email: john@example.com"
        var currentAddressText = await OutputCurrentAddress.InnerTextAsync();
        var permanentAddressText = await OutputPermanentAddress.InnerTextAsync();

        return (
            ExtractValue(nameText),
            ExtractValue(emailText),
            ExtractValue(currentAddressText),
            ExtractValue(permanentAddressText));
    }

    private static string ExtractValue(string labeledText)
    {
        // Example: "Name: John Doe" -> "John Doe"
        if (string.IsNullOrWhiteSpace(labeledText))
            return string.Empty;

        var index = labeledText.IndexOf(':');
        if (index < 0 || index == labeledText.Length - 1)
            return labeledText.Trim();

        return labeledText[(index + 1)..].Trim();
    }
}
