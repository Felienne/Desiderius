using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Sodes.Bridge.Base
{
    public abstract class Tournament
    {
        private Collection<Board2> _boards;
        private List<Participant> theParticipants;
        private string eventName;
        private DateTime created;
        private Scorings scoringMethod;
        private bool allowReplay;

        public Tournament(string name)
            : this()
        {
            this.eventName = name;
        }

        /// <summary>
        /// Only for the serializer
        /// </summary>
        public Tournament()
        {
            this.created = DateTime.Now;
            this._boards = new Collection<Board2>();
            this.theParticipants = new List<Participant>();
            this.scoringMethod = Scorings.scPairs;
            this.allowReplay = false;
            this.BidContest = false;
            this.OvercallsAllowed = true;
        }

        #region Methods

        public Board2 GetNextBoard(int boardNumber, Guid userId)
        {
            return this.GetNextBoardAsync(boardNumber, userId).Result;
        }

        public abstract Task<Board2> GetNextBoardAsync(int boardNumber, Guid userId);

        public abstract Task SaveAsync(BoardResult result);

        public Board2 ViewBoard(int boardNumber)
        {
            if (boardNumber < 1) throw new ArgumentOutOfRangeException("boardNumber", boardNumber + " (should be 1 or more)");
            foreach (var board in this._boards)
            {
                if (board.BoardNumber == boardNumber)
                {
                    return board;
                }
            }

            return null;
        }

        public void CalcTournamentScores()
        {
            foreach (var team in this.Participants)
            {
                team.InitRecalc();
            }

            foreach (var board in this._boards)
            {
                foreach (var result in board.Results)
                {
                    bool foundTeam = false;
                    foreach (var team in this.Participants)
                    {
                        if (team.IsSame(result.Participants.Names))
                        {
                            foundTeam = true;
                            team.AddScore(result.TournamentScore);
                            break;
                        }
                    }

                    if (!foundTeam)
                    {		// corruption in tournament file
                        Participant newParticipant = new Participant(result.Participants.Names);
                        newParticipant.AddScore(result.TournamentScore);
                        this.Participants.Add(newParticipant);
                    }
                }
            }

            foreach (var team in this.Participants)
            {
                team.CalcScore();
            }

            this.Participants.Sort(delegate(Participant p1, Participant p2)
            {
                return -p1.TournamentScore.CompareTo(p2.TournamentScore);
            });
        }

        public void AddResults(Tournament t2)
        {
            foreach (var board in this._boards)
            {

                foreach (var result in t2.ViewBoard(board.BoardNumber).Results)
                {
                    board.Results.Add(result);
                }
            }
        }

        #endregion

        #region Public Properties

        public string EventName
        {
            get { return eventName; }
            set { eventName = value; }
        }

        public DateTime Created
        {
            get { return created; }
            set { created = value; }
        }

        public Scorings ScoringMethod
        {
            get { return scoringMethod; }
            set { scoringMethod = value; }
        }

        public bool AllowReplay
        {
            get { return allowReplay; }
            set { allowReplay = value; }
        }

        public Collection<Board2> Boards
        {
            get { return _boards; }
            set { _boards = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<Participant> Participants
        {
            get { return theParticipants; }
            set { theParticipants = value; }
        }

        public string Trainer { get; set; }

        public string TrainerComment { get; set; }

        public string TrainerConventionCard { get; set; }

        public bool BidContest { get; set; }

        public bool OvercallsAllowed { get; set; }

        public bool Unattended { get; set; }

        #endregion
    }

    public class OnlineTournament
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Collection<OnlineTournamentResult> Ranking { get; set; }
        public Scorings Scoring { get; set; }
    }

    public class OnlineTournamentResult
    {
        public int Rank { get; set; }
        public Guid UserId { get; set; }
        public string Participant { get; set; }
        public string Country { get; set; }
        //public int Total { get; set; }
        public double Average { get; set; }
        public int Boards { get; set; }
    }

    public class RandomBoardsTournament : Tournament
    {
        public RandomBoardsTournament(string name) : base(name) { }

        public override async Task<Board2> GetNextBoardAsync(int boardNumber, Guid userId)
        {
            var c = new Board2(boardNumber);
            c.Distribution.DealRemainingCards(ShufflingRequirement.Random);
            return c;
        }

        public override async Task SaveAsync(BoardResult result)
        {
        }
    }
}
