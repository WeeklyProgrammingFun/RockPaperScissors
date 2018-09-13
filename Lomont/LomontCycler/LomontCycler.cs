using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.RPSBaseLib;

namespace LomontCycler
{
    class LomontCycler : BaseRPSPlayer
    {
        Move lastMove;
        int dir;
        Random rand = new Random();

        public override void Reset(string opponent)
        {
            lastMove = GetMove();
            dir = (rand.Next() & 1) * 2 - 1; // -1 or 1
        }
        public override Move GetMove()
        {
            lastMove = (Move)(((int)lastMove + dir + 3)%3);
            return lastMove;
        }
        public override void SetMove(Move move)
        {
            Tally(lastMove, move); // track stats for fun
        }

        static void Main(string[] args)
        {
            var player = new LomontCycler();
            player.Run(args);
        }
    }
}
