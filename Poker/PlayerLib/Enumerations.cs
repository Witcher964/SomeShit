using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerLib
{
    public static class Enumerations
    {
        public enum Suits { Spade, Heart, Diamond, Club }
        public enum Ranks { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Quuen, King, Ace }
        public enum Rounds { PreFlop, FLop, Turn, River, End }
        public enum PlayerTurns { Wait, Call, Fold, Check, Raise }
    }
}
