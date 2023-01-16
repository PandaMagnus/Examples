using ExampleBlazorApp.Shared;
using System.Text.Json;

namespace ExampleBlazorApp.Client;

public class ViewModel
{
    public ViewModel(HttpClient client)
    {
        _Client = client;
    }

    private HttpClient _Client;

    public async Task<Game> Play(Player playerModel, Game game)
    {
        game.ErrorMessage = null;
        HttpResponseMessage validateResponse = await _Client.PostAsync($"/api/rockpaperscissors/validate/{playerModel.PlayerChoice}", null);
        string gameString = await validateResponse.Content.ReadAsStringAsync();
        Game? returnedGame = JsonSerializer.Deserialize<Game>(gameString);
        if(!validateResponse.IsSuccessStatusCode 
            || returnedGame is null
            || returnedGame.ErrorMessage is not null)
        {
            game = returnedGame ?? game;
            game.GameResult ??= "Something went wrong. Please try again.";
            return game;
        }

        StringContent content = new StringContent(gameString, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage gameResponse = await _Client.PostAsync($"/api/rockpaperscissors/play", content);
        gameString = await gameResponse.Content.ReadAsStringAsync();
        returnedGame = JsonSerializer.Deserialize<Game>(gameString);

        if (!gameResponse.IsSuccessStatusCode
            || returnedGame is null
            || returnedGame.ErrorMessage is not null)
        {
            game = returnedGame ?? game;
            game.GameResult ??= "Something went wrong. Please try again.";
            return game;
        }

        game = returnedGame ?? game;
        return game;
    }
}
