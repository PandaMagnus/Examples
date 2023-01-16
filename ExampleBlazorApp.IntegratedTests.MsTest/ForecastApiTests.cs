using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System.Runtime.CompilerServices;

namespace ExampleBlazorApp.IntegratedTests.MsTest;

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
