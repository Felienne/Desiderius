using Sodes.Base;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace Sodes.Bridge.Base
{
  [DataContract]
  public class BoardResultRecorder : BridgeEventHandlers
  {
    private double theTournamentScore;
    private Auction theAuction;
    private PlaySequence thePlay;
    private Distribution theDistribution;
    private Board2 parent;
    private int frequencyScore;
    private int frequencyCount;
    private Seats _dealer;
    private Vulnerable _vulnerability;

    public BoardResultRecorder(string _owner, Board2 board) : base()
    {
      //if (board == null) throw new ArgumentNullException("board");
      this.Board = board;
      this.Owner = _owner;
      if (board == null)
      {
        this.theDistribution = new Distribution();
      }
      else
      {
        this.theDistribution = board.Distribution.Clone();
      }
    }

    public BoardResultRecorder(string _owner) : this(_owner, null)
    {
    }

    /// <summary>
    /// Only for deserialization
    /// </summary>
    protected BoardResultRecorder()
    {
    }

    protected string Owner;

    #region Public Properties

    [IgnoreDataMember]
    public Board2 Board
    {
      get { return this.parent; }
      set
      {
        if (value != this.parent)
        {
          this.parent = value;
          this.Dealer = value.Dealer;
          this.Vulnerability = value.Vulnerable;
          this.BoardId = value.BoardId;
          if (this.theAuction == null)
          {
            this.theAuction = new Auction(value.Vulnerable, value.Dealer);
          }
          else
          {
            this.theAuction.BoardChanged(value);
          }
        }
      }
    }

    [IgnoreDataMember]
    public Contract Contract
    {
      get
      {
        if (this.Auction == null || (!this.Auction.Ended && this.Auction.Bids.Count > 0)) return null;
        return this.Auction.FinalContract;
      }
      set
      {
        if (this.Auction == null)
        {
          if (this.Board == null)
          {
            this.Auction = new Base.Auction(this.Vulnerability, this.Dealer);
          }
          else
          {
            this.Auction = new Base.Auction(this.Board.Vulnerable, this.Board.Dealer);
          }
        }
        this.Auction.FinalContract = value;
      }
    }

    [DataMember]
    public double TournamentScore
    {
      get
      {
        return theTournamentScore;
      }
      set
      {
        theTournamentScore = value;
      }
    }

    [DataMember]
    internal Seats Dealer
    {
      get { return this._dealer; }
      set
      {
        if (value != this._dealer)
        {
          this._dealer = value;
          if (this.theAuction != null)
          {
            this.theAuction.Dealer = value;
          }
        }
      }
    }

    [DataMember]
    internal Vulnerable Vulnerability
    {
      get { return this._vulnerability; }
      set
      {
        if (value != this._vulnerability)
        {
          this._vulnerability = value;
          if (this.theAuction != null)
          {
            this.theAuction.Vulnerability = value;
          }
        }
      }
    }

    [DataMember]
    public int BoardId { get; set; }

    [IgnoreDataMember]
    public Distribution Distribution { get { return this.theDistribution; } }

    [DataMember]
    public Auction Auction
    {
      get
      {
        return theAuction;
      }
      set
      {
        if (value == null) throw new ArgumentNullException("value");
        if (this.Board == null)
        {
          this.theAuction = new Auction(this.Vulnerability, this.Dealer);
        }
        else
        {
          this.theAuction = new Auction(this.Board.Vulnerable, this.Board.Dealer);
        }
        foreach (var bid in value.Bids)
        {
          this.theAuction.Record(bid);
        }
      }
    }

    [DataMember]
    public PlaySequence Play
    {
      get
      {
        return thePlay;
      }
      set
      {
        // Play can be null when Auction has not finished yet (happens when sending a bug report)
        //if (value == null) throw new ArgumentNullException("value");
        if (this.theAuction != null && this.theAuction.Ended)
        {
          this.thePlay = new Sodes.Bridge.Base.PlaySequence(this.theAuction.FinalContract, 13);
          this.thePlay.Contract.tricksForDeclarer = 0;
          this.thePlay.Contract.tricksForDefense = 0;
          foreach (var item in value.play)
          {
            this.thePlay.Record(item.Suit, item.Rank);
          }
          //if (this.theAuction != null && value.Contract == null)
          //{
          //  this.thePlay.Contract = this.theAuction.FinalContract;
          //}
        }
        else
        {
          this.thePlay = value;
        }
      }
    }

    [IgnoreDataMember]
    public bool IsFrequencyTable { get; set; }

    [IgnoreDataMember]
    public int NorthSouthScore
    {
      get
      {
        if (this.IsFrequencyTable)
        {
          return this.frequencyScore;
        }
        else
        {
          if (this.thePlay == null && !(!this.theAuction.Ended && this.Contract != null)) return -100000;
          return this.Contract.Score * (this.Contract.Declarer == Seats.North || this.Contract.Declarer == Seats.South ? 1 : -1);
        }
      }
      set
      {
        if (!this.IsFrequencyTable) throw new InvalidOperationException("Cannot set for a regular result");
        this.frequencyScore = value;
      }
    }

    [IgnoreDataMember]
    public int Multiplicity
    {
      get
      {
        if (!this.IsFrequencyTable) throw new InvalidOperationException("Cannot get for a regular result");
        return this.frequencyCount;
      }
      set
      {
        if (!this.IsFrequencyTable) throw new InvalidOperationException("Cannot set for a regular result");
        this.frequencyCount = value;
      }
    }

    #endregion

    #region Public Methods

    public void CopyBoardData(Vulnerable vulnerability, Distribution boardDistribution)
    {
      if (boardDistribution == null) throw new ArgumentNullException("boardDistribution");
      if (this.theAuction == null)
      {
        this.theAuction = new Auction(this.Board.Vulnerable, this.Board.Dealer);
      }
      else
      {
        if (this.theAuction.Ended)
        {
          this.theAuction.FinalContract.Vulnerability = vulnerability;
        }
      }

      this.theDistribution = boardDistribution.Clone();
    }

    public override string ToString()
    {
      StringBuilder result = new StringBuilder();
      result.Append(this.Auction.ToString());
      if (this.thePlay != null)
      {
        result.Append(this.thePlay.ToString());
      }

      return result.ToString();
    }

    public override bool Equals(object obj)
    {
      var otherResult = obj as BoardResultRecorder;
      if (otherResult == null) return false;
      if (this.Auction.AantalBiedingen != otherResult.Auction.AantalBiedingen) return false;
      if (this.Auction.Dealer != otherResult.Auction.Dealer) return false;
      if (this.Auction.Declarer != otherResult.Auction.Declarer) return false;
      if (this.Auction.WhoseTurn != otherResult.Auction.WhoseTurn) return false;
      if (this.Play.completedTricks != otherResult.Play.completedTricks) return false;
      if (this.Play.currentTrick != otherResult.Play.currentTrick) return false;
      if (this.TournamentScore != otherResult.TournamentScore) return false;
      if (this.Contract.Bid != otherResult.Contract.Bid) return false;
      if (this.Contract.Vulnerability != otherResult.Contract.Vulnerability) return false;
      // the following line is commented out because the property has [IgnoreDataMember] attribute
      // and serialized values do not match original on deserialization
      //if (this.NorthSouthScore != otherResult.NorthSouthScore) return false;
      return true;
    }

    /// <summary>
    /// Required since Equals has an override
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    #endregion

    #region Bridge Event Handlers

    public override void HandleBoardStarted(int boardNumber, Seats dealer, Vulnerable vulnerabilty)
    {
      if (this.Board == null)
      {
        this.Dealer = dealer;
        this.Vulnerability = vulnerabilty;
        this.theAuction = new Auction(this.Vulnerability, this.Dealer);
      }
    }

    public override void HandleCardPosition(Seats seat, Suits suit, Ranks rank)
    {
      //Log.Trace("BoardResultRecorder({3}).HandleCardPosition: {0} gets {2}{1}", seat, suit.ToXML(), rank.ToXML(), this.Owner);
      if (this.theDistribution.Incomplete)
      {   // this should only happen in a hosted tournament
          //Log.Trace("BoardResultRecorder.HandleCardPosition {0}", this.owner);
        this.theDistribution.Give(seat, suit, rank);
      }
    }

    public override void HandleBidDone(Seats source, Bid bid)
    {
      if (bid == null) throw new ArgumentNullException("bid");
      if (!bid.Hint)
      {
        this.theAuction.Record(bid.Clone());
        if (this.theAuction.Ended)
        {
          this.thePlay = new PlaySequence(this.theAuction.FinalContract, 13);
        }
      }
    }

    public override void HandleCardPlayed(Seats source, Suits suit, Ranks rank)
    {
      //Log.Trace("BoardResultRecorder({3}).HandleCardPlayed: {0} played {2}{1}", source, suit.ToXML(), rank.ToXML(), this.Owner);
      if (this.thePlay != null && this.theDistribution != null)
      {
        this.thePlay.Record(suit, rank);
        if (!this.theDistribution.Owns(source, suit, rank))
        {
          //  throw new FatalBridgeException(string.Format("{0} does not own {1}", source, card));
          /// 18-03-08: cannot check here: hosted tournaments get a card at the moment the card is played
          this.Distribution.Give(source, suit, rank);
        }

        this.theDistribution.Played(source, suit, rank);
      }
    }

    #endregion
  }
}
