using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lomont.RPSBaseLib;

namespace WPFRockPaperScissors
{
    /// <summary>
    /// Run a tournament, given a cross table for the players
    /// </summary>
    class Runner
    {
        public class RunProgress
        {
            public double RatioCompleted;
            public HeadToHeadScore HeadToHeadScore;
        }

        CrossTable table;

        // Run the tournament async
        public Task RunAsync(int rounds, CrossTable crossTable, CancellationToken cancellationToken, IProgress<RunProgress> progress)
        {
            return Task.Run(() =>
            {
                int completedMatches = 0;
                int num = crossTable.Players.Count;
                crossTable.Scores.Clear();
                int numGames = num * (num - 1) / 2;
                table = crossTable;
                completedMatches = 0;
                var roundMatchups = crossTable.NextRoundSchedule();

                while (roundMatchups != null)
                {

                    Parallel.For(0, roundMatchups.Count, n => {
                        var match = roundMatchups[n];
                        cancellationToken.ThrowIfCancellationRequested();
                        var player1 = match.Item1;
                        var player2 = match.Item2;
                        if (player1 != null && player2 != null)
                        { // check for bye
                            var headTohead = PlayMatch(rounds, player1, player2);
                            Interlocked.Increment(ref completedMatches);
                            double completedRatio = completedMatches;
                            completedRatio /= numGames;
                            progress.Report(new RunProgress { RatioCompleted = completedRatio, HeadToHeadScore = headTohead });
                        }
                    });
                    roundMatchups = crossTable.NextRoundSchedule();
                }
            }
            );
        }


        private HeadToHeadScore PlayMatch(int rounds, Player player1, Player player2)
        {
            player1.Reset(player2);
            player2.Reset(player1);

            var scorer = new HeadToHeadScore(player1, player2);

            for (var i =0; i < rounds; ++i)
            {
                if (!player1.GetMove(out Move move1))
                    throw new Exception($"Player {player1} did not move");
                if (!player2.GetMove(out Move move2))
                    throw new Exception($"Player {player2} did not move");
                player1.SetMove(move2);
                player2.SetMove(move1);
                scorer.Tally(player1,move1, player2, move2);
            }
            return scorer;
        }
    }
}
