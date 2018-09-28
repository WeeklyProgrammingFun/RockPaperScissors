using System;
using System.Collections.Generic;
using Lomont.RPSBaseLib;

namespace LomontLongcycle
{
    /// <summary>
    /// Constant reply throughout a match
    /// </summary>
    class LomontLongCycle : BaseRPSPlayer
    {
        Move lastMove;
        Random rand = new Random();
        List<Move> moves = new List<Move>();
        int index;

        public override void Reset(string opponent)
        {
            var len = rand.Next(50, 150);
            moves.Clear();
            for (var i = 0; i < len; ++i)
                moves.Add((Move)(rand.Next() % 3));
            index = 0;
        }
        public override Move GetMove()
        {
            index = (index + 1) % moves.Count;
            return moves[index];
        }
        public override void SetMove(Move move)
        {
            Tally(lastMove, move); // track stats for fun
        }

        static void Main(string[] args)
        {
            var player = new LomontLongCycle();
            player.Run(args);
        }
    }

}
