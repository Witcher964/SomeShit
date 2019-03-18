using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using PlayerLib;
using ReactiveUI;

namespace TableLib
{
    public class Table : ReactiveObject, ITable
    {
        private readonly Card?[] deck = new Card?[52];

        public List<int> position;
        public ObservableCollection<Card> OpenCards { get; } = new ObservableCollection<Card>();

        public ObservableCollection<IPlayer> Players { get; } = new ObservableCollection<IPlayer>();

        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

        private Enumerations.Rounds round;
        public Enumerations.Rounds Round
        {
            get => round;
            private set => this.RaiseAndSetIfChanged(ref round, value);
        }

        private ulong bank;
        public ulong Bank
        {
            get => bank;
            private set => this.RaiseAndSetIfChanged(ref bank, value);
        }

        private bool isGameOn = false;
        public bool IsGameOn
        {
            get => isGameOn;
            private set => this.RaiseAndSetIfChanged(ref isGameOn, value);
        }

        private ulong minRaise;
        public ulong MinRaise
        {
            get => minRaise;
            set => this.RaiseAndSetIfChanged(ref minRaise, value);
        }

        private ulong maxRaise;
        public ulong MaxRaise
        {
            get => maxRaise;
            set => this.RaiseAndSetIfChanged(ref maxRaise, value);
        }

        private ulong playerBet;
        public ulong PlayerBet
        {
            get => playerBet;
            set => this.RaiseAndSetIfChanged(ref playerBet, value);
        }


        private  ulong smallBlind = 5;
        private  ulong bigBlind = 10;
        private readonly uint numOfPlayers = 8;
        private readonly ulong banks = 750UL;

        private ulong curbet;
        private ulong CurBet
        {
            get => curbet;
            set => this.RaiseAndSetIfChanged(ref curbet, value);
        }

        private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        private Random rand = new Random();

        public Table()
        {
            AddPlayers();

            this.ObservableForProperty(t => t.CurBet)
                .Subscribe(_ => { PlayerBet = MinRaise = CurBet == 0 ? 10 : Math.Min(2 * CurBet, Players[0].Money); MaxRaise = /*Players[0].Money - Players[0].Bet >= 0 ? Players[0].Money - Players[0].Bet :*/ Players[0].Money; });

            GameStartCommand = ReactiveCommand.CreateFromTask(
                async () =>
                await Task.Run(
                    () =>
                    GameProcess()
                    ));

            var canExecute = this.WhenAny(
                table => table.Players[0].IsMyTurn,
                table => table.Players[0].Money,
                (isPTurn,mon) => isPTurn.Value && mon.Value > 0
                ).ObserveOnDispatcher();

            Players[0].FoldCommand = ReactiveCommand.CreateFromTask(
                async () =>
                await Task.Run(
                    () =>
                    {
                        Players[0].Turn= Enumerations.PlayerTurns.Fold;
                        Players[0].IsMyTurn = false;
                    }),
                canExecute);

            Players[0].CallCommand = ReactiveCommand.CreateFromTask(
                async () =>
                await Task.Run(
                    () =>
                    {
                        if (CurBet - Players[0].Bet != 0)
                        {
                            Players[0].Turn = Enumerations.PlayerTurns.Call;
                            if (Players[0].Money  < CurBet)
                                Players[0].Blind(Players[0].Money);
                            else
                                Players[0].Blind(CurBet - Players[0].Bet);
                        }
                        else Players[0].Turn = Enumerations.PlayerTurns.Check;
                        Players[0].IsMyTurn = false;
                    }),
                canExecute);

            Players[0].RaiseCommand = ReactiveCommand.CreateFromTask(
                async () =>
                await Task.Run(
                    () =>
                    {
                        Players[0].Turn = Enumerations.PlayerTurns.Raise;
                        PlayerBet = PlayerBet == Players[0].Money ? Players[0].Money + Players[0].Bet : PlayerBet;
                        Players[0].Blind(PlayerBet - Players[0].Bet);
                        Players[0].IsMyTurn = false;
                    }),
                canExecute);
        }

        public ReactiveCommand<Unit, Unit> GameStartCommand { get; private set; }

        public void GameProcess()
        {
            int game = 1;
            dispatcher.Invoke(() => Logs.Clear());
            dispatcher.Invoke(() => IsGameOn = true);

            while (IsGameOn)
            {
                if (game % 5 == 0)
                {
                    smallBlind *= 2;
                    bigBlind *= 2;
                    foreach (var a in Players.Where(p => p.IsGaming))
                    {
                        a.SB *= 2;
                        a.BB *= 2;
                    }
                }
                dispatcher.Invoke(() => Logs.Add($"======= Game {game++} ======="));
                ResetGame();
                Task.Delay(2000).Wait();
                #region Giving Cards
                var ActivePlayers = Players.Where(p => p.IsGaming);
                if(ActivePlayers.Count() == 1)
                {
                    dispatcher.Invoke(() => Logs.Add($"======= {ActivePlayers.First().Name} is WINNING! TOTAL : {ActivePlayers.First().Money}$ ======="));
                    isGameOn = false; break;
                }
                foreach(var player in ActivePlayers)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        player.Hand[j] = TakeCard();
                        if (player is RealPlayer) Task.Delay(500).Wait();
                    }
                }
                #endregion
                while (isGameOn && Round != Enumerations.Rounds.End)
                {
                    dispatcher.Invoke(() => Logs.Add($"####### {Round} #######"));
                    PlayersBet();
                    Task.Delay(1000).Wait();
                    foreach (var player in Players)
                    {
                        Bank += player.Bet;
                        player.Bet = 0UL;
                        if (player.Turn != Enumerations.PlayerTurns.Fold)
                            player.Turn = Enumerations.PlayerTurns.Wait;
                    }
                    Task.Delay(1000).Wait();
                    AddOpenCards();
                    Round++;
                }
                GetWinner();
            }
        }

        private void GenerateDeck()
        {
            int k = 0;
            for (int i = 2; i < 15; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    deck[k++] = new Card() { Suit = (Enumerations.Suits)j, Rank = (Enumerations.Ranks)i };
                }
            }
        }

        private void ResetGame()
        {
            Round = Enumerations.Rounds.PreFlop;
            Bank = CurBet = 0UL;
            OpenCards.Clear();
            GenerateDeck();
            //Mark Players with money < bigBlind and reset Hand.
            foreach(var player in Players.Where(p => p.IsGaming))
            {
                if (player.Money < bigBlind)
                {
                    player.IsGaming = false;
                    player.Turn = Enumerations.PlayerTurns.Fold;
                    //dispatcher.Invoke(() => Logs.Add($"------- {player.Name} - Stands Up -------"));
                }
                else
                {
                    player.Turn = Enumerations.PlayerTurns.Wait;
                    player.IsWinner = false;
                    for (int j = 0; j < 2; j++)
                    {
                        player.Hand[j] = new Card();
                    }
                }
            }
        }

        private void AddPlayers()
        {
            Players.Clear();

            Players.Add(new RealPlayer("Me", banks));
            for (int i = 0; i < 7; i++)
            {
                if (i < numOfPlayers - 1)
                {
                    if(i % 2 == 0)
                        Players.Add(new Computer($"Computer{i + 1}", banks, 1));
                    else
                        Players.Add(new Computer($"Computer{i + 1}", banks, 2));
                }
                else
                    Players.Add(new Computer($"Computer{i + 1}", 0UL, 1));
            }
        }

        private void PlayersBet()
        {
            var PlayingPlayers = Players.Where(p => p.Turn != Enumerations.PlayerTurns.Fold);
            int biddingRound = 0;
            CurBet = 0UL;
            if (Round == Enumerations.Rounds.PreFlop)
            {
                int BlindIndex = rand.Next(0, PlayingPlayers.Count());
                position = new List<int>();
                PlayingPlayers = PlayingPlayers.Skip(BlindIndex).Concat(PlayingPlayers.Take(BlindIndex));
                int ps = 0;
                foreach (var p in PlayingPlayers)
                {
                    p.Position = ps;
                    ps++;
                }
                //smallBlind.
                PlayingPlayers.First().Blind(smallBlind);
                dispatcher.Invoke(() => Logs.Add($"------- {PlayingPlayers.First().Name} - SmallBlind : {smallBlind}$ -------"));

                Task.Delay(1500).Wait();
                //bigBlind.
                PlayingPlayers.Skip(1).First().Blind(bigBlind);
                dispatcher.Invoke(() => Logs.Add($"------- {PlayingPlayers.Skip(1).First().Name} - BigBlind   : {bigBlind}$ -------"));

                Task.Delay(1500).Wait();
                CurBet = bigBlind;
                PlayingPlayers = PlayingPlayers.Skip(2).Concat(PlayingPlayers.Take(2));
            }
            else
            {
                int ps = 0;
                foreach (var p in PlayingPlayers)
                {
                    p.Position = ps;
                    ps++;
                }
            }
            if (Players.Where(p => p.Turn != Enumerations.PlayerTurns.Fold).Sum(p => p.Money != 0 ? 1 : 0) < 2)
                return;
            var z = PlayingPlayers.ToList();
            while(!Players.Where(p => p.Turn != Enumerations.PlayerTurns.Fold).All(p => p.Turn == Enumerations.PlayerTurns.Check || p.Turn == Enumerations.PlayerTurns.Call))
            {
                foreach (var player in z)
                {
                    if (player.Turn == Enumerations.PlayerTurns.Fold) continue;
                    player.IsMyTurn = true;

                    if (player is RealPlayer)
                    {
                        if(player.Money == 0)
                        {
                            player.Turn = Enumerations.PlayerTurns.Check;
                            player.IsMyTurn = false;
                        }
                        while (player.IsMyTurn) ;
                    }
                    else
                    {
                        player.TakeTurn(CurBet - player.Bet, OpenCards, Bank, Players.Select(p => p.Bet), Round, PlayingPlayers.Count(), biddingRound);
                    }

                    if (player.Turn != Enumerations.PlayerTurns.Fold)
                    {
                        if (player.Turn == Enumerations.PlayerTurns.Raise)
                        {
                            if (Players.Where(p => p.Turn != Enumerations.PlayerTurns.Fold && p.Name != player.Name).Sum(p => p.Money + p.Bet < player.Bet ? 1 : 0) != 0)
                            {
                                player.AddMoney(player.Bet - Players.Where(p => p.Name != player.Name && p.Turn != Enumerations.PlayerTurns.Fold).Min(p => p.Bet + p.Money));
                                player.Bet = Players.Where(p => p.Name != player.Name && p.Turn != Enumerations.PlayerTurns.Fold).Min(p => p.Bet + p.Money);                                
                            }
                            CurBet = player.Bet;
                        }
                        dispatcher.Invoke(() => Logs.Add($"------- {player.Name} - {player.Turn}s : {CurBet}$ -------"));
                    }
                    else 
                    {
                        dispatcher.Invoke(() => Logs.Add($"------- {player.Name} - Folds  -------"));

                        if (Players.Sum(p => p.Turn != Enumerations.PlayerTurns.Fold ? 1 : 0) == 1)
                        {
                            Round = Enumerations.Rounds.River;
                            return;
                        }
                    }
                    Task.Delay(1500).Wait();
                    if (Players.Sum(p => p.Turn == Enumerations.PlayerTurns.Raise ? 1 : 0) > 0 &&
                        Players.Where(p => p.Turn != Enumerations.PlayerTurns.Fold).Sum(p => p.Bet != curbet ? 1 : 0) == 0)
                        return;
                }
                biddingRound++;
            }
        }

        private void AddOpenCards()
        {
            switch (Round)
            {
                case Enumerations.Rounds.PreFlop:
                    for (int i = 0; i < 3; i++)
                    {
                        OpenCards.Add(TakeCard());
                        Task.Delay(500).Wait();
                    }
                    break;
                case Enumerations.Rounds.FLop:
                case Enumerations.Rounds.Turn:
                    OpenCards.Add(TakeCard());
                    Task.Delay(500).Wait();
                    break;
                default:
                    break;
            }
        }

        private Card TakeCard()
        {
            int k1, k2;
            Card? DeletedCard;
            do
            {
                k1 = rand.Next(0, 4);
                k2 = rand.Next(0, 13);
                DeletedCard = deck[k1 + 4 * k2];
            } while (!DeletedCard.HasValue);

            deck[k1 + 4 * k2] = null;
            return DeletedCard.Value;
        }

        private void GetWinner()
        {
            if (!IsGameOn) return;
            IPlayer winner = null;

            var PlayingPlayers = Players.Where(p => p.Turn != Enumerations.PlayerTurns.Fold);

            if (PlayingPlayers.Count() == 1)
            {
                winner = PlayingPlayers.First();
                winner.IsWinner = true;
                dispatcher.Invoke(() => Logs.Add($"------- {winner.Name} - Wins   : {Bank}$ -------"));
                Task.Delay(5000).Wait();
                winner.AddMoney(Bank);
                return;
            }

            var vc = PlayingPlayers
                .Select(p => (HandEvaluator.EvaluateHand(OpenCards, p.Hand), p))
                .OrderByDescending(t => t.Item1)
                .GroupBy(t => t.Item1);

            var WinnersCombination = PlayingPlayers
                .Select(p => (HandEvaluator.EvaluateHand(OpenCards, p.Hand), p))
                .OrderByDescending(t => t.Item1)
                .GroupBy(t => t.Item1)
                .First();

            var PotentialWinners = WinnersCombination.Select(T => T.ToTuple().Item2);

            (Enumerations.Ranks, Enumerations.Ranks) TMax = (Enumerations.Ranks.Two, Enumerations.Ranks.Two);

            foreach (var potentialWinner in PotentialWinners)
            {
                var T = potentialWinner.Hand[0].Rank > potentialWinner.Hand[1].Rank ?
                    (potentialWinner.Hand[0].Rank, potentialWinner.Hand[1].Rank) :
                    (potentialWinner.Hand[1].Rank, potentialWinner.Hand[0].Rank);

                if (T.Item1 > TMax.Item1)
                {
                    TMax = T;
                    winner = potentialWinner;
                }
                else if (T.Item1 == TMax.Item1)
                {
                    if (T.Item2 > TMax.Item2)
                    {
                        TMax.Item2 = T.Item2;
                        winner = potentialWinner;
                    }
                    else if (T.Item2 == TMax.Item2)
                    {
                        dispatcher.Invoke(() => Logs.Add($"------- DRAW!!! -------"));
                        winner.IsWinner = true;
                        potentialWinner.IsWinner = true;
                        winner.AddMoney(Bank / 2);
                        potentialWinner.AddMoney(Bank / 2);
                        return;
                    }
                }
            }


            var comb = PotentialWinners.Count() == 1 ? WinnersCombination.First().ToTuple().Item1.combination.ToString() : "High Card: " + TMax.ToString();
            winner.IsWinner = true;
            dispatcher.Invoke(() => Logs.Add($"------- {winner.Name} - Wins   : {Bank}$ -------"));
            dispatcher.Invoke(() => Logs.Add($"------- With {comb}-------"));
            Task.Delay(20000).Wait();
            winner.AddMoney(Bank);
        }
    }
}
