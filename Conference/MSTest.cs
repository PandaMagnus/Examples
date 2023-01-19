using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace PlaywrightExample.MSTest;

[TestClass]
public class RockPaperScissorsTests : PageTest
{
    private const string RockPaperScissorsNavButton = "rps-nav";
    private const string PlayerInput = "player-input";
    private const string SubmitPlayerChoiceButton = "submit-input-btn";
    private const string ComputerChoiceLabel = "computer-choice";
    private const string GameResultLabel = "result";

    [TestMethod]
    public async void NavigateToPageSubmitInputAndValidateResponse()
    {
        await Page.GotoAsync("https://localhost:7062/");
        await Page.GetByTestId(RockPaperScissorsNavButton).ClickAsync();
        await Page.GetByTestId(PlayerInput).FillAsync("rock");
        await Page.GetByTestId(SubmitPlayerChoiceButton).ClickAsync();
        await Expect(Page.GetByTestId(ComputerChoiceLabel)).ToBeVisibleAsync();
        await Expect(Page.GetByTestId(ComputerChoiceLabel)).ToContainTextAsync("Computer Picks");
        await Expect(Page.GetByTestId(GameResultLabel)).Not.ToContainTextAsync("Awaiting player input...");
    }
}


[TestClass]
public class RockPaperScissorsApiTests : PlaywrightTest
{
    [TestMethod]
    public async Task CallValidateAndVerifyResponse()
    {
        IAPIRequestContext request = await Playwright.APIRequest.NewContextAsync();
        IAPIResponse response = await request.PostAsync("https://localhost:7062/api/rockpaperscissors/validate/rock");
    }
}
