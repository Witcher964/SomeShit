using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;

namespace PlayerLib
{
    public class RealPlayer : ReactiveObject, IPlayer
    {
        public string Name { get; private set; }

        private int position = 0;
        public int Position
        {
            get => position;
            set => this.RaiseAndSetIfChanged(ref position, value);
        }

        private int sb = 5;
        public int SB
        {
            get => sb;
            set => this.RaiseAndSetIfChanged(ref sb, value);
        }

        private int bb = 10;
        public int BB
        {
            get => bb;
            set => this.RaiseAndSetIfChanged(ref bb, value);
        }
        private bool isGaming = true;
        public bool IsGaming
        {
            get => isGaming;
            set => this.RaiseAndSetIfChanged(ref isGaming, value);
        }

        private ObservableCollection<Card> hand;
        public ObservableCollection<Card> Hand
        {
            get => hand;
            private set
            {
                hand[0] = value[0];
                hand[1] = value[1];
            }
        }

        private ulong money;
        public ulong Money
        {
            get => money;
            private set => this.RaiseAndSetIfChanged(ref money, value);
        }

        private bool isMyTurn = false;
        public bool IsMyTurn
        {
            get => isMyTurn;
            set => this.RaiseAndSetIfChanged(ref isMyTurn, value);
        }

        private Enumerations.PlayerTurns turn = Enumerations.PlayerTurns.Wait;
        public Enumerations.PlayerTurns Turn
        {
            get => turn;
            set => this.RaiseAndSetIfChanged(ref turn, value);
        }

        private bool isWinner = false;
        public bool IsWinner
        {
            get => isWinner;
            set => this.RaiseAndSetIfChanged(ref isWinner, value);
        }

        private ulong bet = 0UL;
        public ulong Bet
        {
            get => bet;
            set => this.RaiseAndSetIfChanged(ref bet, value);
        }

        public RealPlayer(string _name, ulong _money) : 
            this(_name, new ObservableCollection<Card>(new List<Card>() { new Card(), new Card() }), _money) { }
            
        public RealPlayer(string _name, ObservableCollection<Card> _hand, ulong _money) =>
            (Name, hand, Money) = (_name, _hand, _money);

        public ReactiveCommand<Unit, Unit> CallCommand { get; set; }

        public ReactiveCommand<Unit, Unit> RaiseCommand { get; set; }

        public ReactiveCommand<Unit, Unit> FoldCommand { get; set; }

        public void TakeTurn(ulong currentRate, IEnumerable<Card> OpenCards, ulong Bank, IEnumerable<ulong> Bets, Enumerations.Rounds round, int curNumOfPlayers, int biddingRound)
        {
            while (IsMyTurn) ;
        }

        public void Blind(ulong blind)
        {
            Money -= blind;
            Bet += blind;
        }
        public void AddMoney(ulong bank)
        {
            Money += bank;
        }
    }


    public class Computer : ReactiveObject, IPlayer
    {
        public string Name { get; private set; }

        private int curNumOfPlayers;
        private int pot;
        private int biddingRound;
        private double winChance;
        private int needToBet;
        private bool isBluff;
        private Enumerations.Rounds street;
        private List<int> myHand;
        public List<int> board;
        private Random rand;
        public int botType;

        private bool isGaming = true;
        public bool IsGaming
        {
            get => isGaming;
            set => this.RaiseAndSetIfChanged(ref isGaming, value);
        }

        private ObservableCollection<Card> hand;
        public ObservableCollection<Card> Hand
        {
            get => hand;
            private set
            {
                hand[0] = value[0];
                hand[1] = value[1];
            }
        }

        private ulong money;
        public ulong Money
        {
            get => money;
            private set => this.RaiseAndSetIfChanged(ref money, value);
        }

        private bool isMyTurn = false;
        public bool IsMyTurn
        {
            get => isMyTurn;
            set => this.RaiseAndSetIfChanged(ref isMyTurn, value);
        }

        private int sb = 5;
        public int SB
        {
            get => sb;
            set => this.RaiseAndSetIfChanged(ref sb, value);
        }

        private int bb = 10;
        public int BB
        {
            get => bb;
            set => this.RaiseAndSetIfChanged(ref bb, value);
        }

        private int position = 0;
        public int Position
        {
            get => position;
            set => this.RaiseAndSetIfChanged(ref position, value);
        }
        private Enumerations.PlayerTurns turn = Enumerations.PlayerTurns.Wait;
        public Enumerations.PlayerTurns Turn
        {
            get => turn;
            set => this.RaiseAndSetIfChanged(ref turn, value);
        }

        private bool isWinner = false;
        public bool IsWinner
        {
            get => isWinner;
            set => this.RaiseAndSetIfChanged(ref isWinner, value);
        }

        private ulong bet = 0UL;
        public ulong Bet
        {
            get => bet;
            set => this.RaiseAndSetIfChanged(ref bet, value);
        }

        public Computer(string _name, ulong _money, int bot) :
            this(_name, new ObservableCollection<Card>(new List<Card>() { new Card(), new Card() }), _money, bot)
        {
            //Money = _money;
            //botType = bot;
            //myHand = new List<int> { convertCard(hand[0]), convertCard(hand[1])};
            //winChance = 0;
            //isBluff = false;
            //rand = new Random();
        }

        public Computer(string _name, ObservableCollection<Card> _hand, ulong _money, int bot)
        {
            (Name, hand, Money) = (_name, _hand, _money);
            botType = bot;
            Money = _money;
            myHand = new List<int> { convertCard(hand[0]), convertCard(hand[1]) };
            winChance = 0;
            isBluff = false;
            rand = new Random();
        }
            
        public ReactiveCommand<Unit, Unit> CallCommand { get; set; }

        public ReactiveCommand<Unit, Unit> RaiseCommand { get; set; }

        public ReactiveCommand<Unit, Unit> FoldCommand { get; set; }

        public void betSB()
        {
            if (Money < (ulong)BB)
                throw new Exception("not enough money to do bet");
            Bet += (ulong)SB;
            Money -= (ulong)SB;
        }

        public void betBB()
        {
            if (Money < (ulong)BB)
                throw new Exception("not enough money to do bet");
            Bet += (ulong)BB;
            Money -= (ulong)BB;
        }

        private int isHighCard(List<int> card)
        {
            return card[0];
        }

        private int isOnePair(List<int> card)
        {
            int res = -1;

            for (int i = 0; i < card.Count - 1; i++)
            {
                if (card[i] == card[i + 1])
                {
                    res = card[i];
                    break;
                }
            }

            return res;
        }

        private int isTwoPair(List<int> card)
        {
            int res = -1;
            bool flag = false;

            for (int i = card.Count - 1; i > 0; i--)
            {
                if (card[i] == card[i - 1])
                {
                    if (flag)
                        res = card[i];

                    flag = true;
                    i--;
                }
            }
            return res;
        }

        private int isSet(List<int> card)
        {
            int res = -1;


            for (int i = 0; i < card.Count - 2; i++)
            {
                if (card[i] == card[i + 1] && card[i] == card[i + 2])
                {
                    res = card[i];
                    break;
                }

            }
            return res;
        }

        private int isStraight(List<int> card)
        {
            int res = -1;

            for (int i = 0; i < card.Count - 4; i++)
            {
                if (card[i] == (card[i + 1] + 1) && card[i] == (card[i + 2] + 2) && card[i] == (card[i + 3] + 3) && card[i] == (card[i + 4] + 4))
                {
                    res = card[i];
                    break;
                }
            }
            return res;
        }

        private int isFlush(List<int> card, List<int> suite, List<int> suiteCount)
        {
            int res = -1;
            int n;

            for (n = 0; n < 4; n++)
                if (suiteCount[n] == 5)
                    break;

            if (n == 4)
                return res;

            for (int i = 0; i < suite.Count; i++)
            {
                if (suite[i] == n)
                {
                    res = card[i];
                    break;
                }

            }

            return res;
        }

        private int isFullHouse(List<int> card)
        {
            int res = -1;
            bool flag = false;

            for (int i = 0; i < card.Count - 2; i++)
            {
                if (card[i] == card[i + 1] && card[i] == card[i + 2])
                {
                    res = card[i];
                    break;
                }

            }

            for (int i = 0; i < card.Count - 1; i++)
            {
                if (card[i] == card[i + 1] && card[i] != res)
                {
                    flag = true;
                    break;
                }
            }

            return flag ? res : -1;
        }

        private int isQuads(List<int> card)
        {
            int res = -1;


            for (int i = 0; i < card.Count - 3; i++)
            {
                if (card[i] == card[i + 1] && card[i] == card[i + 2] && card[i] == card[i + 3])
                {
                    res = card[i];
                    break;
                }

            }
            return res;
        }

        private int isStraightFlush(List<int> card, List<int> suite, List<int> suiteCount)
        {
            int res = -1;
            int n;

            for (n = 0; n < 4; n++)
                if (suiteCount[n] == 5)
                    break;

            if (n == 4)
                return res;

            for (int i = 0; i < card.Count - 4; i++)
            {
                if (card[i] == (card[i + 1] + 1) && card[i] == (card[i + 2] + 2) && card[i] == (card[i + 3] + 3) && card[i] == (card[i + 4] + 4))
                {
                    if (suite[i] == n)
                        res = card[i];
                    break;
                }
            }
            return res;
        }

        private void sortHand(List<int> allCard, out List<int> card, out List<int> suite, out List<int> suiteCount)
        {
            card = new List<int>(allCard);
            suite = new List<int>(allCard);
            suiteCount = new List<int>() { 0, 0, 0, 0 };

            for (int i = 0; i < card.Count; i++)
            {
                for (int j = 0; j < card.Count - 1; j++)
                {
                    int t;

                    if (card[j] % 13 < card[j + 1] % 13)
                    {
                        t = card[j + 1];
                        card[j + 1] = card[j];
                        card[j] = t;
                    }

                    if ((card[j] % 13 == card[j + 1] % 13) && (card[j] < card[j + 1]))
                    {
                        t = card[j + 1];
                        card[j + 1] = card[j];
                        card[j] = t;
                    }
                }
            }


            for (int i = 0; i < card.Count; i++)
            {
                suite[i] = card[i] / 13;
                card[i] = card[i] % 13;
                suiteCount[suite[i]]++;
            }
        }

        private int getCombination(List<int> hand, List<int> board)
        {
            List<int> allCard = new List<int>(hand);

            if (board.Count != 0)
                allCard.AddRange(board);

            List<int> card = new List<int>();
            List<int> suite = new List<int>();
            List<int> suiteCount = new List<int>();

            sortHand(allCard, out card, out suite, out suiteCount);

            int result;

            if ((result = isStraightFlush(card, suite, suiteCount)) != -1)
                return 104 + result;

            if ((result = isQuads(card)) != -1)
                return 91 + result;

            if ((result = isFullHouse(card)) != -1)
                return 78 + result;

            if ((result = isFlush(card, suite, suiteCount)) != -1)
                return 65 + result;

            if ((result = isStraight(card)) != -1)
                return 52 + result;

            if ((result = isSet(card)) != -1)
                return 39 + result;

            if ((result = isTwoPair(card)) != -1)
                return 26 + result;

            if ((result = isOnePair(card)) != -1)
                return 13 + result;

            return isHighCard(card);
        }

        private double getProbabilityOfWin()
        {
            Random rand = new Random();

            double winRuns = 0;

            int N = 100000;

            for (int m = 0; m < N; m++)
            {

                int iRand;

                List<int> fullCards = new List<int>(myHand);
                fullCards.AddRange(board);

                List<int> board1 = new List<int>(board);

                //for (int i = 0; i < 2 * (numberOfPlayers - curNumOfPlayers); i++)
                //{
                //    do
                //        iRand = rand.Next(52);
                //    while (fullCards.Contains(iRand));
                //    fullCards.Add(iRand);
                //}

                while (board1.Count != 5)
                {
                    do
                        iRand = rand.Next(52);
                    while (fullCards.Contains(iRand));

                    board1.Add(iRand);
                    fullCards.Add(iRand);
                }

                List<List<int>> otherPlCards = new List<List<int>>();

                for (int i = 0; i < curNumOfPlayers - 1; i++)
                {
                    otherPlCards.Add(new List<int>());

                    for (int j = 0; j < 2; j++)
                    {
                        do
                            iRand = rand.Next(52);
                        while (fullCards.Contains(iRand));

                        otherPlCards[i].Add(iRand);
                        fullCards.Add(iRand);
                    }
                }

                int ourPower = getCombination(myHand, board1);
                double count = 0;
                foreach (var otherCards in otherPlCards)
                {
                    int otherPower = getCombination(otherCards, board1);
                    if (ourPower < otherPower)
                    {
                        count = -1;
                        break;
                    }
                    else if (ourPower == otherPower)
                        count++;

                }

                if (count == -1)
                    continue;
                else if (count > 0)
                    winRuns += 1 / count;
                else
                    winRuns++;
            }

            return winRuns / (double)N;
        }

        private int raise(int bet)
        {
            
            if (((int)Money - bet) < 4 * BB)
            {
                Bet += Money;
                Money = 0;
                return -3;
            }

            Bet += (ulong)bet;
            Money -= (ulong)bet;

            return bet;
        }

        private int call(int needToBet)
        {
            if (needToBet == 0)
                return -2;

            if (((int)Money - needToBet) < 4 * BB)
            {
                Bet += Money;
                Money = 0;
                return -3;
            }

            Bet += (ulong)needToBet;
            Money -= (ulong)needToBet;

            return -1;
        }

        private int fold()
        {
            if (Money < 4 * (ulong)BB)
            {
                Money = 0;
                return -3;
            }

            return 0;
        }

        public int doAction(int curNumOfPlayers, int pot, int position, Enumerations.Rounds street, int biddingRound, int needToBet, List<int> board)
        {
            bool needToGetNewWinChance = true;

            int action = 0;
            double win = 0;

            if (biddingRound > 0 && this.curNumOfPlayers == curNumOfPlayers)
                needToGetNewWinChance = false;

            this.curNumOfPlayers = curNumOfPlayers;
            this.pot = pot;
            this.position = position;
            this.street = street;
            this.biddingRound = biddingRound;
            this.board = board;

            if (needToGetNewWinChance)
                winChance = getProbabilityOfWin();

            win = winChance * pot;

            if (rand.NextDouble() < 0.07 && street == Enumerations.Rounds.PreFlop && biddingRound == 0)
                isBluff = true;

            switch (street)
            {
                //////////// Pre_Flop ////////////
                case Enumerations.Rounds.PreFlop:
                    action = Pre_FlopAction();
                    break;

                //////////// Flop ////////////
                case Enumerations.Rounds.FLop:
                    action = FlopAction(win);
                    break;

                //////////// Turn ////////////
                case Enumerations.Rounds.Turn:
                    action = TurnAction(win);
                    break;

                //////////// River ////////////
                case Enumerations.Rounds.River:
                    action = RiverAction(win);
                    break;

            }

            return action;
        }

        private int Pre_FlopAction()
        {
            if (position < 2)
            {
                if (biddingRound == 0)
                {
                    if (curNumOfPlayers > 6)
                    {
                        if (needToBet > BB)
                        {
                            if (winChance > 0.2 && rand.NextDouble() < 0.85)
                                return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                            else if (winChance > 0.15 && rand.NextDouble() < 0.65)
                                return call(needToBet);
                            else
                                return fold();
                        }
                        else
                        {
                            if (winChance > 0.2 && rand.NextDouble() < 0.95)
                                return raise(3 * BB - (int)Bet);
                            else
                                return call(needToBet);
                        }
                    }
                    else if (curNumOfPlayers > 4)
                    {
                        if (needToBet > BB)
                        {
                            if (winChance > 0.3 && rand.NextDouble() < 0.85)
                                return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                            else if (winChance > 0.25 && rand.NextDouble() < 0.85)
                                return call(needToBet);
                            else
                                return fold();
                        }
                        else
                        {
                            if (winChance > 0.28 && rand.NextDouble() < 0.95)
                                return raise(3 * BB - (int)Bet);
                            else
                                return call(needToBet);
                        }
                    }
                    else
                    {
                        if (needToBet > BB)
                        {
                            if (winChance > 0.75 && rand.NextDouble() < 0.85)
                                return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                            else if (winChance > 0.6 && rand.NextDouble() < 0.85)
                                return call(needToBet);
                            else
                                return fold();
                        }
                        else
                        {
                            if (winChance > 0.5 && rand.NextDouble() < 0.95)
                                return raise(3 * BB - (int)Bet);
                            else if (winChance > 0.4 && rand.NextDouble() < 0.60)
                                return raise(3 * BB - (int)Bet);
                            else
                                return call(needToBet);
                        }
                    }
                }
                else
                {
                    if (curNumOfPlayers > 6)
                    {
                        if (needToBet > BB)
                        {
                            if (winChance > 0.22 || isBluff)
                                return raise((int)Money);
                            else if (winChance > 0.2 && rand.NextDouble() < 0.85)
                                return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                            else if (winChance > 0.2)
                                return call(needToBet);
                            else
                                return fold();
                        }
                        else
                        {
                            if (winChance > 0.2 && rand.NextDouble() < 0.95)
                                return raise(3 * BB - (int)Bet);
                            else
                                return call(needToBet);
                        }
                    }
                    else if (curNumOfPlayers > 4)
                    {
                        if (needToBet > BB)
                        {
                            if (winChance > 0.3 || isBluff)
                                return raise((int)Money);
                            else if (winChance > 0.28 && rand.NextDouble() < 0.85)
                                return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                            else if (winChance > 0.28)
                                return call(needToBet);
                            else
                                return fold();
                        }
                        else
                        {
                            if (winChance > 0.28)
                                return raise(3 * BB - (int)Bet);
                            else
                                return call(needToBet);
                        }
                    }
                    else
                    {
                        if (needToBet > BB)
                        {
                            if (winChance > 0.8 || isBluff)
                                return raise((int)Money);
                            else if (winChance > 0.7 && rand.NextDouble() < 0.80)
                                return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                            else if (winChance > 0.6 && rand.NextDouble() < 0.80)
                                return call(needToBet);
                            else
                                return fold();
                        }
                        else
                        {
                            if (winChance > 0.5)
                                return raise(3 * BB - (int)Bet);
                            else
                                return call(needToBet);
                        }
                    }
                }
            }
            else if (position < 6)
            {
                if (biddingRound == 0)
                {
                    if (curNumOfPlayers > 6)
                    {
                        if (needToBet > BB)
                        {
                            if (winChance > 0.2 && rand.NextDouble() < 0.85)
                                return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                            else
                                return fold();
                        }
                        else
                        {
                            if (winChance > 0.2 && rand.NextDouble() < 0.9)
                                return raise(3 * BB - (int)Bet);
                            else if (winChance > 0.1 && rand.NextDouble() < 0.95)
                                return call(needToBet);
                            else
                                return fold();
                        }
                    }
                    else if (curNumOfPlayers > 4)
                    {
                        if (needToBet > BB)
                        {
                            if (winChance > 0.3 && rand.NextDouble() < 0.85)
                                return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                            else
                                return fold();
                        }
                        else
                        {
                            if (winChance > 0.22 && rand.NextDouble() < 0.95)
                                return raise(3 * BB - (int)Bet);
                            else if (winChance > 0.15 && rand.NextDouble() < 0.85)
                                return call(needToBet);
                            else
                                return fold();
                        }
                    }
                    else
                    {
                        if (needToBet > BB)
                        {
                            if (winChance > 0.65 && rand.NextDouble() < 0.85)
                                return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                            else
                                return fold();
                        }
                        else
                        {
                            if (winChance > 0.5 && rand.NextDouble() < 0.95)
                                return raise(3 * BB - (int)Bet);
                            else if (winChance > 0.35 && rand.NextDouble() < 0.80)
                                return raise(3 * BB - (int)Bet);
                            else
                                return call(needToBet);
                        }
                    }
                }
                else
                {
                    if (curNumOfPlayers > 6)
                    {
                        if (needToBet > BB)
                        {
                            if (winChance > 0.25 || isBluff)
                                return raise((int)Money);
                            else if (winChance > 0.25 && rand.NextDouble() < 0.75)
                                return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                            else
                                return fold();
                        }
                        else
                        {
                            if (winChance > 0.2 && rand.NextDouble() < 0.95)
                                return raise(3 * BB - (int)Bet);
                            else
                                return call(needToBet);
                        }
                    }
                    else if (curNumOfPlayers > 4)
                    {
                        if (needToBet > BB)
                        {
                            if (winChance > 0.35 || isBluff)
                                return raise((int)Money);
                            else if (winChance > 0.33 && rand.NextDouble() < 0.75)
                                return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                            else if (winChance > 0.24 && rand.NextDouble() < 0.75)
                                return call(needToBet);
                            else
                                return fold();
                        }
                        else
                        {
                            if (winChance > 0.28)
                                return raise(3 * BB - (int)Bet);
                            else
                                return call(needToBet);
                        }
                    }
                    else
                    {
                        if (needToBet > BB)
                        {
                            if (winChance > 0.8 || isBluff)
                                return raise((int)Money);
                            else if (winChance > 0.7 && rand.NextDouble() < 0.85)
                                return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                            else if (winChance > 0.6 && rand.NextDouble() < 0.60)
                                return call(needToBet);
                            else
                                return fold();
                        }
                        else
                        {
                            if (winChance > 0.4)
                                return raise(3 * BB - (int)Bet);
                            else
                                return call(needToBet);
                        }
                    }
                }
            }
            else
            {
                if (biddingRound == 0)
                {
                    if (needToBet > BB)
                    {
                        if (winChance > 0.2 && rand.NextDouble() < 0.85)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.2 && rand.NextDouble() < 0.95)
                            return raise(3 * BB - (int)Bet);
                        else if (winChance > 0.1 && rand.NextDouble() < 0.85)
                            return call(needToBet);
                        else
                            return fold();
                    }
                }
                else
                {
                    if (needToBet > BB)
                    {
                        if (winChance > 0.35 || isBluff)
                            return raise((int)Money);
                        else if (winChance > 0.25 && rand.NextDouble() < 0.5)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (winChance > 0.2 && rand.NextDouble() < 0.85)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.2 && rand.NextDouble() < 0.95)
                            return raise(3 * BB - (int)Bet);
                        else
                            return call(needToBet);
                    }
                }
            }
        }

        private int FlopAction(double win)
        {
            if (position < 2)
            {
                if (curNumOfPlayers > 4)
                {
                    if (needToBet > BB)
                    {
                        if (2 * win < needToBet && rand.NextDouble() < 0.9)
                            return fold();
                        else if (1.5 * win < needToBet && rand.NextDouble() < 0.8)
                            return fold();
                        else if (win < needToBet)
                        {
                            if (isBluff && rand.NextDouble() < 0.2)
                                return raise(5 * needToBet);
                            else
                                return fold();
                        }
                        else if (win > 2 * needToBet && rand.NextDouble() < 0.95)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 1.5 * needToBet && rand.NextDouble() < 0.65)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > needToBet && rand.NextDouble() < 0.45 || isBluff)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (rand.NextDouble() < 0.90)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.5 && rand.NextDouble() < 0.65)
                            return raise(3 * BB - (int)Bet);
                        else if (isBluff)
                            return raise(3 * BB - (int)Bet);
                        else
                            return call(needToBet);
                    }
                }
                else
                {
                    if (needToBet > BB)
                    {
                        if (2 * win < needToBet && rand.NextDouble() < 0.9)
                            return fold();
                        else if (1.5 * win < needToBet && rand.NextDouble() < 0.8)
                            return fold();
                        else if (win < needToBet)
                        {
                            if (isBluff && rand.NextDouble() < 0.2)
                                return raise(5 * needToBet);
                            else
                                return fold();
                        }
                        else if (win > 3 * needToBet && rand.NextDouble() < 0.95)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 2.5 * needToBet && rand.NextDouble() < 0.65)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 1.5 * needToBet && rand.NextDouble() < 0.4 || isBluff)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (rand.NextDouble() < 0.80)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.5 && rand.NextDouble() < 0.85)
                            return raise(3 * BB - (int)Bet);
                        else if (isBluff)
                            return raise(5 * BB);
                        else
                            return call(needToBet);
                    }
                }
            }
            else if (position < 5)
            {
                if (curNumOfPlayers > 6)
                {
                    if (needToBet > BB)
                    {
                        if (2 * win < needToBet && rand.NextDouble() < 0.9)
                            return fold();
                        else if (1.5 * win < needToBet && rand.NextDouble() < 0.8)
                            return fold();
                        else if (win < needToBet)
                        {
                            if (isBluff && rand.NextDouble() < 0.2)
                                return raise(5 * needToBet);
                            else
                                return fold();
                        }
                        else if (win > 2 * needToBet && rand.NextDouble() < 0.95)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 1.5 * needToBet && rand.NextDouble() < 0.65)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > needToBet && rand.NextDouble() < 0.45 || isBluff)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (rand.NextDouble() < 0.90)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.5 && rand.NextDouble() < 0.65)
                            return raise(3 * BB - (int)Bet);
                        else if (isBluff)
                            return raise(3 * BB - (int)Bet);
                        else
                            return call(needToBet);
                    }
                }
                else
                {
                    if (needToBet > BB)
                    {
                        if (2 * win < needToBet && rand.NextDouble() < 0.9)
                            return fold();
                        else if (1.5 * win < needToBet && rand.NextDouble() < 0.8)
                            return fold();
                        else if (win < needToBet)
                        {
                            if (isBluff && rand.NextDouble() < 0.2)
                                return raise(5 * needToBet);
                            else
                                return fold();

                        }
                        else if (win > 3 * needToBet && rand.NextDouble() < 0.95)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 2.5 * needToBet && rand.NextDouble() < 0.65)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 1.5 * needToBet && rand.NextDouble() < 0.45 || isBluff)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (rand.NextDouble() < 0.80)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.45 && rand.NextDouble() < 0.9)
                            return raise(3 * BB - (int)Bet);
                        else if (isBluff)
                            return raise(5 * BB);
                        else
                            return call(needToBet);
                    }
                }
            }
            else
            {
                if (needToBet > BB)
                {
                    if (win < needToBet)
                        return fold();
                    else if (win > 2 * needToBet && rand.NextDouble() < 0.95)
                        return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                    else if (win > 1.5 * needToBet && rand.NextDouble() < 0.65)
                        return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                    else if (win > needToBet && rand.NextDouble() < 0.45 || isBluff)
                        return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                    else if (rand.NextDouble() < 0.90)
                        return call(needToBet);
                    else
                        return fold();
                }
                else
                {
                    if (winChance > 0.25 && rand.NextDouble() < 0.85)
                        return raise(3 * BB - (int)Bet);
                    else if (isBluff)
                        return raise(5 * BB);
                    else
                        return call(needToBet);
                }
            }
        }

        private int TurnAction(double win)
        {
            if (position < 2)
            {
                if (curNumOfPlayers > 4)
                {
                    if (needToBet > BB)
                    {
                        if (2 * win < needToBet && rand.NextDouble() < 0.9)
                            return fold();
                        else if (1.5 * win < needToBet && rand.NextDouble() < 0.8)
                            return fold();
                        else if (win < needToBet)
                        {
                            if (isBluff && rand.NextDouble() < 0.2)
                                return raise(5 * needToBet);
                            else
                                return fold();
                        }
                        else if (win > 2 * needToBet && rand.NextDouble() < 0.95)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 1.5 * needToBet && rand.NextDouble() < 0.65)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > needToBet && rand.NextDouble() < 0.45 || isBluff)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (rand.NextDouble() < 0.90)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.5 && rand.NextDouble() < 0.65)
                            return raise(3 * BB - (int)Bet);
                        else if (isBluff)
                            return raise(3 * BB - (int)Bet);
                        else
                            return call(needToBet);
                    }
                }
                else
                {
                    if (needToBet > BB)
                    {
                        if (2 * win < needToBet && rand.NextDouble() < 0.9)
                            return fold();
                        else if (1.5 * win < needToBet && rand.NextDouble() < 0.8)
                            return fold();
                        else if (win < needToBet)
                        {
                            if (isBluff && rand.NextDouble() < 0.2)
                                return raise(5 * needToBet);
                            else
                                return fold();
                        }
                        else if (win > 3 * needToBet && rand.NextDouble() < 0.95)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 2.5 * needToBet && rand.NextDouble() < 0.65)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 1.5 * needToBet && rand.NextDouble() < 0.4 || isBluff)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (rand.NextDouble() < 0.80)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.5 && rand.NextDouble() < 0.85)
                            return raise(3 * BB - (int)Bet);
                        else if (isBluff)
                            return raise(5 * BB);
                        else
                            return call(needToBet);
                    }
                }
            }
            else if (position < 5)
            {
                if (curNumOfPlayers > 6)
                {
                    if (needToBet > BB)
                    {
                        if (2 * win < needToBet && rand.NextDouble() < 0.9)
                            return fold();
                        else if (1.5 * win < needToBet && rand.NextDouble() < 0.8)
                            return fold();
                        else if (win < needToBet)
                        {
                            if (isBluff && rand.NextDouble() < 0.2)
                                return raise(5 * needToBet);
                            else
                                return fold();
                        }
                        else if (win > 2 * needToBet && rand.NextDouble() < 0.95)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 1.5 * needToBet && rand.NextDouble() < 0.65)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > needToBet && rand.NextDouble() < 0.45 || isBluff)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (rand.NextDouble() < 0.90)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.5 && rand.NextDouble() < 0.65)
                            return raise(3 * BB - (int)Bet);
                        else if (isBluff)
                            return raise(3 * BB - (int)Bet);
                        else
                            return call(needToBet);
                    }
                }
                else
                {
                    if (needToBet > BB)
                    {
                        if (2 * win < needToBet && rand.NextDouble() < 0.9)
                            return fold();
                        else if (1.5 * win < needToBet && rand.NextDouble() < 0.8)
                            return fold();
                        else if (win < needToBet)
                        {
                            if (isBluff && rand.NextDouble() < 0.2)
                                return raise(5 * needToBet);
                            else
                                return fold();

                        }
                        else if (win > 3 * needToBet && rand.NextDouble() < 0.95)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 2.5 * needToBet && rand.NextDouble() < 0.65)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 1.5 * needToBet && rand.NextDouble() < 0.45 || isBluff)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (rand.NextDouble() < 0.80)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.45 && rand.NextDouble() < 0.9)
                            return raise(3 * BB - (int)Bet);
                        else if (isBluff)
                            return raise(5 * BB);
                        else
                            return call(needToBet);
                    }
                }
            }
            else
            {
                if (needToBet > BB)
                {
                    if (win < needToBet)
                        return fold();
                    else if (win > 2 * needToBet && rand.NextDouble() < 0.95)
                        return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                    else if (win > 1.5 * needToBet && rand.NextDouble() < 0.65)
                        return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                    else if (win > needToBet && rand.NextDouble() < 0.45 || isBluff)
                        return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                    else if (rand.NextDouble() < 0.90)
                        return call(needToBet);
                    else
                        return fold();
                }
                else
                {
                    if (winChance > 0.25 && rand.NextDouble() < 0.85)
                        return raise(3 * BB - (int)Bet);
                    else if (isBluff)
                        return raise(5 * BB);
                    else
                        return call(needToBet);
                }
            }
        }

        private int RiverAction(double win)
        {
            if (position < 2)
            {
                if (curNumOfPlayers > 4)
                {
                    if (needToBet > BB)
                    {
                        if (2 * win < needToBet && rand.NextDouble() < 0.9)
                            return fold();
                        else if (1.5 * win < needToBet && rand.NextDouble() < 0.8)
                            return fold();
                        else if (win < needToBet)
                        {
                            if (isBluff && rand.NextDouble() < 0.2)
                                return raise(5 * needToBet);
                            else
                                return fold();
                        }
                        else if (win > 2 * needToBet && rand.NextDouble() < 0.95)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 1.5 * needToBet && rand.NextDouble() < 0.65)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > needToBet && rand.NextDouble() < 0.45 || isBluff)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (rand.NextDouble() < 0.90)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.5 && rand.NextDouble() < 0.65)
                            return raise(3 * BB - (int)Bet);
                        else if (isBluff)
                            return raise(3 * BB - (int)Bet);
                        else
                            return call(needToBet);
                    }
                }
                else
                {
                    if (needToBet > BB)
                    {
                        if (2 * win < needToBet && rand.NextDouble() < 0.9)
                            return fold();
                        else if (1.5 * win < needToBet && rand.NextDouble() < 0.8)
                            return fold();
                        else if (win < needToBet)
                        {
                            if (isBluff && rand.NextDouble() < 0.2)
                                return raise(5 * needToBet);
                            else
                                return fold();
                        }
                        else if (win > 3 * needToBet && rand.NextDouble() < 0.95)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 2.5 * needToBet && rand.NextDouble() < 0.65)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 1.5 * needToBet && rand.NextDouble() < 0.4 || isBluff)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (rand.NextDouble() < 0.80)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.5 && rand.NextDouble() < 0.85)
                            return raise(3 * BB - (int)Bet);
                        else if (isBluff)
                            return raise(5 * BB);
                        else
                            return call(needToBet);
                    }
                }
            }
            else if (position < 5)
            {
                if (curNumOfPlayers > 6)
                {
                    if (needToBet > BB)
                    {
                        if (2 * win < needToBet && rand.NextDouble() < 0.9)
                            return fold();
                        else if (1.5 * win < needToBet && rand.NextDouble() < 0.8)
                            return fold();
                        else if (win < needToBet)
                        {
                            if (isBluff && rand.NextDouble() < 0.2)
                                return raise(5 * needToBet);
                            else
                                return fold();
                        }
                        else if (win > 2 * needToBet && rand.NextDouble() < 0.95)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 1.5 * needToBet && rand.NextDouble() < 0.65)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > needToBet && rand.NextDouble() < 0.45 || isBluff)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (rand.NextDouble() < 0.90)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.5 && rand.NextDouble() < 0.65)
                            return raise(3 * BB - (int)Bet);
                        else if (isBluff)
                            return raise(3 * BB - (int)Bet);
                        else
                            return call(needToBet);
                    }
                }
                else
                {
                    if (needToBet > BB)
                    {
                        if (2 * win < needToBet && rand.NextDouble() < 0.9)
                            return fold();
                        else if (1.5 * win < needToBet && rand.NextDouble() < 0.8)
                            return fold();
                        else if (win < needToBet)
                        {
                            if (isBluff && rand.NextDouble() < 0.2)
                                return raise(5 * needToBet);
                            else
                                return fold();

                        }
                        else if (win > 3 * needToBet && rand.NextDouble() < 0.95)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 2.5 * needToBet && rand.NextDouble() < 0.65)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (win > 1.5 * needToBet && rand.NextDouble() < 0.45 || isBluff)
                            return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                        else if (rand.NextDouble() < 0.80)
                            return call(needToBet);
                        else
                            return fold();
                    }
                    else
                    {
                        if (winChance > 0.45 && rand.NextDouble() < 0.9)
                            return raise(3 * BB - (int)Bet);
                        else if (isBluff)
                            return raise(5 * BB);
                        else
                            return call(needToBet);
                    }
                }
            }
            else
            {
                if (needToBet > BB)
                {
                    if (win < needToBet)
                        return fold();
                    else if (win > 2 * needToBet && rand.NextDouble() < 0.95)
                        return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                    else if (win > 1.5 * needToBet && rand.NextDouble() < 0.65)
                        return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                    else if (win > needToBet && rand.NextDouble() < 0.45 || isBluff)
                        return raise(3 * (needToBet + (int)Bet) - (int)Bet);
                    else if (rand.NextDouble() < 0.90)
                        return call(needToBet);
                    else
                        return fold();
                }
                else
                {
                    if (winChance > 0.25 && rand.NextDouble() < 0.85)
                        return raise(3 * BB - (int)Bet);
                    else if (isBluff)
                        return raise(5 * BB);
                    else
                        return call(needToBet);
                }
            }
        }

        private int convertCard(Card card)
        {
            int val;
            val = ((int)card.Rank - 2) + 13 * ((int)card.Suit);
            return val;
        }

        public void TakeTurn(ulong currentRate, IEnumerable<Card> OpenCards, ulong Bank, IEnumerable<ulong> Bets, Enumerations.Rounds round, int curNumOfPlayers, int biddingRound)
        {
            if (botType == 1)
            {
                needToBet = (int)currentRate;
                rand = new Random();
                if (Money == 0)
                {
                    Turn = Enumerations.PlayerTurns.Check;
                    return;
                }
                myHand = new List<int> { convertCard(hand[0]), convertCard(hand[1]) };
                List<int> b = new List<int>();
                foreach (var c in OpenCards)
                    b.Add(convertCard(c));
                int sum = (int)Bank;
                foreach (var bet in Bets)
                    sum += (int)bet;
                int act = doAction(curNumOfPlayers, sum, position, round, biddingRound, (int)currentRate, b);

                if (act == -3)
                {
                    Turn = Enumerations.PlayerTurns.Raise;
                }
                else if (act == -2)
                {
                    Turn = Enumerations.PlayerTurns.Check;
                }
                else if (act == -1)
                {
                    Turn = Enumerations.PlayerTurns.Call;
                }
                else if (act == 0)
                {
                    Turn = Enumerations.PlayerTurns.Fold;
                    return;
                }
                else
                {
                    Turn = Enumerations.PlayerTurns.Raise;
                }
                IsMyTurn = false;
            }
            else
            {
                if (Money < currentRate)
                {
                    Turn = Enumerations.PlayerTurns.Fold;
                }
                ulong sum = 0;
                foreach (var item in Bets)
                {
                    sum += item;
                }
                Turn = (Enumerations.PlayerTurns)
                Bot.action(
                    String.Join(" ", Hand[0], Hand[1]),
                    OpenCards.Count() == 0 ? "" : String.Join(" ", OpenCards.Select(s => s)),
                    Bank + sum,
                    Convert.ToInt32(currentRate),
                    Convert.ToInt32(2 * currentRate),
                    Bets.Count()
                    );

                if (Turn == Enumerations.PlayerTurns.Call)
                {
                    if (Money < currentRate)
                    {
                        Turn = Enumerations.PlayerTurns.Fold;
                        return;
                    }
                    Money -= currentRate;
                    Bet += currentRate;
                }
                else if (Turn == Enumerations.PlayerTurns.Raise)
                {
                    if (Money < 2 * currentRate)
                    {
                        currentRate = Money;
                    }
                    else currentRate = currentRate == 0 ? Money < 10 ? Money : 10 : currentRate * 2;

                    Money -= currentRate;
                    Bet += currentRate;
                }
                IsMyTurn = false;
            }
        }

        public void Blind(ulong blind)
        {
            if (blind == (ulong)SB)
                betSB();
            else
                betBB();
        }

        public void AddMoney(ulong bank)
        {
            Money += bank;
        }
    }
}
