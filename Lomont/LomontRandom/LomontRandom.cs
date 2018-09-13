using System;
using Lomont.RPSBaseLib;

namespace LomontRandom
{
    /// <summary>
    /// Random answer
    /// </summary>
    class LomontRandom : BaseRPSPlayer
    {
        Move lastMove;
        Random rand = new Random();

        public override void Reset(string opponent)
        {
        }
        public override Move GetMove()
        {
            lastMove = (Move)(rand.Next() % 3);
            return lastMove;
        }
        public override void SetMove(Move move)
        {
            Tally(lastMove, move); // track stats for fun
        }

        static void Main(string[] args)
        {
            var player = new LomontRandom();
            player.Run(args);
        }
    }
}
