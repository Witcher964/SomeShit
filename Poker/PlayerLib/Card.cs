using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerLib
{
    public struct Card
    {
        public Enumerations.Ranks Rank;
        public Enumerations.Suits Suit;
        public string Path
        {
            get => String.Join("", "Images/", $"{char.ToLower(Suit.ToString()[0])}{(int)Rank:00}.bmp");
        }

        public override string ToString() =>
            string.Join(
                "", 
                Rank < Enumerations.Ranks.Ten ? ((int)Rank).ToString()[0] : Rank.ToString()[0], 
                char.ToLower(Suit.ToString()[0])
                );
    }
}
