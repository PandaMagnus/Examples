using System.Text.Json.Serialization;

namespace ExampleBlazorApp.Shared;


public enum Option
{
    Invalid,
    Rock,
    Paper,
    Scissors
}


public enum Outcome
{
    Indeterminate,
    Win,
    Lose,
    Draw
}

public class Game
{
    [JsonPropertyName("isPlayerSelectionValid")]
    public bool IsPlayerSelectionValid { get; set; }
    [JsonPropertyName("playerChoice")]
    public Option PlayerChoice { get; set; }
    [JsonPropertyName("computerChoice")]
    public Option ComputerChoice { get; set; }
    [JsonPropertyName("gameResult")]
    public string? GameResult { get; set; }
    [JsonPropertyName("setResult")]
    public string SetResult { get; set; } = "";
    // May not need error message.
    // Instead put results in GameResult?
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}
