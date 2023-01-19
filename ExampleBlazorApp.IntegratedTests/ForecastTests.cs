using Microsoft.Playwright;

namespace ExampleBlazorApp.IntegratedTests;

public class ForecastTests
{
    private const string ForecastButton = "fetch-data-button";
    private const string ForecastTable = "forecast-table";

    [Fact]
    public async Task PageLoad()
    {
        // FOR CONF:
        using IPlaywright pw = await Playwright.CreateAsync();
        IBrowser chrome = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        IBrowserContext context = await chrome.NewContextAsync();
        IPage page = await context.NewPageAsync();
        await page.GotoAsync("https://localhost:7062/");
        await page.GetByTestId(RockPaperScissorsNavButton).ClickAsync();
        await page.GetByTestId(PlayerInput).FillAsync("rock");
        await page.RunAndWaitForResponseAsync(async () =>
        {
            await page.GetByTestId(SubmitPlayerChoiceButton).ClickAsync();
        }, response => response.Url.Contains("/api/rockpaperscissors/play"));

        string? text = await page.GetByTestId(GameResultLabel).TextContentAsync();
        Assert.NotNull(text);
        Assert.NotEqual("Awaiting player input...", text);
    }

    [Fact]
    public async void FetchData()
    {
        // FOR CONF:
        using IPlaywright pw = await Playwright.CreateAsync();
        IAPIRequestContext context = await pw.APIRequest.NewContextAsync();
        IAPIResponse response = await context.PostAsync("https://localhost:7062/api/rockpaperscissors/validate/rock", new APIRequestContextOptions { IgnoreHTTPSErrors = true });
        Assert.Equal(200, response.Status);
        ValidateResponse? deserializedResponse = System.Text.Json.JsonSerializer.Deserialize<ValidateResponse>(await response.TextAsync());
        Assert.NotNull(deserializedResponse);
        Assert.True(deserializedResponse.isPlayerSelectionValid);
    }
}
