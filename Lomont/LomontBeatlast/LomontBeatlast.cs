using System;
using Lomont.RPSBaseLib;

namespace LomontBeatLast
{
    /// <summary>
    /// Beat last move answer
    /// </summary>
    class LomontBeatLast : BaseRPSPlayer
    {
        Move lastMove;
        Random rand = new Random();

        public override void Reset(string opponent)
        {
            lastMove = (Move)(rand.Next() % 3);
        }
        public override Move GetMove()
        {
            // if last was rock, play paper, etc.
            lastMove = (Move)(((int)lastMove+1)%3);
            return lastMove;
        }
        public override void SetMove(Move move)
        {
            Tally(lastMove, move); // track stats for fun
            lastMove = move;
        }

        static void Main(string[] args)
        {
            var player = new LomontBeatLast();
            player.Run(args);
        }
    }
}
