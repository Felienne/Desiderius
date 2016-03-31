using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;

namespace Sodes.Bridge.Base
{
    [DataContract]
    public struct PlayRecord
    {
        [IgnoreDataMember]
        public Seats seat;

        [DataMember]
        public Suits Suit;

        [DataMember]
        public Ranks Rank;

        [IgnoreDataMember]
        public int man;

        [IgnoreDataMember]
        public int trick;

        [IgnoreDataMember]
        public string Comment;

        public override string ToString()
        {
            return SuitHelper.ToParser(this.Suit).ToLowerInvariant() + Sodes.Bridge.Base.Rank.ToXML(this.Rank);
        }
    }

    [DataContract]
    public class PlaySequence
    {
        [DataMember]
        internal Collection<PlayRecord> play
        {
            get;
            private set;
        }

        private short lastPlay;
        private Contract finalContract;
        private Seats declarer;
        private Seats declarersPartner;

        public PlaySequence(Contract bidResult, int tricksRemaining)
            : this()
        {
            finalContract = bidResult;
            declarer = finalContract.Declarer;
            declarersPartner = declarer.Partner();
            whoseTurn = declarer.Next();
            leadSuit = Suits.NoTrump;
            remainingTricks = (byte)tricksRemaining;
        }

        /// <summary>
        /// Special constructor for unit tests that want to evaluate somewhere in the middle of the game
        /// </summary>
        /// <param name="bidResult"></param>
        /// <param name="tricksRemaining"></param>
        /// <param name="_whoseTurn"></param>
        public PlaySequence(Contract bidResult, int tricksRemaining, Seats _whoseTurn)
            : this(bidResult, tricksRemaining)
        {
            this.whoseTurn = _whoseTurn;
        }

        /// <summary>
        /// Only for serializer
        /// </summary>
        public PlaySequence()
        {
            play = new Collection<PlayRecord>();
            lastPlay = -1;
            currentTrick = 1;
            man = 1;
        }

        public Contract Contract
        {
            get
            {
                return finalContract;
            }
        }

        [IgnoreDataMember]
        public byte man;

        [IgnoreDataMember]
        public byte currentTrick;

        [IgnoreDataMember]
        public Seats whoseTurn;
        [IgnoreDataMember]
        public Suits leadSuit;
        [IgnoreDataMember]
        public Seats bestMan;
        [IgnoreDataMember]
        public Suits bestSuit;
        [IgnoreDataMember]
        public Ranks bestRank;
        [IgnoreDataMember]
        public byte remainingTricks;

        public bool PlayEnded
        {
            get
            {
                return this.finalContract.Bid.IsPass || this.currentTrick > 13;
            }
        }

        public void Record(Seats s, Suits c, Ranks r, string comment)
        {
            lastPlay++;
            var p = (play.Count == lastPlay ? new PlayRecord() : play[lastPlay]);
            p.seat = s;
            p.Suit = c;
            p.Rank = r;
            p.man = man;
            p.trick = currentTrick;
            p.Comment = comment;
            if (play.Count == lastPlay) play.Add(p); else play[lastPlay] = p;

            if (man == 1)
            {
                leadSuit = c;
            }

            if ((man == 1) || (c == bestSuit && r > bestRank) || (c != bestSuit && c == finalContract.Bid.Suit))
            {
                bestSuit = c;
                bestRank = r;
                bestMan = s;
            }

            if (man == 4)
            {
                man = 1;
                currentTrick++;
                remainingTricks--;
                leadSuit = Suits.NoTrump;
                whoseTurn = bestMan;
#if strict
				if (this.finalContract.tricksForDeclarer + this.finalContract.tricksForDefense + this.remainingTricks > 12)
					throw new FatalBridgeException("PlaySequence.Record: tricks > 12");
#endif
                if (bestMan == declarer || bestMan == declarersPartner)
                {
                    this.finalContract.tricksForDeclarer++;
                }
                else
                {
                    this.finalContract.tricksForDefense++;
                }
            }
            else
            {
                man++;
                whoseTurn = whoseTurn.Next();
            }
        }

        public void Record(Seats s, Suits c, Ranks r)
        {
            this.Record(s, c, r, "");
        }

        public void Record(Seats s, Card c)
        {
            Record(s, c.Suit, c.Rank, "");
        }
        public void Record(Suits c, Ranks r)
        {
            Record(whoseTurn, c, r, "");
        }

        public void Record(Suits c, Ranks r, string comment)
        {
            Record(whoseTurn, c, r, comment);
        }

        public void Record(Card c)
        {
            Record(whoseTurn, c);
        }

        public Card CardWhenPlayed(int trick, Seats player)
        {
            for (int i = 0; i <= lastPlay; i++)
                if (play[i].seat == player && play[i].trick == trick)
                    return new Card(play[i].Suit, play[i].Rank);
            return null;
        }

        public Card CardPlayed(int trick, Seats player)
        {
            for (int i = 0; i <= lastPlay; i++)
                if (play[i].seat == player && play[i].trick == trick)
                    return new Card(play[i].Suit, play[i].Rank);
            throw new FatalBridgeException("CardPlayed: card not found");
        }
        public Card CardPlayed(int trick, int player)
        {
            for (int i = 0; i <= lastPlay; i++)
                if (play[i].man == player && play[i].trick == trick)
                    return new Card(play[i].Suit, play[i].Rank);
            throw new FatalBridgeException("CardPlayed: card not found");
        }
        public Seats Player(int trick, int player)
        {
            for (int i = 0; i <= lastPlay; i++)
                if (play[i].man == player && play[i].trick == trick)
                    return play[i].seat;
            throw new FatalBridgeException("Player: trick or man not found");
        }
        public int WhichMan(int trick, Seats player)
        {
            for (int i = 0; i <= lastPlay; i++)
                if (play[i].seat == player && play[i].trick == trick)
                    return play[i].man;
            throw new FatalBridgeException("Man: trick or player not found");
        }

        public PlaySequence Clone()
        {
            PlaySequence n = (PlaySequence)MemberwiseClone();
            n.play = new Collection<PlayRecord>();
            for (int i = 0; i <= lastPlay; i++)
            {
                n.play.Add(play[i]);
            }

            n.finalContract = this.finalContract.Clone();
            n.finalContract.tricksForDefense++;
            if (n.finalContract.tricksForDefense == this.finalContract.tricksForDefense)
                throw new FatalBridgeException("PlaySequence.Clone: finalContract");
            n.finalContract.tricksForDefense--;
            return n;
        }
        public bool DeclarersTurn { get { return whoseTurn == declarer || whoseTurn == declarersPartner; } }

        public bool IsLeader(Seats me)
        {
            return this.finalContract.IsLeader(me);
        }

        public Suits Trump { get { return this.finalContract.Bid.Suit; } }

        public int completedTricks { get { return (lastPlay < 3) ? 0 : play[lastPlay].trick; } }
        public void Undo()
        {
            if (man == 1)
            {
                man = 4;
                currentTrick--;
                remainingTricks++;
                if (bestMan == declarer || bestMan == declarersPartner)
                {
                    this.finalContract.tricksForDeclarer--;
                }
                else
                {
                    this.finalContract.tricksForDefense--;
                }

#if strict
				if (this.finalContract.tricksForDeclarer + this.finalContract.tricksForDefense + this.remainingTricks != 13)
					throw new FatalBridgeException("PlaySequence.Undo: tricks <> 13");
#endif
            }
            else
                man--;

            if (man == 1)
                leadSuit = Suits.NoTrump;
            else
            {
                // replay the trick to re-establish bestMan
                for (int m = 1; m < man; m++)
                {
                    int t = lastPlay - (man - m);
                    Suits s = play[t].Suit;
                    Ranks r = play[t].Rank;
                    if (man == 4 && m == 1) leadSuit = s;
                    if ((m == 1) || (s == bestSuit && r > bestRank) || (s != bestSuit && s == finalContract.Bid.Suit))
                    {
                        bestSuit = s;
                        bestRank = r;
                        bestMan = play[t].seat;
                    }
                }
            }

            whoseTurn = play[lastPlay].seat;
            //play.RemoveAt(lastPlay);
            lastPlay--;
        }

        public Collection<PlayRecord> AllCards
        {
            get
            {
                var l = new Collection<PlayRecord>();
                for (int c = 0; c <= lastPlay; c++)
                {
                    l.Add(this.play[c]);
                }

                return l;
            }
        }

        public int PlayedInTrick(Card card)
        {
            return this.PlayedInTrick(card.Suit, card.Rank);
        }

        public int PlayedInTrick(Suits s, Ranks r)
        {
            for (int i = 0; i <= lastPlay; i++)
                if (play[i].Suit == s && play[i].Rank == r)
                    return play[i].trick;
            return 14;
        }

        public bool TrickEnded
        {
            get
            {
                return this.man == 1;
            }
        }

        public int Length(Seats who, Suits suit)
        {
            int cardsPlayed = 0;
            for (int i = 0; i <= lastPlay; i++)
            {
                if (play[i].Suit == suit && play[i].seat == who)
                {
                    cardsPlayed++;
                }
            }

            return cardsPlayed;
        }

        public int LowerCardsCount(Suits suit, Ranks rank)
        {
            int result = 0;
            for (Ranks r = Ranks.Two; r < rank; r++)
            {
                if (this.PlayedInTrick(suit, r) == 14) result++;
            }
            return result;
        }

        public bool HasBeenRuffed(Suits suit)
        {
            for (int trick = 1; trick < this.currentTrick; trick++)
            {
                if (this.CardPlayed(trick, 1).Suit == suit)
                {		// the suit has been led in this trick
                    for (int manInTrick = 2; manInTrick <= 4; manInTrick++)
                    {
                        if (this.CardPlayed(trick, manInTrick).Suit == this.Trump)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public Seats Dummy { get { return this.finalContract.Declarer.Partner(); } }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i <= lastPlay; i++)
            {
                result.Append(play[i].ToString() + " ");
            }

            return result.ToString();
        }
    }

    /// Does not inherit from FatalBridgeException to prevent a Debugger.Break
    public class NoGoodCardFoundException : Exception { }
}
