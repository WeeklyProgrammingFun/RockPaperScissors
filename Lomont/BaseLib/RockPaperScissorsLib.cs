using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Lomont.RPSBaseLib.RockPaperScissorsLib;

namespace Lomont.RPSBaseLib
{

    /// <summary>
    /// capture a process, provide input and output streams
    /// </summary>
    public class ProcessCapture
    {
        // anything other than black shows debug messages 
        public ConsoleColor color = ConsoleColor.Black;

        /// <summary>
        /// Given filename to execute, do so
        /// </summary>
        /// <param name="filename"></param>
        public ProcessCapture(string filename)
        {

            process.EnableRaisingEvents = true;
            process.OutputDataReceived += process_OutputDataReceived;
            process.ErrorDataReceived += process_ErrorDataReceived;
            process.Exited += process_Exited;

            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = "";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            //below line is optional if we want a blocking call
            //process.WaitForExit();
        }

        /// <summary>
        /// Write a message to the item
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message)
        {
            WriteDebugMessage(message, true);
            process.StandardInput.WriteLine(message);
        }

        // read (and clear) any pending messages
        public List<string> Read(bool waitTillReceived)
        {
            var lines = new List<string>();
            do
            {
                while (messages.TryDequeue(out var result))
                {
                    WriteDebugMessage(result, false);
                    lines.Add(result);
                }
            } while (!waitTillReceived || !lines.Any());
            return lines;
        }

        #region Implementation

        void WriteDebugMessage(string message, bool isInput)
        {
            if (color != ConsoleColor.Black)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                if (isInput)
                    message = "In: " + message;
                else
                    message = "Out:" + message;
                Console.WriteLine(message);
                Console.ForegroundColor = oldColor;
            }
        }

        void process_Exited(object sender, EventArgs e)
        {
            //Console.WriteLine(string.Format("process exited with code {0}\n", process.ExitCode.ToString()));
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // Console.WriteLine("ERROR: " + e.Data + "\n");
        }

        ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            messages.Enqueue(e.Data);
            //Console.WriteLine(e.Data + "\n");
        }

        Process process = new Process();

        #endregion
    }

    /// <summary>
    /// Static types and helper functions
    /// </summary>
    public static class RockPaperScissorsLib
    {

        public static bool TextMoveLegal(string answer)
        {
            answer = answer.ToLower();
            return (answer == "rock" || answer == "paper" || answer == "scissors");
        }

        // list of exe in this directory, minus self
        public static List<string> GetProgramNames()
        {
            var selfName = AppDomain.CurrentDomain.FriendlyName;
            return
                Directory.GetFiles(".\\", "*.exe")
                .Select(n => n.Substring(2))
                .Where(n => n != selfName)
                .ToList();
        }

        // get result for player 1 versus player 2 move
        public static Result Score(Move player1, Move player2)
        {
            return results[(int)player1 * 3 + (int)player2];
        }

        static Result[] results = {
                // player1*3+player2
                //   Rock           Paper             Scissors
                Result.Tie       , Result.Player2Win,  Result.Player1Win,   // Rock      player 1
                Result.Player1Win, Result.Tie       ,  Result.Player2Win,   // Paper     player 1
                Result.Player2Win, Result.Player1Win,  Result.Tie           // Scissors  player 1
            };
    }

    public enum Move
    {
        Rock,
        Paper,
        Scissors
    }
    public enum Result
    {
        Player1Win, Player2Win, Tie
    }

    /// <summary>
    /// Provide a simple rock paper scissors player to derive moves from
    /// </summary>
    public class BaseRPSPlayer
    {
        #region Interface

        /// <summary>
        /// Run the console protocol
        /// </summary>
        /// <param name="args"></param>
        public void Run(string[] args)
        {
            Action DoMove = () =>
            {
                Console.WriteLine(GetMove());
            };


            while (true)
            {
                var line = Console.ReadLine().ToLower();
                if (line.StartsWith("reset"))
                {
                    Reset(line.Substring(5).Trim());
                    DoMove();
                }
                else if (line.StartsWith("rock"))
                {
                    SetMove(Move.Rock);
                    DoMove();
                }
                else if (line.StartsWith("paper"))
                {
                    SetMove(Move.Paper);
                    DoMove();
                }
                else if (line.StartsWith("scissors"))
                {
                    SetMove(Move.Scissors);
                    DoMove();
                }
                else if (line == "quit")
                {  // exit
                    break;
                }
                // other lines ignored
            }
            Closing();
        }

        /// <summary>
        /// Call on reset to new oppponent
        /// </summary>
        /// <param name="opponent"></param>
        public virtual void Reset(string opponent)
        {
        }
        /// <summary>
        /// Call to get move
        /// </summary>
        /// <returns></returns>
        public virtual Move GetMove()
        {
            return Move.Paper;
        }
        /// <summary>
        /// Call to set move
        /// </summary>
        /// <param name="move"></param>
        public virtual void SetMove(Move move)
        {
            Tally(Move.Paper, move);
        }
        public void Closing()
        {
            Console.WriteLine($"{wins} wins, {losses} losses, {ties} ties, {100.0 * wins / (losses + ties + wins):F1}% win %");
        }

        public Result Tally(Move selfMove, Move opponentMove)
        {
            var result = R.Score(selfMove, opponentMove);
            if (result == Result.Player1Win)
                wins++;
            else if (result == Result.Player2Win)
                losses++;
            if (result == Result.Tie)
                ties++;
            return result;
        }

        #endregion

        #region Implementation
        // stats
        protected int wins, losses, ties;
        #endregion
    }


}
