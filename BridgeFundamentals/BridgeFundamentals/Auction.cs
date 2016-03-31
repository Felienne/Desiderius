namespace Sodes.Bridge.Base
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Collections.ObjectModel;

    /// <summary>
    /// A seat and a bid together form the base element of an auction.
    /// </summary>
    //[DataContract]
    //public struct AuctionItem
    //{
    //  //[DataMember]
    //  //public Seats seat;

    //  [DataMember]
    //  public Bid Bid;

    //  /// <summary>
    //  /// .
    //  /// </summary>
    //  /// <param name="s">The seat that made the bid</param>
    //  /// <param name="b">The bid that was made</param>
    //  public AuctionItem(Seats s, Bid b)
    //  {
    //    //this.seat = s; 
    //    this.Bid = b;
    //  }

    //  //internal AuctionItem() 
    //  //{
    //  //  this.seat = Seats.North;
    //  //  this.Bid = Bid.C("Pass");
    //  //}
    //}

    /// <summary>
    /// Auction maintains all bids that occur in a game and allows to query them.
    /// </summary>
    [DataContract]
    public class Auction
    {
        private int passCount;
        private bool doubled;
        private bool redoubled;
        private Bid lastBid = new Bid(SpecialBids.Pass);
        private Contract contract;
        private Vulnerable theVulnerability;
        private Seats firstSeatNotToPass;
        private bool allPassesTillNow;
        private Seats theDealer;
        private BoardResult parent;

        /// <summary>Constructor</summary>
        /// <param name="v">The vulnerability for this hand</param>
        public Auction(Vulnerable v, Seats dealer)
            : this()
        {
            this.theVulnerability = v;
            this.theDealer = dealer;
        }

        public Auction(BoardResult p)
            : this()
        {
            this.parent = p;
        }

        /// <summary>
        /// Just for serializer
        /// </summary>
        internal Auction()
        {
            this.passCount = 4;
            this.allPassesTillNow = true;
            this.Bids = new Collection<Bid>();
        }

        [DataMember]
        public Collection<Bid> Bids { get; private set; }

        /// <summary>Indicates whether the auction has finished (3 or 4 passes).</summary>
        /// <value>Returns true when the auction has ended</value>
        public bool Ended { get { return this.passCount == 0; } }

        /// <summary>Get the contract after the auction has ended</summary>
        /// <remarks>Not allowed to call during the auction</remarks>
        /// <value>?</value>
        public Contract FinalContract
        {
            get
            {
                if (!this.Ended && this.contract == null) throw new InvalidOperationException("The contract has not yet been determined");
                if (this.contract == null) this.contract = new Contract(this.lastBid, this.Doubled, this.Redoubled, this.Declarer, this.Vulnerability);
                return this.contract;
            }
            internal set
            {
                this.contract = value;
            }
        }

        public Seats WhoseTurn
        {
            get
            {
                if (this.Ended)
                {
                    return this.Declarer.Next();
                }
                else
                {
                    var who = this.Dealer;
                    var shifts = this.Bids.Count % 4;
                    for (int i = 0; i < shifts; i++) who = who.Next();
                    return who;
                }
            }
        }

        /// <summary>The seat that dealt this hand and thus will start the auction</summary>
        /// <value>?</value>
        //[IgnoreDataMember]
        public Seats Dealer
        {
            get { return this.parent == null || this.parent.Board == null ? this.theDealer : this.parent.Board.Dealer; }
            internal set { this.theDealer = value; }
        }

        /// <summary>The seat that bid the final contract</summary>
        /// <remarks>Allowed to call during the auction</remarks>
        /// <value>?</value>
        public Seats Declarer
        {
            get
            {
                int i = this.Bids.Count - 1;    // pointing to last bid
                while (i >= 0 && !this.Bids[i].IsRegular) i--;  // pointing to winning bid
                if (i < 0) return this.Dealer;		// 4 passes
                int j = i % 2;        // point to first bid of partnership
                while (!(this.Bids[j].IsRegular && this.Bids[i].Suit == this.Bids[j].Suit)) j += 2;  // pointing first bid in contract suit
                return this.WhoBid0(j);
            }
        }

        /// <summary>The first seat that did not pass</summary>
        /// <value>?</value>
        public Seats FirstNotToPass { get { return this.firstSeatNotToPass; } }

        /// <summary>The last bid that was not pass, double or redouble</summary>
        /// <value>?</value>
        public Bid LastRegularBid
        {
            get { return this.lastBid; }
        }

        /// <summary>Has the last bid been doubled?</summary>
        /// <value>?</value>
        public bool Doubled
        {
            get { return this.doubled; }
        }

        /// <summary>Has the last bid been redoubled</summary>
        /// <value>?</value>
        public bool Redoubled
        {
            get { return this.redoubled; }
        }

        //[DataMember]
        public Vulnerable Vulnerability
        {
            get { return this.parent == null || this.parent.Board == null ? this.theVulnerability : this.parent.Board.Vulnerable; }
            internal set { this.theVulnerability = value; }
        }

        /// <summary>Is it ok to double now?</summary>
        /// <value>?</value>
        public bool AllowDouble
        {
            get
            {
                if (this.doubled) return false;
                if (!this.lastBid.IsRegular) return false;
                var who = this.WhoBid0(this.Bids.Count - 1).Next();
                return this.Declarer != who && this.Declarer.Partner() != who;
            }
        }

        /// <summary>Is it ok to redouble now?</summary>
        /// <value>?</value>
        public bool AllowRedouble
        {
            get
            {
                if (!this.doubled) return false;
                if (this.redoubled) return false;
                var who = this.WhoBid0(this.Bids.Count - 1).Next();
                return this.Declarer.Partner() == who || this.Declarer == who;
            }
        }

        /// <summary>Get the number of bids in this auction</summary>
        /// <value>?</value>
        public byte AantalBiedingen { get { return (byte)this.Bids.Count; } }

        /// <summary>Get the 4th suit</summary>
        /// <value>?</value>
        public Suits WasVierdeKleur(int skip)
        {
            SuitCollection<bool> genoemd = new SuitCollection<bool>(false);
            Suits result = Suits.NoTrump;
            int nr = 2 + skip;
            while (nr <= this.AantalBiedingen)
            {
                if (this.Terug(nr).IsRegular)
                {
                    genoemd[this.Terug(nr).Suit] = true;
                }
                nr += 2;
            }
            nr = 0;
            for (Suits cl = Suits.Clubs; cl <= Suits.Spades; cl++)
            {
                if (!genoemd[cl])
                {
                    result = cl;
                    nr++;
                }
            }

            if (nr > 1) result = Suits.NoTrump;
            return result;
        }

        /// <summary>Get the 4th suit</summary>
        /// <value>?</value>
        public Suits VierdeKleur
        {
            get
            {
                return this.WasVierdeKleur(0);
            }
        }

        //    /// <summary>Indexer that returns an AuctionItem</summary>
        //    /// <param name="index">int index</param>
        //    /// <value>?</value>
        //    public new AuctionItem this[int index] 
        //    {
        //      get { return (AuctionItem)base[index]; }
        //      set { base[index] = value; }
        //    }

        /// <summary>Add an AuctionItem to the auction</summary>
        /// <param name="seat">The seat that made the bid</param>
        /// <param name="bid">The bid that has been made</param>
        public void Record(Seats seat, Bid bid)
        {
            if (bid == null) throw new ArgumentNullException("bid");
            if (bid.Index > 37) throw new AuctionException("Unknown bid: {0}", bid.Index);
            if (bid.IsDouble && !this.AllowDouble) throw new AuctionException("Double not allowed");
            if (bid.IsRedouble && !this.AllowRedouble) throw new AuctionException("Redouble not allowed");
            if (bid.IsRegular && this.lastBid >= bid) throw new AuctionException("Bid {0} is too low", bid);
            if (this.Ended) throw new AuctionException("Auction has already ended");
            if (seat != this.WhoseTurn) throw new AuctionException(string.Format("Expected a bid from {0} instead of {1}", this.WhoseTurn, seat));

            this.Bids.Add(bid);

            if (bid.IsPass)
            {
                this.passCount--;
                if (this.Ended)
                {
                }
            }
            else
            {
                this.passCount = 3;
                if (this.allPassesTillNow)
                {
                    this.allPassesTillNow = false;
                    this.firstSeatNotToPass = seat;
                }
            }

            if (bid.IsRegular)
            {
                this.doubled = false;
                this.redoubled = false;
                this.lastBid = bid;
            }
            else
            {
                if (bid.IsDouble)
                {
                    this.doubled = true;
                }
                else
                {
                    if (bid.IsRedouble) this.redoubled = true;
                }
            }
        }

        public void Record(Bid bid)
        {
            this.Record(this.WhoseTurn, bid);
        }

        /// <summary>Is the current auction comparable with the auction in the parameter?</summary>
        /// <param name="biedSerie">The auction to compare with</param>
        /// <returns>True when auctions are "equal"</returns>
        /// <remarks>See: Vergelijkbaarmuv</remarks>
        public bool Vergelijkbaar(string biedSerie)
        {
            return this.VergelijkbaarMUV(biedSerie, (byte)0);
        }

        /// <summary>Did the actual auction start with the given auction?</summary>
        /// <param name="biedSerie">The auction to compare with</param>
        /// <returns>True when auctions are "equal"</returns>
        /// <remarks>See: VergelijkbaarMUV</remarks>
        public bool StartedWith(string biedSerie)
        {
            return this.VergelijkbaarMUV(biedSerie, -1);
        }

        /// <summary>Verify whether the actual auction equals the given auction</summary>
        /// <param name="biedSerie">The auction to compare with</param>
        /// <param name="muv">The number of bids that may not be considered in the comparison</param>
        /// <returns>True when auctions are "equal"</returns>
        /// <remarks>
        ///    Deze functie parst een string en vergelijkt de keywords die erin
        ///    voorkomen met de auction.
        ///    Keywords  Betekenis
        ///    pas*      0 of meer keer passen
        ///    pas0      even aantal maal passen  (ook 0 maal)
        ///    pas1      oneven aantal maal passen
        ///    aX        op a-niveau in een kleur geboden, x is daarna een constante
        ///    aY        op a-niveau in een kleur geboden, y is daarna een constante
        ///    aZ        op a-niveau in een kleur geboden, z is daarna een constante
        ///    aM        op a-niveau in H of S geboden
        ///    aN        op a-niveau in K of R geboden
        ///    aa        bod aa gedaan, (aa between 00 and 37)
        ///    **				 ?
        /// </remarks>
        public bool VergelijkbaarMUV(string biedSerie, int muv)
        {
            //bool result = true;
            Suits w = Suits.NoTrump;
            Suits x = Suits.NoTrump;
            Suits y = Suits.NoTrump;
            Suits z = Suits.NoTrump;
            string keyWord = "";
            string specialKeywords = "WXYZMN";
            byte number = 0;
            byte bod = 1;

            while (biedSerie.Length > 0 && bod <= this.Bids.Count - muv)
            {
                NextKeyWord(ref biedSerie, ref keyWord, ref number);

                if (keyWord == "**") bod = (byte)(this.Bids.Count - muv + 1);    // klaar
                else if (keyWord == "??")
                {
                    if (bod <= this.Bids.Count) bod++;    // door naar het volgende bod
                    else return false;
                }
                else if (keyWord == "NP")
                {
                    if (bod - 1 > this.Bids.Count || this.Bids[bod - 1].IsPass) return false;
                    bod++;
                }
                else if (keyWord == "PASS*" || keyWord == "PAS*")
                {
                    while (bod <= this.Bids.Count && this.Bids[bod - 1].IsPass) bod++;
                }
                else if (keyWord == "PASS0" || keyWord == "PAS0")
                {
                    while ((bod + 1 <= this.Bids.Count)
                            && (this.Bids[bod - 1].IsPass)
                            && (this.Bids[bod].IsPass)
                            ) bod += 2;
                    if (bod <= this.Bids.Count && this.Bids[bod - 1].IsPass) return false;
                }
                else if (keyWord == "PASS1" || keyWord == "PAS1")
                {
                    if (bod <= this.Bids.Count && this.Bids[bod - 1].IsPass)
                    {
                        bod++;
                        while ((bod + 1 <= this.Bids.Count)
                                && (this.Bids[bod - 1].IsPass)
                                && (this.Bids[bod].IsPass)) bod += 2;
                        if (bod <= this.Bids.Count && this.Bids[bod - 1].IsPass) return false;
                    }
                    else return false;
                }
                else if (keyWord == "PASS2" || keyWord == "PAS2")
                {
                    if (bod + 1 <= this.Bids.Count && this.Bids[bod - 1 + 0].IsPass && this.Bids[bod - 1 + 1].IsPass)
                    {
                        bod += 2;
                        if (bod <= this.Bids.Count && this.Bids[bod - 1].IsPass) bod++;
                    }
                    else return false;
                }
                else if (keyWord == "PASS0OR1")
                {
                    if (bod <= this.Bids.Count && this.Bids[bod - 1].IsPass)
                    {
                        bod++;
                        if (bod <= this.Bids.Count && this.Bids[bod - 1].IsPass) return false;
                    }
                }
                else if ((keyWord.Length == 0) && (number >= 0) && (bod <= this.Bids.Count))
                {
                    if (this.Bids[bod - 1].Index != number) return false;
                    bod++;
                }
                else if (specialKeywords.IndexOf(keyWord) >= 0 && bod <= this.Bids.Count)
                {
                    if ((this.Bids[bod - 1].IsRegular)
                        && (this.Bids[bod - 1].Level == (BidLevels)number)
                        && (this.Bids[bod - 1].Suit != Suits.NoTrump))
                    {
                        switch (keyWord[0])
                        {
                            case 'W':
                                if (w == Suits.NoTrump)
                                {
                                    w = this.Bids[bod - 1].Suit;
                                    if (w == x || w == y || w == z) return false;
                                }
                                else if (w != this.Bids[bod - 1].Suit) return false;
                                break;

                            case 'X':
                                if (x == Suits.NoTrump)
                                {
                                    x = this.Bids[bod - 1].Suit;
                                    if (x == y || x == z || x == w) return false;
                                }
                                else if (x != this.Bids[bod - 1].Suit) return false;
                                break;

                            case 'Y':
                                if (y == Suits.NoTrump)
                                {
                                    y = this.Bids[bod - 1].Suit;
                                    if (y == x || y == z || y == w) return false;
                                }
                                else if (y != this.Bids[bod - 1].Suit) return false;
                                break;

                            case 'Z':
                                if (z == Suits.NoTrump)
                                {
                                    z = this.Bids[bod - 1].Suit;
                                    if (z == x || z == y || z == w) return false;
                                }
                                else if (z != this.Bids[bod - 1].Suit) return false;
                                break;

                            case 'M':
                                if (!(this.Bids[bod - 1].Suit == Suits.Hearts || this.Bids[bod - 1].Suit == Suits.Spades)) return false;
                                break;
                            case 'N':
                                if (!(this.Bids[bod - 1].Suit == Suits.Clubs || this.Bids[bod - 1].Suit == Suits.Diamonds)) return false;
                                break;
                        }
                        bod++;
                    }
                    else return false;
                }
                else if (bod <= this.Bids.Count)
                {
                    throw new FatalBridgeException("Syntax error in Vergelijkbaar: " + keyWord);
                }
                else return false;
            }

            if (muv == -1) return true;
            if (bod <= this.Bids.Count - muv) return false;    // vergelijkingen moeten uitputtend zijn Met Uitzondering Van de laatste x biedingen
            if (biedSerie.Length > 0)
            {
                NextKeyWord(ref biedSerie, ref keyWord, ref number);
                if ((biedSerie.Length > 0)
                  || ((keyWord != "PAS*") && (keyWord != "PAS0") && (keyWord != "**")))
                {
                    return false;  // vergelijkingen moeten uitputtend zijn
                }
            }
            return true;
        }

        /// <summary>Return the bid that occurred a specified number of bids ago</summary>
        /// <param name="bidMoment">The number of bids to skip</param>
        /// <returns>The bid that occurred a specified number of bids ago</returns>
        [DebuggerStepThrough]
        public Bid Terug(int bidMoment)
        {
#if DEBUG
            if (bidMoment > this.Bids.Count) throw new AuctionException("Before first bid");
            if (bidMoment < 1) throw new AuctionException("After last bid");
#endif
            return this.Bids[this.Bids.Count - bidMoment];
        }

        /// <summary>Find when a specified bid occurred in the auction</summary>
        /// <param name="bidToFind">The bid to find</param>
        /// <returns>The moment in the auction that the bid occurred</returns>
        /// <remarks>"The moment" is expressed in a number of bids (1 = the previous bid) that can be used with Terug</remarks>
        public int WanneerGeboden(Bid bidToFind)
        {
            int moment = this.Bids.Count;
            byte result = 0;
            while ((moment > 0)
                      && (this.Bids[moment - 1] != bidToFind))
            {
                moment--;
            }

            if (moment > 0) result = (byte)(this.Bids.Count + 1 - moment);   // wel gevonden
            return result;
        }

        public int WanneerGeboden(string bidToFind)
        {
            return WanneerGeboden(Bid.C(bidToFind));
        }

        /// <summary>Find when a specified bid occurred in the auction</summary>
        /// <param name="level">The level of the bid to find</param>
        /// <param name="suit">The suit of the bid to find</param>
        /// <returns>The moment in the auction that the bid occurred</returns>
        /// <remarks>"The moment" is expressed in a number of bids (1 = the previous bid) that can be used with Terug</remarks>
        public int WanneerGeboden(int level, Suits suit)
        {
            return this.WanneerGeboden(new Bid(level, suit));
        }

        /// <summary>
        /// Counting from last bid. 1 is last bid
        /// </summary>
        /// <param name="bidMoment"></param>
        /// <returns></returns>
        public Seats WhoBid(int bidMoment)
        {
#if DEBUG
            if (bidMoment > this.Bids.Count) throw new AuctionException("Before first bid");
            if (bidMoment < 1) throw new AuctionException("After last bid");
#endif
            var who = this.Dealer;
            var shifts = (this.Bids.Count - bidMoment) % 4;
            for (int i = 0; i < shifts; i++) who = who.Next();
            return who;
        }

        /// <summary>
        /// Counting from first bid. 0 is first bid
        /// </summary>
        /// <param name="bidMoment"></param>
        /// <returns></returns>
        public Seats WhoBid0(int bidMoment)
        {
#if DEBUG
            if (bidMoment >= this.Bids.Count) throw new AuctionException("After last bid");
            if (bidMoment < 0) throw new AuctionException("Before first bid");
#endif
            var who = this.Dealer;
            var shifts = (bidMoment) % 4;
            for (int i = 0; i < shifts; i++) who = who.Next();
            return who;
        }

        public bool HasBid(Suits suit, int bidMoment)
        {
            while (bidMoment <= this.Bids.Count)
            {
                Bid b = this.Bids[this.Bids.Count - bidMoment];
                if (b.IsRegular && b.Suit == suit) return true;
                bidMoment += 4;
            }

            return false;
        }

        /// <summary>Will the specified suit be "the 4th suit" when bid?</summary>
        /// <param name="nieuweKleur">The suit that will be bid</param>
        /// <returns>True when the specified suit has not been bid before</returns>
        public bool WordtVierdeKleur(Suits nieuweKleur)
        {
            SuitCollection<bool> genoemd = new SuitCollection<bool>(false);
            genoemd[nieuweKleur] = true;
            byte nr = 2;
            while (nr <= this.AantalBiedingen)
            {
                if (this.Terug(nr).IsRegular)
                {
                    Suits cl = this.Terug(nr).Suit;
                    if (cl != Suits.NoTrump) genoemd[cl] = true;
                }
                nr += 2;
            }
            bool result = true;
            for (Suits cl = Suits.Clubs; cl <= Suits.Spades; cl++)
                if (!genoemd[cl]) result = false;
            return result;
        }

        public bool Opened
        {
            get
            {
                for (int b = 0; b < this.Bids.Count; b++)
                {
                    if (this.Bids[b].IsRegular)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public Bid OpeningBid
        {
            get
            {
                for (int b = 0; b < this.Bids.Count; b++)
                {
                    if (this.Bids[b].IsRegular)
                    {
                        return this.Bids[b];
                    }
                }

                throw new InvalidOperationException("No opening bid yet");
            }
        }

        public Seats Opener
        {
            get
            {
                for (int b = 0; b < this.Bids.Count; b++)
                {
                    if (this.Bids[b].IsRegular)
                    {
                        return this.WhoBid0(b);
                    }
                }

                throw new InvalidOperationException("No opening bid yet");
            }
        }

        /// <summary>Get the next keyword in an auction compare string</summary>
        /// <param name="biedSerie">The auction</param>
        /// <param name="keyWord">The keyword that will be returned</param>
        /// <param name="number">?</param>
        private static void NextKeyWord(ref string biedSerie, ref string keyWord, ref byte number)
        {
            System.Text.StringBuilder b = new System.Text.StringBuilder(biedSerie.ToUpper());
            while (b.Length > 0 && Char.IsWhiteSpace(b[0]))
            {
                b.Remove(0, 1);
            }
            System.Text.StringBuilder k = new System.Text.StringBuilder("");
            while (b.Length > 0 && !Char.IsWhiteSpace(b[0]))
            {
                k.Append(b[0]);
                b.Remove(0, 1);
            }
            number = 0;
            while (k.Length > 0 && Char.IsDigit(k[0]))
            {
                number = (byte)((byte)(10 * number) + (byte)(Char.GetNumericValue(k[0])));
                k.Remove(0, 1);
            }
            biedSerie = b.ToString();
            keyWord = k.ToString();
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("West  North East  South");
            Seats skip = this.Dealer;
            while (skip != Seats.West)
            {
                result.Append("-     ");
                skip = skip.Previous();
            }

            var who = this.Dealer;
            foreach (var item in this.Bids)
            {
                result.Append(item.ToString().PadRight(6));
                if (who == Seats.South)
                {
                    result.AppendLine();
                }

                who = who.Next();
            }

            return result.ToString();
        }

        /// <summary>
        /// Who bid this suit first? Me or partner?
        /// </summary>
        /// <param name="trump"></param>
        /// <returns></returns>
        public Seats FirstBid(Suits trump)
        {
            Seats result = this.WhoseTurn;
            int back = 2;
            while (back <= this.Bids.Count)
            {
                if (this.Terug(back).IsRegular && this.Terug(back).Suit == trump) result = (back % 4 == 0 ? this.WhoseTurn : this.WhoseTurn.Partner());
                back += 2;
            }

            return result;
        }

        /// <summary>
        /// Who bid this suit first?
        /// </summary>
        /// <param name="trump"></param>
        /// <returns></returns>
        public Seats FirstToBid(Suits trump)
        {
            Seats result = this.WhoseTurn;
            int back = 1;
            while (back <= this.Bids.Count)
            {
                if (this.Terug(back).IsRegular && this.Terug(back).Suit == trump)
                {
                    return this.WhoBid(back);
                }

                back++;
            }

            return result;
        }

        internal void BoardChanged(Board2 p)
        {
            this.theDealer = p.Dealer;
            this.theVulnerability = p.Vulnerable;
            this.contract = null;
        }

        internal void BoardChanged(BoardResult p)
        {
            this.parent = p;
            if (this.contract != null) this.contract.Vulnerability = p.Board.Vulnerable;
        }

        /// <summary>
        /// Only used after a deserialize
        /// </summary>
        /// <param name="v"></param>
        /// <param name="dealer"></param>
        //public void SetAuction(Vulnerable v, Seats dealer)
        //{
        //  this.theVulnerability = v;
        //  this.Dealer = dealer;
        //}
    }

    /// <summary>
    /// Dedicated exception class for Auction.
    /// </summary>
    public class AuctionException : FatalBridgeException
    {
        /// <summary>Constructor</summary>
        public AuctionException(string format, params object[] args)
            : base(format, args)
        {
        }
    }
}
