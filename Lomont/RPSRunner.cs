using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Collections.Concurrent;
using Lomont.RPSBaseLib;
using R = Lomont.RPSBaseLib.RockPaperScissorsLib;

/* todo
 1. better print
 2. add/remove players command line
 3. better debug from command line
 4. command line help
 5. Keep player live through all?
 6. Code to github with new bot instructions
 */

/* week 1 9/12/2018

250 games each head to head, bots killed betewen matches
Win percentages
  48% LomontBeatlast.exe
  44% RockPaperScissorsLee.exe
  36% LomontCycler.exe
  34% SundermanPsychic.exe
  32% HoganRandom.exe
  30% SVHRockPaperScissors.exe
  29% HoganNaive.exe
  28% HoganConstant.exe
  26% LomontEcho.exe
  26% LomontRandom.exe
  26% LomontConstant.exe

100 games each head to head, bots killed between matches
Win percentages
  48% LomontBeatlast.exe
  43% RockPaperScissorsLee.exe
  35% LomontCycler.exe
  35% SundermanPsychic.exe
  32% HoganRandom.exe
  30% HoganNaive.exe
  30% SVHRockPaperScissors.exe
  26% LomontEcho.exe
  26% LomontConstant.exe
  25% HoganConstant.exe
  25% LomontRandom.exe
*/

namespace RockPaperScissors
{
    class RPSRunner
    {

        class Score
        {
            public string Player1;
            public string Player2;
            public int player1Wins;
            public int player2Wins;
            public int tie;
        }

        class Player
        {
            public string filename;
            public void Capture()
            {
                pc = new ProcessCapture(filename);
            }
            public ProcessCapture pc;
            public int wins, losses, ties;

            public double WinRatio()
            {
                var total = wins + ties + losses;
                if (total == 0)
                    return 0;
                return (double)(wins) / (wins + ties + losses);
            }
        }

        static List<Score> scores = new List<Score>();

        static bool TextToMove(string text, out Move move)
        {
            move = Move.Rock;
            if (String.IsNullOrEmpty(text))
                return false;
            text = text.ToLower();
            if (text == "rock")
            {
                move = Move.Rock;
                return true;
            }
            if (text == "paper")
            {
                move = Move.Paper;
                return true;
            }
            if (text == "scissors")
            {
                move = Move.Scissors;
                return true;
            }
            return false;
        }


        static Score FindScore(string player1Name, string player2Name)
        {
            var s = scores.FirstOrDefault(sc => sc.Player1 == player1Name && sc.Player2 == player2Name);
            if (s == null)
            {
                s = new Score();
                s.Player1 = player1Name;
                s.Player2 = player2Name;
            }
            return s;
        }

        // tally score, get result
        private static Result TallyScore(Player player1, Player player2, string ans1, string ans2)
        {
            var s = FindScore(player1.filename, player2.filename);

            Result result;
            if (!TextToMove(ans1, out Move move1))
            {
                s.player2Wins++; // default by error
                result = Result.Player2Win;
            }
            if (!TextToMove(ans2, out Move move2))
            {
                s.player1Wins++; // default by error
                result = Result.Player1Win;
            }
            else
            {
                result = R.Score(move1, move2);
            }
            switch (result)
            {
                case Result.Player1Win:
                    s.player1Wins++; // default by error
                    player1.wins++;
                    player2.losses++;
                    break;
                case Result.Player2Win:
                    s.player2Wins++; // default by error
                    player2.wins++;
                    player1.losses++;
                    break;
                case Result.Tie:
                    s.tie++; // default by error
                    player1.ties++;
                    player2.ties++;
                    break;
            }
            return result;
        }

        static int gamesPerRound = 100;
        static int numRounds = 1;


        static bool debugPrint = false;

        static List<Player> players = new List<Player>();

        // pretty print all items - todo
        static void PrettyPrint()
        {
            var maxNameLen = players.Max(p => p.filename.Length);

        }
        static void PlayGame(Player player1, Player player2)
        {

            var player1Color = ConsoleColor.Blue;
            var player2Color = ConsoleColor.Red;
            var oldColor = Console.ForegroundColor;

            Console.ForegroundColor = player1Color;
            Console.Write(player1.filename);
            Console.ForegroundColor = oldColor;
            Console.Write(" vs ");
            Console.ForegroundColor = player2Color;
            Console.Write(player2.filename);
            Console.WriteLine();

            for (var game = 0; game < gamesPerRound; ++game)
            {
                player1.Capture();
                player2.Capture();

                if (debugPrint)
                {
                    player1.pc.color = ConsoleColor.DarkGreen;
                    player2.pc.color = ConsoleColor.DarkBlue;
                }

                player1.pc.Write($"RESET {player2.filename}");
                player2.pc.Write($"RESET {player1.filename}");
                for (var round = 0; round < numRounds; ++round)
                {
                    var ans1 = player1.pc.Read(true).FirstOrDefault(s => R.TextMoveLegal(s));
                    var ans2 = player2.pc.Read(true).FirstOrDefault(s => R.TextMoveLegal(s));

                    player1.pc.Write(ans2);
                    player2.pc.Write(ans1);

                    var result = TallyScore(player1, player2, ans1, ans2);

                    //Console.ForegroundColor = player1Color;
                    //Console.Write(ans1);
                    //Console.ForegroundColor = oldColor;
                    //Console.Write(" vs ");
                    //Console.ForegroundColor = player2Color;
                    //Console.Write(ans2);
                    //Console.ForegroundColor = oldColor;
                    //Console.Write(" =>result: ");
                    //if (result == Result.Player1Win)
                    //    Console.ForegroundColor = player1Color;
                    //else if (result == Result.Player2Win)
                    //    Console.ForegroundColor = player2Color;
                    //else 
                    //    Console.ForegroundColor = tieColor;
                    //Console.WriteLine(result);
                }
                player1.pc.Write("QUIT");
                player2.pc.Write("QUIT");
            }
            Console.ForegroundColor = oldColor;
        }


        static void Main(string[] args)
        {
            var foreground = Console.ForegroundColor;
            try
            {
                if (args.Any() && args[0] == "-d")
                    debugPrint = true;

                var names = R.GetProgramNames();
                int count = names.Count();


                if (count == 0)
                {
                    Console.WriteLine("Cannot find any executables to run");
                    return;
                }
                else
                {
                    Console.WriteLine("Participants: ");
                    foreach (var n in names)
                    {
                        Console.WriteLine($"   {n}");
                        players.Add(new Player { filename = n });
                    }
                }


                // round robin them
                for (var i = 0; i < count; ++i)
                    for (var j = i + 1; j < count; ++j)
                    {
                        var player1 = players[i];
                        var player2 = players[j];
                        PlayGame(player1, player2);
                    }

                // results:
                // todo - sort on win ratio?
                players.Sort((a, b) => -a.WinRatio().CompareTo(b.WinRatio()));

                Console.WriteLine("Win percentages");
                foreach (var p in players)
                {
                    Console.WriteLine($"  {(100 * p.WinRatio()):F0}% {p.filename}");
                }
            }
            catch
            {
                Console.ForegroundColor = foreground;
            }

        }
    }
}
