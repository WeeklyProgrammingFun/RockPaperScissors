using System;
using Lomont.RPSBaseLib;

namespace LomontBeatLast
{
    /// <summary>
    /// Play last move answer
    /// </summary>
    class LomontEcho : BaseRPSPlayer
    {
        Move lastMove;
        Random rand = new Random();

        public override void Reset(string opponent)
        {
            lastMove = (Move)(rand.Next() % 3);
        }
        public override Move GetMove()
        {
            // play last heard
            return lastMove;
        }
        public override void SetMove(Move move)
        {
            Tally(lastMove, move); // track stats for fun
            lastMove = move;
        }

        static void Main(string[] args)
        {
            var player = new LomontEcho();
            player.Run(args);
        }
    }
}
