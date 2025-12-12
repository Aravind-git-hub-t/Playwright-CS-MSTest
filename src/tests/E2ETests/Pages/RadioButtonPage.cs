using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages;

public class RadioButtonPage : BasePage
{
    private const string PageUrl = "https://demoqa.com/radio-button";

    public RadioButtonPage(IPage page) : base(page)
    {
    }

    // ------------------------------
    // Button locators
    // ------------------------------

private ILocator YesRadioButtonInput       => Page.Locator("label[for='yesRadio']");
private ILocator ImpressiveButtonInput => Page.Locator("label[for='impressiveRadio']");
private ILocator NoRadioButtonInput        => Page.Locator("#noRadio");
private ILocator TextSuccessOutput         => Page.Locator(".text-success");


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
    public async Task Button(String buttonName)
    {
        switch (buttonName)
        {
            case "yes":
            await YesRadioButtonInput.ClickAsync();
            break;
            case "impressive":
            await ImpressiveButtonInput.ClickAsync();
            break;
            default:
            break;
        }

    }
    
    // ------------------------------
    // Extract output results
    // ------------------------------

    public async Task<string>
        GetResultValueAsync()
    {
        await TextSuccessOutput.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible
        });
        string result = await TextSuccessOutput.InnerTextAsync();

        return result;
    }

    public async Task<bool>
        checkIfDisabled()
    {
        bool state = true; 

        state = await NoRadioButtonInput.IsEnabledAsync();

        return state;
    }

}
