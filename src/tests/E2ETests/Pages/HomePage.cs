using System.Threading.Tasks;
using Microsoft.Playwright;

namespace E2ETests.Pages;

public class HomePage
{
    private readonly IPage _page;

    public HomePage(IPage page)
    {
        _page = page;
    }

    public async Task NavigateAsync(string url)
        => await _page.GotoAsync(url);

    public async Task<string> GetTitleAsync()
        => await _page.TitleAsync();
}
