using Lomont.RPSBaseLib;
using System.Diagnostics;
using WPFRockPaperScissors;

// class to track head to head score
class Score
{
    public Score(Player player)
    {
        Player = player;
    }
    public Player Player;
    public int wins{get; set; }
    public int losses{get; set; }
    public int ties{get; set; }
    
    // wins / total games
    public double WinRatio
    {
        get
        {
            var w = (double)wins;
            var t = wins + losses + ties;
            var p = w / (t == 0 ? 1 : t);
            return p;
        }
    }
    public void Add(Score score)
    {
        wins += score.wins;
        losses += score.losses;
        ties+= score.ties;
    }
    public override string ToString()
    {
        return $"{Player.Filename} {100*WinRatio:F1}% W:{wins} L:{losses} T:{ties}";
    }
}

class HeadToHeadScore
{
    public HeadToHeadScore(Player player1, Player player2)
    {
        player1Score = new Score(player1);
        player2Score = new Score(player2);
    }

    public Score player1Score;
    public Score player2Score;

    public override string ToString()
    {
        return $"{player1Score} : {player2Score}";
    }

    internal void Tally(Player player1, Move move1, Player player2, Move move2)
    {
        var result = RockPaperScissorsLib.Score(move1, move2);
        //Debug.WriteLine($"{player1.Filename} vs {player2.Filename}: {move1}-{move2} {result}");
        if (result == Result.Player1Win)
        {
            player1Score.wins++;
            player2Score.losses++;
        }
        else if (result == Result.Player2Win)
        {
            player1Score.losses++;
            player2Score.wins++;
        }
        else
        {
            player1Score.ties++;
            player2Score.ties++;
        }
    }
}


