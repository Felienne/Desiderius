using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;

namespace Sodes.Bridge.Base
{
    [DataContract]
    public class Board2
    {
        private int theBoardNumber;		//used for display (relative to tournament)
        private Seats theDealer;
        private Vulnerable theVulnerability;
        private Distribution theDistribution;
        private Collection<BoardResult> results;
        private BoardResult currentResult;
        private string theComment;
        private int theBoardId;			// used for storage (absolute to table)

        public Board2(Seats dealer, Vulnerable vulnerability, Distribution distribution)
            : this()
        {
            this.theDealer = dealer;
            this.theVulnerability = vulnerability;
            this.theDistribution = distribution;
        }

        public Board2(int boardNumber)
            : this()
        {
            this.theDealer = SeatsExtensions.DealerFromBoardNumber(boardNumber);
            this.theVulnerability = VulnerableConverter.FromBoardNumber(boardNumber);
            this.theBoardNumber = boardNumber;
            this.theDistribution.DealRemainingCards(ShufflingRequirement.Random);
        }

        public Board2()
        {
            this.theDealer = Seats.North;
            this.theVulnerability = Vulnerable.Both;
            this.theDistribution = new Distribution();
            this.results = new Collection<BoardResult>();
            this.theBoardNumber = 1;
        }

        /// <summary>
        /// Read a board from a string.
        /// The string must have the dealer and vulnerability on the first line
        /// E,Both
        ///        s 753
        ///        h KT53
        ///        d T654
        ///        c K6
        /// s T984        s AKQJ62
        /// h 842         h AQ7
        /// d AQ          d 3
        /// c AQJ7        c 543
        ///        s 
        ///        h J96
        ///        d K9872
        ///        c T982
        /// </summary>
        public Board2(string diagram)
        {
            if (diagram == null) throw new ArgumentNullException("diagram");
            this.theDistribution = new Distribution();
            this.results = new Collection<BoardResult>();

            string[] lines = diagram.Replace("\r", "").Replace("\t", "   ").Split('\n');
            string[] contract = lines[0].Split(',');

            ParseSuit(lines[01].Trim(), Seats.North);
            ParseSuit(lines[02].Trim(), Seats.North);
            ParseSuit(lines[03].Trim(), Seats.North);
            ParseSuit(lines[04].Trim(), Seats.North);
            ParseSuit(lines[09].Trim(), Seats.South);
            ParseSuit(lines[10].Trim(), Seats.South);
            ParseSuit(lines[11].Trim(), Seats.South);
            ParseSuit(lines[12].Trim(), Seats.South);
            ParseSuit(lines[05].Substring(00, 17).Trim(), Seats.West);
            ParseSuit(lines[06].Substring(00, 17).Trim(), Seats.West);
            ParseSuit(lines[07].Substring(00, 17).Trim(), Seats.West);
            ParseSuit(lines[08].Substring(00, 17).Trim(), Seats.West);
            ParseSuit(lines[05].Substring(17).Trim(), Seats.East);
            ParseSuit(lines[06].Substring(17).Trim(), Seats.East);
            ParseSuit(lines[07].Substring(17).Trim(), Seats.East);
            ParseSuit(lines[08].Substring(17).Trim(), Seats.East);

            this.theDealer = SeatsExtensions.FromXML(contract[0].Trim());
            this.theVulnerability = VulnerableConverter.FromXML(contract[1].Trim());
        }

        ~Board2()
        {
            this.theDistribution = null;
            this.results.Clear();
        }

        private void ParseSuit(string cards, Seats owner)
        {
            /// s AKQ63
            /// S A K Q 6 3
            Suits s = SuitHelper.FromXML(cards.Substring(0, 1));
            cards = cards.Substring(1);
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != ' ' && cards[i] != '-')
                {
                    Ranks r = Rank.From(cards[i]);
                    this.theDistribution.Give(owner, s, r);
                }
            }
        }

        #region Public Properties

        [DataMember]
        public int BoardNumber
        {
            get { return theBoardNumber; }
            set { theBoardNumber = value; }
        }

        [DataMember]
        public Seats Dealer
        {
            get { return theDealer; }
            set
            {
                this.theDealer = value;
                if (this.results != null)
                {
                    foreach (var result in this.results)
                    {
                        result.CopyBoardData(this.theVulnerability, this.theDistribution);
                    }
                }
            }
        }

        [DataMember]
        public Vulnerable Vulnerable
        {
            get { return theVulnerability; }
            set
            {
                theVulnerability = value;
                if (this.results != null)
                {
                    foreach (var result in this.results)
                    {
                        result.CopyBoardData(this.theVulnerability, this.theDistribution);
                    }
                }
            }
        }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string Comment
        {
            get
            {
                return theComment;
            }
            set
            {
                theComment = value;
            }
        }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string AfterAuctionComment { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string ClosingComment { get; set; }

        [DataMember]
        public Distribution Distribution
        {
            get { return theDistribution; }
            set
            {
                theDistribution = value;
                if (this.results != null)
                {
                    foreach (var result in this.results)
                    {
                        result.CopyBoardData(this.theVulnerability, this.theDistribution);
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]		// needed for deserialization
        [DataMember]
        public Collection<BoardResult> Results
        {
            get { return results; }
            set
            {
                this.results = value;
                if (this.results != null)
                {
                    foreach (var result in this.results)
                    {
                        result.Board = this;
                    }
                }
            }
        }

        [DataMember]
        public int BoardId
        {
            get { return this.theBoardId; }
            set { this.theBoardId = value; }
        }

        [DataMember]
        public int TournamentId { get; set; }
        
        public string BestContract { get; set; }

        public SeatCollection<SuitCollection<int>> FeasibleTricks { get; set; }

        #endregion

        #region Public Methods

        public BoardResult CurrentResult(Participant currentContender, bool allowReplay)
        {
            if (currentContender == null) throw new ArgumentNullException("currentContender");
            if (this.currentResult == null || (this.currentResult.Play != null && this.currentResult.Play.PlayEnded))
            {
                foreach (var result in this.results)
                {
                    if (currentContender.IsSame(result.Participants.Names))
                    {
                        if (!allowReplay)
                        {
                            throw new InvalidOperationException("Replay not allowed");
                        }

                        break;
                    }
                }

                //if (this.currentResult == null)
                {
                    this.currentResult = new BoardResult("Board", this, currentContender);
                    this.results.Add(this.currentResult);
                }
            }

            return this.currentResult;
        }

        public BoardResult CurrentResult(bool detached = false)
        {
            if (this.currentResult == null)
            {
                this.currentResult = new BoardResult("Board", this, new SeatCollection<string>());
                this.currentResult.Board = this;
                this.results.Add(this.currentResult);
            }

            return this.currentResult;
        }

        public void ClearCurrentResult()
        {
            this.currentResult = null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void CalcBoardScore()
        {
            //System.Collections.Generic..SortedList scores = new System.Collections.SortedList();
            //int resultCount = 0;
            //foreach (BoardResult result in this.results)
            //{
            //  if (	 (result.Play != null 
            //          && (result.Play.PlayEnded || result.Contract.Bid.IsPass || result.Contract.tricksForDeclarer + result.Contract.tricksForDefense == 13)
            //         ) 
            //      || (result.Auction.Count == 0 
            //          && result.Contract != null
            //         )
            //     )
            //  {
            //    resultCount++;
            //    int contractScore = result.NorthSouthScore;
            //    if (scores.Contains(contractScore))
            //    {
            //      scores.SetByIndex(scores.IndexOfKey(contractScore), (int)scores[contractScore] + 1);
            //    }
            //    else
            //    {
            //      scores.Add(contractScore, 1);
            //    }
            //  }
            //}

            //// voorlopig alleen paren score
            ////TODO: ook imps kunnen berekenen
            //if (resultCount == 0)
            //{
            //}
            //else
            //{
            //  if (resultCount == 1)
            //  {
            //    results[0].TournamentScore = 100;
            //  }
            //  else
            //  {
            //    int Top = 2 * resultCount - 2;
            //    int nextScore = Top;
            //    int CurrentResult = scores.Count;
            //    while (CurrentResult >= 1)
            //    {
            //      int equalScores = (int)scores.GetByIndex(CurrentResult - 1);
            //      scores.SetByIndex(CurrentResult - 1, 100.0 * (nextScore + 1 - equalScores) / (1.0 * Top));
            //      nextScore -= 2 * equalScores;
            //      CurrentResult--;
            //    }
            //    foreach (BoardResult result in results)
            //    {
            //      if (result.Contract != null && scores.ContainsKey(result.NorthSouthScore))
            //      {
            //        double x = (double)scores[result.NorthSouthScore];
            //        result.TournamentScore = (float)x;
            //      }
            //    }
            //  }

            //  this.results.Sort(delegate(BoardResult p1, BoardResult p2)
            //  {
            //    return -p1.TournamentScore.CompareTo(p2.TournamentScore);
            //  });
            //}
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("Board: " + this.BoardNumber + " Dealer: " + this.Dealer + " Vulnerable: " + this.Vulnerable);
            result.Append(this.theDistribution.ToString());
            foreach (BoardResult boardResult in this.results)
            {
                result.AppendLine(boardResult.ToString());
            }

            if (this.theComment != null)
            {
                result.AppendLine("Comment: " + this.theComment.Replace("\n", "\r\n"));
            }

            return result.ToString();
        }

        /// <summary>
        /// Indicates whether a board is being played that may not be interrupted
        /// </summary>
        public bool IsBeingPlayed()
        {
            if (this.currentResult != null)
            {
                if (this.currentResult.Play != null)
                {
                    if (this.currentResult.Play.PlayEnded)
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public void CorrectResults()
        {
            foreach (var result in this.results)
            {
                result.CopyBoardData(this.theVulnerability, this.theDistribution);
            }
        }

        /// <summary>
        /// To verify in tests whether serialization is complete
        /// </summary>
        public override bool Equals(object obj)
        {
            var board = obj as Board2;
            if (board == null) return false;
            if (this.AfterAuctionComment != board.AfterAuctionComment) return false;
            if (this.BoardId != board.BoardId) return false;
            if (this.BoardNumber != board.BoardNumber) return false;
            if (this.ClosingComment != board.ClosingComment) return false;
            if (this.Comment != board.Comment) return false;
            if (this.Dealer != board.Dealer) return false;
            if (this.TournamentId != board.TournamentId) return false;
            if (this.Vulnerable != board.Vulnerable) return false;
            if (!this.Distribution.Equals(board.Distribution)) return false;
            if (this.Results.Count != board.Results.Count) return false;
            for (int r = 0; r < this.Results.Count; r++)
            {
                if (!this.Results[r].Equals(board.Results[r])) return false;
            }
            return true;
        }

        /// <summary>
        /// Required when overriding Equals
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        private class InactiveBridgeEventBus : BridgeEventBus
        {
            public InactiveBridgeEventBus() : base() { }

            public override void Link(BridgeEventBusClient other) { }

            public override void Unlink(BridgeEventBusClient other) { }
        }
    }
}
