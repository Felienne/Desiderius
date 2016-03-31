using System;
using System.Diagnostics;

namespace Sodes.Bridge.Base
{
    /// <summary>
    /// Summary description for Card.
    /// </summary>
    [DebuggerStepThrough]
    public class Card
    {
        private string _comment;
        private Suits suit;
        private Ranks rank;

        public Card(Suits suit, Ranks rank)
        {
            this.suit = suit;
            this.rank = rank;
        }

        public Card(Suits suit, Ranks rank, string comment) : this(suit, rank)
        {
            this._comment = comment;
        }

        public Card(string cardDescription) : this(SuitHelper.FromXML(cardDescription.Substring(0, 1)), Sodes.Bridge.Base.Rank.From(cardDescription.Substring(1, 1)))
        {
        }
        public Card()
        {
            this.suit = Suits.Clubs;
            this.rank = Ranks.Ace;
        }

        public Suits Suit { get { return this.suit; } /*set { this.suit = value; }*/ }
        public Ranks Rank { get { return this.rank; } }
        public static bool operator >(Card card1, Card card2)
        {
            return card1.suit == card2.suit && card1.rank > card2.rank;
        }
        public static bool operator <(Card card1, Card card2)
        {
            return card1.suit == card2.suit && card1.rank < card2.rank;
        }
        public static bool operator ==(Card card1, Card card2)
        {
            return (((Object)card1 == null && (Object)card2 == null) || ((Object)card1 != null && (Object)card2 != null && card1.suit == card2.suit && card1.rank == card2.rank));
        }
        public static bool operator ==(Card card1, string card2)
        {
            return card1 == new Card(card2);
        }
        public static bool operator !=(Card card1, Card card2)
        {
            return !(card1 == card2);
        }
        public static bool operator !=(Card card1, string card2)
        {
            return !(card1 == card2);
        }
        public override bool Equals(Object obj)
        {
            Card c = obj as Card;
            return null != c && this == c;
        }

        public static bool IsNull(object card)
        {
            return card == null;
        }

        public override int GetHashCode()
        {
            return 0;    //re.GetHashCode()  im.GetHashCode();
        }
        public static bool Wins(Card card1, Card card2, Suits trump)
        {
            return card1 > card2 || (card1.suit != card2.suit && card1.suit == trump);
        }

        public override string ToString()
        {
            return "" + SuitHelper.ToString(Suit).ToLowerInvariant() + Sodes.Bridge.Base.Rank.ToXML(Rank);
        }
        public byte HighCardPoints
        {
            get
            {
                return (byte)this.rank.HCP();
            }
        }
        public string Comment
        {
            get
            {
                return this._comment;
            }
        }
        public Card Clone()
        {
            return new Card(Suit, Rank);
        }
    }

    public class KaartSets
    {
        private string thisSet;
        public KaartSets(string setje)
        {
            thisSet = setje;
            if (setje.IndexOf("N") >= 0) throw new FatalBridgeException("N in KaartSets");
        }
        public bool Contains(VirtualRanks rank)
        {
            string s = Rank.ToXML((Ranks)rank);
            return thisSet.IndexOf(s) >= 0;
        }
        public bool Contains(string ranks)
        {
            bool result = true;
            for (int i = 0; i <= ranks.Length - 1; i++)
            {
                if (thisSet.IndexOf(ranks[i]) < 0) result = false;
            }
            return result;
        }
        public bool ContainsAnyOf(string ranks)
        {
            bool result = false;
            for (int i = 0; i <= ranks.Length - 1; i++)
            {
                if (thisSet.IndexOf(ranks[i]) >= 0) result = true;
            }
            return result;
        }
        public void Add(VirtualRanks rank)
        {
            thisSet += Rank.ToXML((Ranks)rank);
        }
        public bool IsEmpty()
        {
            return (thisSet.Length == 0);
        }

        public static KaartSets C(string set)
        {
            return new KaartSets(set);
        }
    }

    public class SimpleMove
    {
        public Suits Suit;
        public Ranks Rank;

        public SimpleMove() { Suit = (Suits)(-1); Rank = (Ranks)(-1); }
        public SimpleMove(Suits s, Ranks r) { Suit = s; Rank = r; }

        public override string ToString()
        {
            return "" + SuitHelper.ToString(Suit) + Sodes.Bridge.Base.Rank.ToXML(Rank);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }
            else
            {
                SimpleMove move = obj as SimpleMove;
                return (move != null && this.Suit == move.Suit && this.Rank == move.Rank);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
