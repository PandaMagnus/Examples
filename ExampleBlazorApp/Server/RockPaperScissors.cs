using ExampleBlazorApp.Shared;

namespace ExampleBlazorApp.Server;

//public enum Option
//{
//    Invalid,
//    Rock,
//    Paper,
//    Scissors
//}

//public enum Outcome
//{
//    Indeterminate,
//    Win,
//    Lose,
//    Draw
//}

public static class RockPaperScissors
{
    private static List<(Option PlayerPick, Outcome PlayerOutcome)> _PriorHumanChoices = new List<(Option, Outcome)>();
    private static List<List<(Option PlayerPick, Outcome PlayerOutcome)>> _SingleGameModel = new(); // Is this worth it? Seems like random chance might be most straighforward. UNLESS a player likes to play one option... A LOT. And then randomly picking from here would weight towards beating that.
    private static List<List<(Option PlayerPick, Outcome PlayerOutcome)>> _BestOfThreeModel = new();
    private static List<List<(Option PlayerPick, Outcome PlayerOutcome)>> _BestOfFiveModel = new();
    private static bool _SetComplete; // Does IIS parallelize this correctly? Or does this need to be an instance?
    private static string[,] _Outcomes;

    // Probably extract these adds out for clarity
    // Also figure out how to configure this for games that last for other than 3 turns
    // Also find a better way to do this other than hard coding. Maybe play something like 1,000 random choice games and then replace as real games are played?
    // Or have three different training sets, one for single games, best of 3 and best of 5?
    // Or, hell, a combination... a dataset that mixes of best of 1, best of 3, best of 5, best of 7, etc.?
    static RockPaperScissors()
    {
        _Outcomes = new string[4, 4]
        {
            { "I", "I", "I", "I" },
            { "I", "D", "L", "W" },
            { "I", "W", "D", "L" },
            { "I", "L", "W", "D" }
        };

        #region Training Data
        // Check the performance impact of Tuples. May be more performant to use a list of KeyValue pairs?
        // Although we'd have to assess how important performance is for this.
        // Also, does _TrainingModel need to be a List<List<>>? Seems like we could use something faster for the outer one like a HashSet or Dictionary if we don't duplicate game results.
        // OR do we ditch that altogether? Is there a better way to handle this that doesn't also require the number of games specified in a set?
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Rock, Outcome.Win),
                (Option.Rock, Outcome.Lose),
                (Option.Scissors, Outcome.Win)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Rock, Outcome.Win),
                (Option.Rock, Outcome.Lose),
                (Option.Scissors, Outcome.Lose)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Rock, Outcome.Lose),
                (Option.Scissors, Outcome.Win),
                (Option.Scissors, Outcome.Win)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Rock, Outcome.Draw),
                (Option.Rock, Outcome.Win),
                (Option.Rock, Outcome.Lose)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Rock, Outcome.Draw),
                (Option.Paper, Outcome.Draw),
                (Option.Scissors, Outcome.Win)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Paper, Outcome.Win),
                (Option.Paper, Outcome.Lose),
                (Option.Rock, Outcome.Win)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Paper, Outcome.Win),
                (Option.Paper, Outcome.Lose),
                (Option.Rock, Outcome.Lose)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Paper, Outcome.Lose),
                (Option.Rock, Outcome.Win),
                (Option.Rock, Outcome.Win)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Paper, Outcome.Draw),
                (Option.Paper, Outcome.Win),
                (Option.Paper, Outcome.Lose)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Paper, Outcome.Draw),
                (Option.Scissors, Outcome.Draw),
                (Option.Rock, Outcome.Win)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Scissors, Outcome.Win),
                (Option.Scissors, Outcome.Lose),
                (Option.Paper, Outcome.Win)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Scissors, Outcome.Win),
                (Option.Scissors, Outcome.Lose),
                (Option.Paper, Outcome.Lose)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Scissors, Outcome.Lose),
                (Option.Paper, Outcome.Win),
                (Option.Paper, Outcome.Win)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Scissors, Outcome.Draw),
                (Option.Scissors, Outcome.Win),
                (Option.Scissors, Outcome.Lose)
            }
        );
        _BestOfThreeModel.Add(
            new List<(Option PlayerPick, Outcome Outcome)>
            {
                (Option.Scissors, Outcome.Draw),
                (Option.Rock, Outcome.Draw),
                (Option.Paper, Outcome.Win)
            }
        );
    }
    #endregion

    public static Option ValidatePlayerInput(string userInput)
    {
        if (userInput is null) userInput = "";

        return userInput.ToLower() switch
        {
            "rock" => Option.Rock,
            "r" => Option.Rock,
            "paper" => Option.Paper,
            "p" => Option.Paper,
            "scissors" => Option.Scissors,
            "scissor" => Option.Scissors,
            "s" => Option.Scissors,
            _ => Option.Invalid
        };
    }

    public static Game ProcessPlayerInput(Game game)
    {
        game.ComputerChoice = DetermineComputerChoice();
        Outcome didPlayerWin = DetermineIfPlayerWon(game);

        // Might make sense to move this logic into DetermineIfHumanWon
        if (didPlayerWin is Outcome.Indeterminate)
        {
            game.GameResult = "Unable to determine the victory. Please try again.";
        }
        else if (didPlayerWin is Outcome.Draw)
        {
            game.GameResult = "A draw occurred. Try again!";
        }
        else if (didPlayerWin is Outcome.Win)
        {
            game.GameResult = "Congratulations, you won!";
        }
        else
        {
            game.GameResult = "Sorry, the computer won. Please try again!";
        }

        // if(SetIsComplete)
        // {
        //    _PriorHumanChoices = new();
        // }
        _PriorHumanChoices.Add((game.PlayerChoice, didPlayerWin));
        //_BestOfThreeModel.Add((game.PlayerChoice, didPlayerWin));
        return game;
    }

    private static Option DetermineComputerChoice()
    {
        return Decision();
    }


    // Don't need this method. Just handle it as part of determining the computer choice.
    public static Option Decision()
    {
        Option computerPick = RandomPick();

        List<List<(Option PlayerPick, Outcome PlayerOutcome)>> possibleSegments = new();
        possibleSegments.AddRange(_BestOfThreeModel.Where(m => m.Take(_PriorHumanChoices.Count).SequenceEqual(_PriorHumanChoices)).ToList());

        if (_PriorHumanChoices.Count > 0)
        {
            // if(game.BestOf == 3)

            if (possibleSegments.Count > 0)
            {
                // Would it be worth weighting *these* instead of random choice?
                // As a hacky version, if we come up with an example where the player won, go ahead and re-try
                // Another consideration: the most common strategy is to play against whatever the other person played last.
                // E.G. If I play rock and comp plays paper, I'm more likely to play scissors in game 2.
                Random rand = new();
                int randPick = rand.Next(0, possibleSegments.Count);
                var guessNextPick = possibleSegments[randPick][_PriorHumanChoices.Count];
                if (guessNextPick.PlayerOutcome is Outcome.Win)
                {
                    randPick = rand.Next(0, possibleSegments.Count);
                    guessNextPick = possibleSegments[randPick][_PriorHumanChoices.Count];
                }
                computerPick = PickWinningOption(guessNextPick.PlayerPick);
            }
        }

        // Add prior human choice segments to training model here
        return computerPick;
    }

    private static Option RandomPick()
    {
        int rand = new Random()
            .Next(3);
        return rand switch
        {
            0 => Option.Rock,
            1 => Option.Paper,
            2 => Option.Scissors,
            _ => Option.Rock,
        };
    }

    private static Outcome DetermineIfPlayerWon(Game game)
    {
        // Feels like this might be able to be done more efficiently
        //switch (game.PlayerChoice)
        //{
        //    case Option.Rock:
        //        if (game.ComputerChoice is Option.Rock)
        //            return Outcome.Draw;
        //        if (game.ComputerChoice is Option.Scissors)
        //            return Outcome.Win;
        //        if (game.ComputerChoice is Option.Paper)
        //            return Outcome.Lose;
        //        break;
        //    case Option.Paper:
        //        if (game.ComputerChoice is Option.Rock)
        //            return Outcome.Win;
        //        if (game.ComputerChoice is Option.Scissors)
        //            return Outcome.Lose;
        //        if (game.ComputerChoice is Option.Paper)
        //            return Outcome.Draw;
        //        break;
        //    case Option.Scissors:
        //        if (game.ComputerChoice is Option.Rock)
        //            return Outcome.Lose;
        //        if (game.ComputerChoice is Option.Scissors)
        //            return Outcome.Draw;
        //        if (game.ComputerChoice is Option.Paper)
        //            return Outcome.Win;
        //        break;
        //    default:
        //        break;
        //}


        //int playerChoice = (int)game.PlayerChoice;
        //int computerChoice = (int)game.ComputerChoice;

        // Subtract one since 0 in the enum is Invalid
        var result = _Outcomes[(int)game.PlayerChoice, (int)game.ComputerChoice];
        return result.ToUpperInvariant() switch
        {
            "I" => Outcome.Indeterminate,
            "W" => Outcome.Win,
            "L" => Outcome.Lose,
            "D" => Outcome.Draw,
            _ => Outcome.Indeterminate
        };

        //return Outcome.Indeterminate;
    }

    private static Option PickWinningOption(Option userSelection)
    {
        return userSelection switch
        {
            Option.Rock => Option.Paper,
            Option.Paper => Option.Scissors,
            Option.Scissors => Option.Rock,
            _ => Option.Invalid
        };
    }
}
