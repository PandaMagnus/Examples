using Microsoft.Playwright;
using System.Net;

namespace PlaywrightExample.xUnit;

public class RockPaperScissorsTests
{
    private const string RockPaperScissorsNavButton = "rps-nav";
    private const string PlayerInput = "player-input";
    private const string SubmitPlayerChoiceButton = "submit-input-btn";
    private const string ComputerChoiceLabel = "computer-choice";
    private const string GameResultLabel = "result";

    [Fact]
    public async Task NavigateToPageAndSubmitInputVerifyValidResponse()
    {
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

        Assert.True(await page.GetByTestId(ComputerChoiceLabel).IsVisibleAsync());
        string? computerChoice = await page.GetByTestId(ComputerChoiceLabel).TextContentAsync();
        Assert.Contains("Computer Picks", computerChoice);
        string? gameResult = await page.GetByTestId(GameResultLabel).TextContentAsync();
        Assert.NotNull(gameResult);
        Assert.NotEqual("Awaiting player input...", gameResult);
    }

    [Fact]
    public async Task ValidateValidInput()
    {
        using IPlaywright pw = await Playwright.CreateAsync();
        IAPIRequestContext context = await pw.APIRequest.NewContextAsync();
        IAPIResponse response = await context.PostAsync("https://localhost:7062/api/rockpaperscissors/validate/rock", new APIRequestContextOptions { IgnoreHTTPSErrors = true });
        Assert.Equal(200, response.Status);
        string body = await response.TextAsync();
        ValidateResponse? deserializedResponse = System.Text.Json.JsonSerializer.Deserialize<ValidateResponse>(await response.TextAsync());
        Assert.NotNull(deserializedResponse);
        Assert.True(deserializedResponse.isPlayerSelectionValid);
    }
}


public class ValidateResponse
{
    public bool isPlayerSelectionValid { get; set; }
    public int playerChoice { get; set; }
    public int computerChoice { get; set; }
    public object gameResult { get; set; }
    public string setResult { get; set; }
    public object errorMessage { get; set; }
}
