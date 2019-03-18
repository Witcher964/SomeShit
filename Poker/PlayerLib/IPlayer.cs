using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;


namespace PlayerLib
{
    public interface IPlayer
    {
        string Name { get; }
        ulong Money { get; }
        int Position { get; set; }
        int SB { get; set; }
        int BB { get; set; }
        bool IsGaming { get; set; }
        bool IsMyTurn { get; set; }
        bool IsWinner { get; set; }

        ObservableCollection<Card> Hand { get; }
        Enumerations.PlayerTurns Turn { get; set; }
        ulong Bet { get; set; }

        void Blind(ulong blind);
        void AddMoney(ulong money);

        //Bindings bot.
        void TakeTurn(ulong currentRate, IEnumerable<Card> OpenCards, ulong Bank, IEnumerable<ulong> Bets, Enumerations.Rounds round, int curNumOfPlayers, int biddingRound);
        
        ReactiveCommand<Unit, Unit> CallCommand { get; set; }
        ReactiveCommand<Unit, Unit> RaiseCommand { get; set; }
        ReactiveCommand<Unit,Unit> FoldCommand { get; set; }
    }
}
