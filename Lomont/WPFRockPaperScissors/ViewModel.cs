using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace WPFRockPaperScissors
{
    class ViewModel : ViewModelBase
    {

        public ViewModel()
        {
            LoadPlayersCommand = new RelayCommand(LoadPlayers);
            RunCommand = new RelayCommand(RunMatches);
            ClearMessagesCommand = new RelayCommand(() => Messages.Clear());
            ClearAllCommand= new RelayCommand(() => Select(0));
            SelectAllCommand = new RelayCommand(() => Select(1));
            InvertAllCommand = new RelayCommand(() => Select(2));
            CrossTable = new CrossTable();
            Messages.Add("Starting");
            if (!IsInDesignMode)
                LoadPlayers();
        }
        public ObservableCollection<Player> Players { get; } = new ObservableCollection<Player>();
        public ICommand LoadPlayersCommand { get; }
        public ICommand RunCommand{ get; }
        public ICommand ClearMessagesCommand{ get; }
        public ICommand ClearAllCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand InvertAllCommand { get; }

        void Select(int type)
        {
            // tpye 0,1,2 = clear,set,invert
            foreach (var p in Players)
            {
                if (type == 0) p.Enabled = false;
                if (type == 1) p.Enabled = true;
                if (type == 2) p.Enabled = !p.Enabled;
            }

        }

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        // Define the cancellation token.
        CancellationTokenSource cancellationSource = new CancellationTokenSource();

        // process on ui thread
        void Dispatch(Action action)
        {
            if (action == null) return;
            Dispatcher dispatchObject = Application.Current.Dispatcher;
            if (dispatchObject == null || dispatchObject.CheckAccess())
            {
                action();
            }
            else
            {
                dispatchObject.Invoke(action);
            }
        }


        async void RunMatches()
        {
            try
            {
                NotRunning = false;
                CrossTable.Clear();
                foreach (var player in Players)
                {
                    if (player.Enabled)
                        CrossTable.AddPlayer(player);
                }

                if (CrossTable.Players.Count < 2)
                {
                    MessageBox.Show("Not enough players selected");
                    return;
                }

                var runner = new Runner();

                Progress = 0.0;
                Progress<Runner.RunProgress> progress = new Progress<Runner.RunProgress>(progressUpdate =>
                {
                    Dispatch(() =>
                    {
                        Progress = progressUpdate.RatioCompleted*100;
                        CrossTable.AddScore(progressUpdate.HeadToHeadScore);
                        //Messages.Add(progressUpdate.HeadToHeadScore.ToString());
                        MakeScoring();
                    });
                });

                await runner.RunAsync(rounds, CrossTable, cancellationSource.Token, progress);

            }
            catch (Exception ex)
            {
                Messages.Add($"EXCEPTION: {ex}");
            }
            NotRunning = true;
        }

        void LoadPlayers()
        {
            Players.Clear();
            var names = Lomont.RPSBaseLib.RockPaperScissorsLib.GetProgramNames();
            names = names.Where(n => n.ToLower() != "rpsrunner.exe").ToList();
            foreach (var name in names)
                Players.Add(new Player(name));
        }

        internal void Closing()
        {
            foreach (var p in Players)
                p.Close();
        }

        CrossTable CrossTable { get;  }

        private bool notRunning = true;
        public bool NotRunning
        {
            get { return notRunning; }
            set { Set(ref notRunning, value); }
        }

        private double progress = 0.0;
        public double Progress
        {
            get { return progress; }
            set { Set(ref progress, value); }
        }

        private int rounds = 1000;
        public int Rounds
        {
            get { return rounds; }
            set { Set(ref rounds, value); }
        }


        public class ScoringRow
        {
            public ScoringRow(Player player)
            {
                Player = player;
                TotalScore = new Score(Player);
            }
            public int Index { get; set; }
            public Player Player { get; set; }
            public Score TotalScore { get; }

            // opponent score, index 1+,self,opponent
            public ObservableCollection<Tuple<Score,int,Player,Player>> OpponentScores { get; } = new ObservableCollection<Tuple<Score,int,Player,Player>>();
        }

        public ObservableCollection<ScoringRow> ScoringRows { get; } = new ObservableCollection<ScoringRow>();

        // fill in scoring row from cross table
        void MakeScoring()
        {
            // make total scores
            var scores = new List<ScoringRow>();
            var players = CrossTable.Players.Where(pp => pp != null).ToList();
            foreach (var player in players)
            {
                var row = new ScoringRow(player);
                foreach (var s in CrossTable.Scores)
                {
                    if (s.player1Score.Player == player)
                        row.TotalScore.Add(s.player1Score);
                    else if (s.player2Score.Player == player)
                        row.TotalScore.Add(s.player2Score);
                }
                scores.Add(row);
            }

            // sort on total
            scores.Sort((a, b) => -a.TotalScore.WinRatio.CompareTo(b.TotalScore.WinRatio));

            // make space for each head to head
            var num = scores.Count;
            foreach (var s in scores)
            {
                for (var j = 0; j < num; ++j)
                    s.OpponentScores.Add(null);
            }

            // fill in any head to head
            foreach (var s in CrossTable.Scores)
            {
                var player1Index = scores.FindIndex(sc => sc.Player == s.player1Score.Player);
                var player2Index = scores.FindIndex(sc => sc.Player == s.player2Score.Player);
                scores[player1Index].OpponentScores[player2Index] = new Tuple<Score, int, Player,Player>(s.player1Score, player2Index+1, s.player1Score.Player, s.player2Score.Player);
                scores[player2Index].OpponentScores[player1Index] = new Tuple<Score, int, Player,Player>(s.player2Score, player1Index+1, s.player2Score.Player, s.player1Score.Player);
            }

            // copy out
            ScoringRows.Clear();
            for (var index = 0; index < scores.Count; ++index)
            {
                var s = scores[index];
                s.Index = index+1;
                ScoringRows.Add(s);
            }
        }
    }
}
