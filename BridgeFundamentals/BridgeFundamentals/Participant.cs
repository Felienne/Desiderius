using System;

namespace Sodes.Bridge.Base
{
    public class Participant
    {
        private SeatCollection<string> theNames;
        private int lastBoardCompleted;
        private DateTime theLastPlay;
        private int scoreCount;
        private double sumOfScores;
        private double totalTournamentScore;

        public Participant()
        {
            this.Names = new SeatCollection<string>();
        }

        public Participant(string north, string east, string south, string west)
        {
            this.Names = new SeatCollection<string>(new string[] { north, east, south, west });
        }

        public Participant(SeatCollection<string> allNames)
        {
            this.Names = allNames;
        }

        public SeatCollection<string> Names
        {
            get
            {
                return theNames;
            }
            set
            {
                theNames = value;
            }
        }

        public int LastBoard
        {
            get
            {
                return lastBoardCompleted;
            }
            set
            {
                lastBoardCompleted = value;
            }
        }

        public DateTime LastPlay
        {
            get
            {
                return theLastPlay;
            }
            set
            {
                theLastPlay = value;
            }
        }

        public double TournamentScore
        {
            get
            {
                return totalTournamentScore;
            }
            set
            {
                totalTournamentScore = value;
            }
        }

        public override string ToString()
        {
            return this.theNames[Seats.North] + "/" + this.theNames[Seats.South]
                + " - " + this.theNames[Seats.West] + "/" + this.theNames[Seats.East];
        }

        public bool IsSame(SeatCollection<string> other)
        {
            for (Seats s = Seats.North; s <= Seats.West; s++)
            {
                if (this.theNames[s] != other[s])
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsSame(string[] other)
        {
            return this.IsSame(new SeatCollection<string>(other));
        }

        public void InitRecalc()
        {
            this.scoreCount = 0;
            this.sumOfScores = 0;
        }

        public void AddScore(double boardScore)
        {
            this.scoreCount++;
            this.sumOfScores += boardScore;
        }

        public void CalcScore()
        {
            this.totalTournamentScore = this.sumOfScores / this.scoreCount;
        }
    }
}
