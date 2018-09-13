using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Lomont.RPSBaseLib;

namespace WPFRockPaperScissors
{
    // manage a cross table
    class CrossTable : ObservableObject
    {
        public void Clear()
        {
            Players.Clear();
            Round = 0;
        }

        // add all players here
        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }

        public List<Player> Players { get; } = new List<Player>();

        public int Round { get; private set; }

        public List<HeadToHeadScore> Scores {get; } = new List<HeadToHeadScore>();

        // Fischer yates shuffle
        static Random rand = new Random();
        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // list of game pairings, null when no more
        // player is paired with null for a bye
        public List<Tuple<Player,Player>> NextRoundSchedule()
        {
            if (Round == 0)
            { // if odd, on first round, add enough to make
                if ((Players.Count & 1) == 1)
                    Players.Add(null);

                // and shuffle
                Shuffle(Players);

            }
            if (Round == Players.Count - 1)
                return null; // no more
            ++Round; // prepare round 
            var rotator = Players[1];
            Players.RemoveAt(1);
            Players.Add(rotator);

            // now pair first half with second
            var schedule = new List<Tuple<Player, Player>>();
            var count = Players.Count;
            for (var i = 0; i < count/2; ++i)
                schedule.Add(new Tuple<Player,Player>(Players[i], Players[count - i - 1]));
            return schedule;
        }

        internal void AddScore(HeadToHeadScore headToHeadScore)
        {
            Scores.Add(headToHeadScore);
        }
    }
}
