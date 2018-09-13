using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.RPSBaseLib;

namespace LomontConstant
{
    /// <summary>
    /// Constant reply throughout a match
    /// </summary>
    class LomontConstant : BaseRPSPlayer
    {
        Move lastMove;
        Random rand = new Random();

        public override void Reset(string opponent)
        {
            lastMove = (Move)(rand.Next() % 3);
        }
        public override Move GetMove()
        {
            return lastMove;
        }
        public override void SetMove(Move move)
        {
            Tally(lastMove, move); // track stats for fun
        }

        static void Main(string[] args)
        {
            var player = new LomontConstant();
            player.Run(args);
        }
    }

}
