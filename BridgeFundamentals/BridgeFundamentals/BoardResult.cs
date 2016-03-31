using System;
using System.Runtime.Serialization;
using System.Text;

namespace Sodes.Bridge.Base
{
    [DataContract]
    public class BoardResult : BoardResultRecorder
    {
        public BoardResult(string _owner, Board2 board, Participant newParticipant)
            : this(_owner, board, newParticipant.Names)
        {
        }

        public BoardResult(string _owner, Board2 board, SeatCollection<string> newParticipants) : base(_owner, board)
        {
            //if (board == null) throw new ArgumentNullException("board");
            this.Participants = new Participant(newParticipants);
        }

        [DataMember]
        private string[] theParticipants 
        {
            get
            {
                return new string[] { this.Participants.Names[Seats.North], this.Participants.Names[Seats.East], this.Participants.Names[Seats.South], this.Participants.Names[Seats.West] };
            }
            set
            {
                this.Participants = new Participant(value[0], value[1], value[2], value[3]);
            }
        }

        /// <summary>
        /// Only for deserialization
        /// </summary>
        private BoardResult()
        {
        }

        #region Public Properties

        [IgnoreDataMember]
        public Participant Participants { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        //[DataMember]
        public string TeamName
        {
            get
            {
                return this.Participants.Names[Seats.North] + "/" + this.Participants.Names[Seats.South]
                    //+ " - " + this.theParticipants[Seats.West] + "/" + this.theParticipants[Seats.East]
                    ;
            }
            //internal set		// required for DataContract
            //{
            //}
        }

        [DataMember]
        public int TournamentId { get; set; }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("Result for " + this.TeamName);
            result.Append(base.ToString());
            return result.ToString();
        }

        public override bool Equals(object obj)
        {
            var otherResult = obj as BoardResult;
            if (otherResult == null) return false;
            if (!base.Equals(otherResult)) return false;
            if (this.Participants.Names[Seats.South] != otherResult.Participants.Names[Seats.South]) return false;
            if (this.TeamName != otherResult.TeamName) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #region Bridge Event Handlers

        #endregion
    }
}
