using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace ExampleBlazorApp.IntegratedTests.MsTest;

[TestClass]
public class ForecastTests : PageTest
{
    private const string ForecastButton = "fetch-data-button";
    private const string ForecastTable = "forecast-table";

    [TestMethod]
    public async Task PageLoad()
    {
        await Page.GotoAsync("https://localhost:7257");
        await Page.GetByTestId(ForecastButton).ClickAsync();
        await Expect(Page).ToHaveTitleAsync("Weather Forecast");
        await Expect(Page.Locator(ForecastTable)).ToBeEnabledAsync();
    }
}

[TestClass]
public class ForecastApiTests : PlaywrightTest
{
    [TestMethod]
    public async void FetchData()
    {
        IAPIRequestContext request = await Playwright.APIRequest.NewContextAsync();
        IAPIResponse response = await request.GetAsync("https://localhost:7257/WeatherForecast");
        await Expect(response).ToBeOKAsync();
    }
}
