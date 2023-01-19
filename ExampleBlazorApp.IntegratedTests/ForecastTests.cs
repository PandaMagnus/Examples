using Microsoft.Playwright;

namespace ExampleBlazorApp.IntegratedTests;

public class ForecastTests
{
    private const string ForecastButton = "fetch-data-button";
    private const string ForecastTable = "forecast-table";

    [Fact]
    public async Task PageLoad()
    {
        using IPlaywright pw = await Playwright.CreateAsync();
        IBrowser chrome = await pw.Chromium.LaunchAsync();
        IBrowserContext context = await chrome.NewContextAsync();
        IPage page = await context.NewPageAsync();
        await page.GotoAsync("https://localhost:7257");
        await page.GetByTestId(ForecastButton).ClickAsync();
        bool isTableEnabled = await page.GetByTestId(ForecastTable).IsEnabledAsync();
        Assert.True(isTableEnabled, "Forecast table never became enabled after attempting to navigate to the page.");
        
        // FOR CONF:
        await page.RunAndWaitForResponseAsync(async () =>
        {
            await page.GetByTestId(SubmitPlayerChoiceButton).ClickAsync();
        }, response => response.Url.Contains("/api/rockpaperscissors/play"));

        string? text = await page.GetByTestId(GameResultLabel).TextContentAsync();
    }

    [Fact]
    public async void FetchData()
    {
        using IPlaywright pw = await Playwright.CreateAsync();
        IAPIRequestContext context = await pw.APIRequest.NewContextAsync();
        IAPIResponse response = await context.GetAsync("https://localhost:7257/WeatherForecast");
        Assert.Equal(200, response.Status);
    }
}
