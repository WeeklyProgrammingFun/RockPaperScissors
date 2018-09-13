using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Lomont.RPSBaseLib;
using R = Lomont.RPSBaseLib.RockPaperScissorsLib;

namespace WPFRockPaperScissors
{

    class Player : ObservableObject
    {
        public Player(string filename)
        {
            Filename = filename;
            proc = new Lomont.RPSBaseLib.ProcessCapture(filename);
        }

        Lomont.RPSBaseLib.ProcessCapture proc;

        private string filename = "";
        public string Filename
        {
            get { return filename; }
            set { Set(ref filename, value); }
        }

        private bool enabled = true;
        public bool Enabled
        {
            get { return enabled; }
            set { Set(ref enabled, value); }
        }

        public override string ToString()
        {
            return Filename;
        }

        public void Reset(Player opponent)
        {
            proc.Write($"RESET {opponent.Filename}");
        }

        public bool GetMove(out Move move)
        {
            move = Move.Rock;
            var answer = proc.Read(true).FirstOrDefault(s => R.TextMoveLegal(s));

            if (String.IsNullOrEmpty(answer))
                return false;
            answer = answer.ToLower();
            if (answer == "rock")
            {
                move = Move.Rock;
                return true;
            }
            else if (answer == "paper")
            {
                move = Move.Paper;
                return true;
            }
            if (answer == "scissors")
            {
                move = Move.Scissors;
                return true;
            }
            return false;
        }
        public void SetMove(Move opponentMove)
        {
            if (opponentMove == Move.Rock)
                proc.Write("ROCK");
            else if (opponentMove == Move.Paper)
                proc.Write("PAPER");
            else if (opponentMove == Move.Scissors)
                proc.Write("SCISSORS");
        }

        internal void Close()
        {
            proc.Write("QUIT");
        }
    }
}
