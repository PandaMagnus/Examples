using Microsoft.Playwright.NUnit;

namespace ExampleBlazorApp.IntegratedTests.Nunit;

public class ForecastApiTests : PlaywrightTest
{
    [SetUp]
    public async void Setup()
    {
        await CreateAPIRequestContext();
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}