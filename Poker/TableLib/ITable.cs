using PlayerLib;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;

namespace TableLib
{
    public interface ITable
    {
        ObservableCollection<Card> OpenCards { get; }
        ObservableCollection<IPlayer> Players { get; }
        Enumerations.Rounds Round { get; }
        ulong Bank { get; }

        bool IsGameOn { get; }

        ulong MinRaise { get; }
        ulong MaxRaise { get; }
        ulong PlayerBet { get; }
        ObservableCollection<string> Logs { get; }

        void GameProcess();
        ReactiveCommand<Unit, Unit> GameStartCommand { get; }
    }
}
