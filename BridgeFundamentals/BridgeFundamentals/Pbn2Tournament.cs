using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sodes.Bridge.Base
{
    public static class Pbn2Tournament
    {
        #region Save

        public static void Save(Tournament t, Stream s)
        {
            using (var w = new StreamWriter(s))
            {
                w.WriteLine("% PBN 2.1");
                w.WriteLine("% EXPORT");
                w.WriteLine("% Creator: RoboBridge");
                w.WriteLine("");
                w.WriteLine("[Event \"{0}\"]", t.EventName);
                if (t.Created.Year > 1700) w.WriteLine("[Date \"{0}\"]", t.Created.ToString("yyyy.MM.dd"));
                w.WriteLine("[Scoring \"{0}\"]", t.ScoringMethod == Scorings.scIMP ? "IMP" : "MP");

                foreach (var board in t.Boards)
                {
                    int resultCount = board.Results.Count;
                    if (resultCount == 0) resultCount = 1;
                    for (int result = 0; result < resultCount; result++)
                    {
                        if (result == 0 || (result < board.Results.Count && board.Results[result].Auction.Ended))
                        {
                            w.WriteLine("");
                            w.WriteLine("[Board \"{0}\"]", board.BoardNumber);
                            w.WriteLine("[Dealer \"{0}\"]", board.Dealer.ToXML());
                            w.WriteLine("[Vulnerable \"{0}\"]", board.Vulnerable.ToPbn());
                            w.WriteLine("[Deal \"{0}\"]", board.Distribution.ToPbn());
                            if (board.BestContract != null) w.WriteLine("{{PAR of the deal: {0}}}", board.BestContract);
                            if (board.FeasibleTricks != null)
                            {
                                w.Write("{Feasability:");
                                SeatsExtensions.ForEachSeat(seat => SuitHelper.ForEachTrump(suit => w.Write(" " + board.FeasibleTricks[seat][suit])));
                                w.WriteLine(" }");
                            }
                        }

                        if (result < board.Results.Count && board.Results.Count > 0)
                        {
                            var boardResult = board.Results[result];
                            if (boardResult.Auction != null && boardResult.Auction.Ended)
                            {
                                for (Seats seat = Seats.North; seat <= Seats.West; seat++)
                                {
                                    if (boardResult.Participants.Names[seat].Length > 0)
                                    {
                                        w.WriteLine("[{0} \"{1}\"]", seat.ToXMLFull(), boardResult.Participants.Names[seat]);
                                    }
                                }

                                w.WriteLine("[Contract \"{0}\"]", boardResult.Contract.ToXML());
                                if (!t.BidContest)
                                {
                                    w.WriteLine("[Score \"{0}\"]", (boardResult.Contract.Declarer.IsSameDirection(Seats.North) ? 1 : -1) * boardResult.NorthSouthScore);
                                    //TODO: bug in below line: dutch regional settings puts a comma in 57,7% instead of 57.5%
                                    w.WriteLine("[{1} \"{0:F2}\"]", (boardResult.Contract.Declarer.IsSameDirection(Seats.North) ? 1 : -1) * boardResult.TournamentScore, (t.ScoringMethod == Scorings.scIMP ? "ScoreIMP" : "ScorePercentage"));
                                }
                                w.WriteLine("[Auction \"{0}\"]", board.Dealer.ToXML());
                                int bidCount = 0;
                                foreach (var bid in boardResult.Auction.Bids)
                                {
                                    bidCount++;
                                    if (bidCount > 1 && bidCount % 4 == 1)
                                    {
                                        w.WriteLine();
                                    }
                                    w.Write(bid.ToXML());
                                    if (bidCount % 4 > 0)
                                    {
                                        w.Write(" ");
                                    }
                                }

                                w.WriteLine();  // end of auction
                                if (boardResult.Contract.Bid.IsRegular && boardResult.Play != null && !t.BidContest)
                                {
                                    w.WriteLine("[Declarer \"{0}\"]", boardResult.Contract.Declarer.ToXML());
                                    w.WriteLine("[Result \"{0}\"]", boardResult.Contract.tricksForDeclarer);
                                    w.WriteLine("[Play \"{0}\"]", boardResult.Contract.Declarer.Next().ToXML());
                                    for (int trick = 1; trick <= boardResult.Play.completedTricks; trick++)
                                    {
                                        var who = boardResult.Contract.Declarer;
                                        for (int man = 1; man <= 4; man++)
                                        {
                                            who = who.Next();
                                            bool played = false;
                                            Card c = boardResult.Play.CardWhenPlayed(trick, who);
                                            if (!Card.IsNull(c))
                                            {
                                                played = true;
                                            }

                                            w.Write(played ? c.ToString() : "- ");
                                            if (man == 4)
                                            {
                                                w.WriteLine();
                                            }
                                            else
                                            {
                                                w.Write(" ");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                w.WriteLine("");
            }
        }

        private static string ToPbn(this Distribution d)
        {
            // N:AK.AQT53.T82.A93 QT3.92.9643.KQT4 98.K76.AKJ7.J872 J76542.J84.Q5.65
            string result = "N:";
            for (Seats seat = Seats.North; seat <= Seats.West; seat++)
            {
                for (Suits suit = Suits.Spades; suit >= Suits.Clubs; suit--)
                {
                    for (Ranks rank = Ranks.Ace; rank >= Ranks.Two; rank--)
                    {
                        if (d.Owns(seat, suit, rank))
                        {
                            result += rank.ToXML();
                        }
                    }

                    if (suit > Suits.Clubs) result += ".";
                }

                if (seat < Seats.West) result += " ";
            }

            return result;
        }

        private static string ToPbn(this Vulnerable v)
        {
            switch (v)
            {
                case Vulnerable.Neither:
                    return "None";
                case Vulnerable.NS:
                    return "NS";
                case Vulnerable.EW:
                    return "EW";
                case Vulnerable.Both:
                    return "All";
                default: throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Load

        public static Tournament Load(string content)
        {
            Tournament tournament = new PbnTournament();
            tournament.Trainer = "";
            Board2 currentBoard = null;
            Seats declarer = (Seats)(-1);
            int tricksForDeclarer = 0;
            int round = 0;
            var nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            Regex macro = new Regex("^(%|;).*$");
            Regex endOfBoard = new Regex("^(\\*.*|)$");
            Regex emptyLine = new Regex("^\\s*$");
            Regex commentOneLine = new Regex("{(?<comment>.*)}\\s*");
            Regex commentMultipleLines = new Regex("^{(?<comment>.*)$");
            Regex data = new Regex("^\\[(?<item>.*)\\s\"(?<value>.*)\"[ ]*\\]$");

            var lines = content.Split('\n');
            var lineCount = lines.Length;
            var lineNumber = 0;
            string line;
            string pbnCreator = "";      // what software produced this pbn?
            string clubBridgeWebs = "";  // which bridge club

            // Read and display lines from the file until the end of 
            // the file is reached.
            while (lineNumber < lineCount)
            {
                line = lines[lineNumber++].Trim();
                bool moreToParse;
                do
                {
                    moreToParse = false;

                    Match ma = macro.Match(line);
                    if (ma.Success)
                    {
                        int p = line.ToUpper().IndexOf("ROBOBRIDGE");
                        if (p > 0 && p < 10)
                        {		// specific RoboBridge instructions
                            line = line.Substring(p + 10).Trim();
                            if (line.ToUpper().StartsWith("TRAINING"))
                            {		// this is a RoboBridge training file
                                tournament.Trainer = "?";
                                tournament.TrainerConventionCard = "";
                                if (line.Length > 8)
                                {
                                    line = line.Substring(8).Trim();
                                    p = line.IndexOf(' ');
                                    tournament.Trainer = line.Substring(0, p);
                                    tournament.TrainerComment = line.Substring(p).Trim();
                                }
                                line = lines[lineNumber++].Trim();
                                moreToParse = true;
                                while (macro.Match(line).Success && line.Contains(tournament.Trainer))
                                {
                                    p = line.IndexOf(tournament.Trainer) + tournament.Trainer.Length;
                                    tournament.TrainerComment += " " + line.Substring(p).Trim();
                                    line = lines[lineNumber++].Trim();
                                }
                            }
                            else if (line.ToUpper().StartsWith("CC "))
                            {		// convention card to use
                                tournament.TrainerConventionCard = "Acol";
                                if (line.Length > 3)
                                {
                                    line = line.Substring(3).Trim();
                                    tournament.TrainerConventionCard = line;
                                }
                                line = (lineNumber < lineCount ? lines[lineNumber++].Trim() : null);
                                moreToParse = true;
                                while (macro.Match(line).Success && line.Contains(tournament.Trainer))
                                {
                                    p = line.IndexOf(tournament.Trainer) + tournament.Trainer.Length;
                                    tournament.TrainerComment += " " + line.Substring(p).Trim();
                                    line = (lineNumber < lineCount ? lines[lineNumber++].Trim() : null);
                                }
                            }
                            else if (line.ToUpper().StartsWith("BIDCONTEST"))
                            {		// convention card to use
                                tournament.BidContest = true;
                            }
                            else if (line.ToUpper().StartsWith("NOOVERCALLS"))
                            {		// convention card to use
                                tournament.OvercallsAllowed = false;
                            }
                            else if (line.ToUpper().StartsWith("UNATTENDED"))
                            {		// convention card to use
                                tournament.Unattended = true;
                            }
                            else
                            {
                                throw new PbnException("Unknown RoboBridge instruction: {0}", line);
                            }
                        }
                        else if (line.ToUpper().Contains("CREATOR"))
                        {
                            pbnCreator = line.Substring(10).Trim();
                        }

                        continue;
                    }

                    ma = endOfBoard.Match(line);
                    if (ma.Success) continue;

                    ma = emptyLine.Match(line);
                    if (ma.Success) continue;

                    ma = commentOneLine.Match(line);
                    if (ma.Success)
                    {
                        var comment = ma.Captures[0].Value;
                        line = line.Replace(comment, "");
                        moreToParse = (line.Trim().Length > 0);
                        if (currentBoard != null)
                        {
                            // {PAR of the deal: 4H  = played by North: 620 points}
                            // {Feasability: 8 7 10 10 8 5 6 3 3 3 8 7 10 10 8 5 6 3 3 3 }

                            if (comment.StartsWith("{PAR of the deal: "))
                            {
                                var t = comment.Substring(18, comment.Length - 19);     // 4H  = played by North: 620 points}
                                var contractEnd = t.IndexOf('='); if (contractEnd < 0) contractEnd = t.IndexOf('-');
                                var contract = t.Substring(0, contractEnd).Trim();
                                var declarerStart = t.IndexOf(" by ") + 4;
                                var declarerEnd = t.IndexOf(": ");
                                var d = t.Substring(declarerStart, declarerEnd - declarerStart - 1);
                                currentBoard.BestContract = contract + " by " + d;
                            }
                            else if (comment.StartsWith("{Feasability: "))
                            {
                                var tricks = comment.Substring(14, comment.Length - 16).Split(' ');
                                currentBoard.FeasibleTricks = new SeatCollection<SuitCollection<int>>();
                                SeatsExtensions.ForEachSeat(seat =>
                                    {
                                        currentBoard.FeasibleTricks[seat] = new SuitCollection<int>();
                                        SuitHelper.ForEachTrump(suit => currentBoard.FeasibleTricks[seat][suit] = int.Parse(tricks[5 * (int)seat + (int)suit]));
                                    });
                            }
                        }

                        continue;
                    }

                    ma = commentMultipleLines.Match(line);
                    if (ma.Success)
                    {
                        do
                        {
                            line = (lineNumber < lineCount ? lines[lineNumber++].Trim() : null);
                        } while (!Regex.IsMatch(line, "}$"));
                        continue;
                    }

                    ma = data.Match(line);
                    if (ma.Success)
                    {
                        string itemName = ma.Groups["item"].Value.TrimEnd().ToLowerInvariant();
                        string itemValue = ma.Groups["value"].Value.TrimEnd();
                        if (itemValue.Length > 0 || itemName == "board")
                        {
                            switch (itemName)
                            {
                                case "event":
                                    if (tournament.EventName == null) // && itemValue != "#")   will not handle multiple events in one file
                                    {
                                        tournament.EventName = itemValue;
                                    }
                                    break;

                                case "site":
                                    if (pbnCreator == "Bridgewebs") clubBridgeWebs = itemValue;
                                    break;

                                case "date":
                                    if (itemValue != "#")
                                    {
                                        string[] dateParts = itemValue.Split('.');
                                        if (dateParts.Length > 1)
                                        {		// dots were used as seperator
                                            if (dateParts.Length >= 3)
                                            {
                                                try
                                                {
                                                    tournament.Created = new DateTime(int.Parse(dateParts[0]), int.Parse(dateParts[1]), int.Parse(dateParts[2]));
                                                }
                                                catch (ArgumentOutOfRangeException)
                                                {
                                                }
                                                catch (FormatException)
                                                {
                                                }
                                            }
                                        }
                                        else
                                        {
                                            DateTime eventDate;
                                            DateTime.TryParse(itemValue, out eventDate);
                                            tournament.Created = eventDate;
                                        }
                                    }
                                    break;

                                case "board":
                                    int boardNumber = 0;
                                    if (string.IsNullOrWhiteSpace(itemValue))
                                    {
                                        boardNumber = tournament.Boards.Count + 1;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            boardNumber = 100 * round + int.Parse(itemValue);
                                        }
                                        catch (FormatException)
                                        {
                                            throw new PbnException("Illegal board number in [board]: {0}", itemValue);
                                        }
                                    }

                                    if (currentBoard != null && currentBoard.Distribution.Deal == null) throw new PbnException("Specify [board] first", itemValue);

                                    currentBoard = tournament.GetNextBoard(boardNumber, Guid.Empty);
                                    if (!string.IsNullOrEmpty(tournament.Trainer)) currentBoard.ClosingComment = ".";
                                    tricksForDeclarer = -1;
                                    declarer = (Seats)(-1);
                                    break;

                                case "north":
                                case "east":
                                case "south":
                                case "west":
                                    var seat2 = SeatsExtensions.FromXML(itemName);
                                    if (currentBoard == null) currentBoard = tournament.GetNextBoard(tournament.Boards.Count + 1, Guid.Empty);
                                    if (!(currentBoard.Results.Count == 0 && (itemValue == "#" || itemValue == "?")))
                                    {
                                        currentBoard.CurrentResult(true).Participants.Names[seat2] = itemValue;
                                    }
                                    break;

                                case "dealer":
                                    if (currentBoard == null)
                                    {
                                        currentBoard = tournament.GetNextBoard(tournament.Boards.Count + 1, Guid.Empty);
                                        tricksForDeclarer = -1;
                                    }
                                    try
                                    {
                                        currentBoard.Dealer = SeatsExtensions.FromXML(itemValue);
                                    }
                                    catch (FatalBridgeException)
                                    {
                                        throw new PbnException("Board {0}: Unknown [dealer]: {1}", currentBoard.BoardNumber, itemValue);
                                    }
                                    break;

                                case "vulnerable":
                                    try
                                    {
                                        currentBoard.Vulnerable = VulnerableConverter.FromXML(itemValue);
                                    }
                                    catch (FatalBridgeException)
                                    {
                                        throw new PbnException("Board {0}: Unknown [vulnerable]: {1}", currentBoard.BoardNumber, itemValue);
                                    }
                                    break;

                                case "deal":
                                    if (currentBoard == null) currentBoard = tournament.GetNextBoard(tournament.Boards.Count + 1, Guid.Empty);
                                    if (currentBoard.Distribution.Incomplete)
                                    {
                                        currentBoard.Distribution.InitCardDealing();
                                        // [Deal "N:T9643.AT.JT954.K J2.863.AQ7.A9854 AQ75.Q9754.2.QT6 K8.KJ2.K863.J732"]
                                        string players = "NESW";
                                        string suit = "SHDC";
                                        switch (itemValue.Substring(0, 1))
                                        {
                                            case "N":
                                                players = "NESW";
                                                break;
                                            case "E":
                                                players = "ESWN";
                                                break;
                                            case "S":
                                                players = "SWNE";
                                                break;
                                            case "W":
                                                players = "WNES";
                                                break;
                                            default: throw new PbnException("Board {0}: Unknown seat {1} in [deal]", currentBoard.BoardNumber, itemValue.Substring(0, 1));
                                        }

                                        string[] hands = itemValue.Substring(2).Split(' ');
                                        for (int p = 0; p < hands.Length; p++)
                                        {
                                            string[] suits = hands[p].Split('.');
                                            if (suits.Length != 4) throw new PbnException("Board {0}: error in [deal]: not 4 suits for {1}", currentBoard.BoardNumber, SeatsExtensions.FromXML(players.Substring(p, 1)));
                                            for (int s = 0; s < 4; s++)
                                            {
                                                for (int c = 0; c < suits[s].Length; c++)
                                                {
                                                    try
                                                    {
                                                        currentBoard.Distribution.Give(SeatsExtensions.FromXML(players.Substring(p, 1)), SuitHelper.FromXML(suit.Substring(s, 1)), Rank.From(suits[s].Substring(c, 1)));
                                                    }
                                                    catch (FatalBridgeException x)
                                                    {
                                                        throw new PbnException("Board {0}: error in [deal]: {1}", currentBoard.BoardNumber, x.Message);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;

                                case "declarer":
                                    if (itemValue != "?")
                                    {
                                        try
                                        {
                                            declarer = SeatsExtensions.FromXML(itemValue);
                                        }
                                        catch (FatalBridgeException)
                                        {
                                            throw new PbnException("Board {0}: Unknown [declarer]: {1}", currentBoard.BoardNumber, itemValue);
                                        }
                                    }
                                    break;
                                case "contract":
                                    if (itemValue != "?")
                                    {
                                        try
                                        {
                                            currentBoard.CurrentResult(true).Contract = new Contract(itemValue, declarer, currentBoard.Vulnerable);
                                        }
                                        catch (FatalBridgeException)
                                        {
                                            throw new PbnException("Board {0}: Illegal [contract]: {1}", currentBoard.BoardNumber, itemValue);
                                        }

                                        if (tricksForDeclarer >= 0) currentBoard.CurrentResult(true).Contract.tricksForDeclarer = tricksForDeclarer;
                                    }
                                    break;
                                case "result":
                                    if (itemValue.Length > 0)
                                    {
                                        if (itemValue != "?" && itemValue != "##")
                                        {
                                            try
                                            {
                                                tricksForDeclarer = int.Parse(itemValue);
                                            }
                                            catch (FormatException)
                                            {
                                                throw new PbnException("Illegal number in [result]: {0}", itemValue);
                                            }
                                            if (currentBoard.CurrentResult(true).Auction != null && currentBoard.CurrentResult(true).Auction.Ended)
                                            {
                                                currentBoard.CurrentResult(true).Contract.tricksForDeclarer = tricksForDeclarer;
                                                currentBoard.CurrentResult(true).Contract.tricksForDefense = 13 - tricksForDeclarer;
                                                currentBoard.CalcBoardScore();
                                            }
                                        }
                                    }
                                    break;
                                case "annotator":
                                    break;
                                case "round":
                                    if (itemValue != "#")
                                    {
                                        int.TryParse(itemValue, out round);
                                    }
                                    break;
                                case "score":
                                    // ignore score; it will be calcultaed from contract and result
                                    break;
                                case "scoreimp":
                                    if (itemValue.StartsWith("NS")) itemValue = itemValue.Substring(2);
                                    currentBoard.CurrentResult(true).TournamentScore = double.Parse(itemValue, nfi);
                                    break;
                                case "scoretable":
                                    /*
[ScoreTable "PairId_NS\2R;PairId_EW\2R;Contract\6L;Declarer\1R;Result\2R"]
 7  8 4H     N 11
 9 12 3NT    N 11
[ScoreTable "Rank\3R;Contract\5L;Declarer\1;Result\2R;Score_NS\5R;IMP_NS\6R;Multiplicity"]
  1 6C    W  9   150   9.91 1
  2 5CX   E 10   100   9.46 1
 14 6C    W 11    50   8.47 5
123 12345 1 12 12345 123456 
                                     */

                                    var columnDefinitions = itemValue.Split(';');
                                    if (clubBridgeWebs == "The Bridge Academy" && columnDefinitions[2] == "Contract\\5L") columnDefinitions[2] = "Contract\\6L";
                                    int rank = 0;
                                    do
                                    {
                                        line = (lineNumber < lineCount ? lines[lineNumber++] : "");
                                        if (line.Length >= 1 && line[line.Length - 1] == '\r') line = line.Substring(0, line.Length - 1);
                                        if (line.Length >= 1 && line[0] != '[')
                                        {
                                            ScoreTableEntry entry;// = new ScoreTableEntry();
                                            entry.Rank = ++rank;
                                            entry.Players = "";
                                            entry.Multiplicity = 1;
                                            entry.ScoreNS = 0;
                                            entry.ImpNS = 0;
                                            entry.Result = 0;
                                            entry.Declarer = Seats.North;
                                            entry.Contract = "";
                                            int p = 0;
                                            for (int c = 0; c < columnDefinitions.Length; c++)
                                            {
                                                var x = columnDefinitions[c].Split('\\');
                                                int fieldLength = 10;
                                                if (x.Length > 1) fieldLength = x[1] == "1" ? 1 : int.Parse(x[1].Substring(0, x[1].Length - 1));
                                                else
                                                {
                                                    int nextTab = line.IndexOf("\t", p);
                                                    if (nextTab >= 0) fieldLength = nextTab - p;
                                                }
                                                var value = (p + fieldLength > line.Length ? line.Substring(p) : line.Substring(p, fieldLength));
                                                p += fieldLength + 1;
                                                switch (x[0].ToLower())
                                                {
                                                    case "pairid_ns":
                                                        entry.Players = value + " - ";
                                                        break;
                                                    case "pairid_ew":
                                                        entry.Players += value;
                                                        break;
                                                    case "contract":
                                                        entry.Contract = value.Trim();
                                                        break;
                                                    case "declarer":
                                                        if (value != "-") entry.Declarer = SeatsExtensions.FromXML(value);
                                                        break;
                                                    case "result":
                                                        if (value.Trim() != "-") entry.Result = int.Parse(value);
                                                        break;
                                                    case "score_ns":
                                                        entry.ScoreNS = int.Parse(value);
                                                        break;
                                                    case "imp_ns":
                                                        entry.ImpNS = double.Parse(value, new CultureInfo("en-US"));
                                                        break;
                                                    case "multiplicity":
                                                        entry.Multiplicity = int.Parse(value);
                                                        rank += entry.Multiplicity - 1;
                                                        break;
                                                }
                                            }

                                            currentBoard.ClearCurrentResult();
                                            currentBoard.CurrentResult(true).IsFrequencyTable = true;
                                            currentBoard.CurrentResult(true).NorthSouthScore = entry.ScoreNS;
                                            currentBoard.CurrentResult(true).Multiplicity = entry.Multiplicity;
                                            if (entry.Contract.Length > 0)
                                            {
                                                currentBoard.CurrentResult(true).Contract = new Contract(entry.Contract, entry.Declarer, currentBoard.Vulnerable);
                                                currentBoard.CurrentResult(true).Contract.tricksForDeclarer = entry.Result;
                                            }
                                            currentBoard.CurrentResult(true).TournamentScore = entry.ImpNS;
                                            currentBoard.CurrentResult(true).Participants.Names[Seats.North] = entry.Players;
                                        }
                                    } while (line.Length >= 1 && line[0] != '[');
                                    break;

                                case "auction":
                                    //if (!ignoreBoardResult)
                                    {
                                        var auctionStarter = Seats.North;
                                        try
                                        {
                                            auctionStarter = SeatsExtensions.FromXML(itemValue);
                                        }
                                        catch (FatalBridgeException)
                                        {
                                            throw new PbnException("Board {0}: Unknown dealer {1} in [auction]", currentBoard.BoardNumber, itemValue);
                                        }
                                        if (auctionStarter != currentBoard.Dealer) throw new PbnException("Board {0}: [Auction] not started by dealer", currentBoard.BoardNumber);
                                        var currentResult = currentBoard.CurrentResult(true);
                                        currentResult.Auction = new Auction(currentResult);
                                        string auction = "";
                                        line = (lineNumber < lineCount ? lines[lineNumber++].Trim() : null);
                                        while (line != null && !line.Contains("[") && line.Trim() != "*")
                                        {
                                            auction += "\n" + line;
                                            line = (lineNumber < lineCount ? lines[lineNumber++].Trim() : null);
                                        }

                                        auction = auction.Trim();
                                        // 1H 1S { hg jhghg jg } pass double { jhgjhg jg jhgj gjg} pass pass pass { hghjg jh gj }
                                        string openingComment = "";
                                        auction = Regex.Replace(auction, "^{(?<comment>.+?)}", (match) =>
                                            {
                                                openingComment = SuitSymbols(match.Groups["comment"].Value.Trim().Replace("\\n", "\n"));
                                                return "";
                                            }, RegexOptions.Singleline).Trim();

                                        while (auction.Length > 0)
                                        {
                                            string bid = "";
                                            auction = Regex.Replace(auction, "^(?<bid>[a-z0-9=\\*\\!\\$\\-]+)($|{| |\n)", (match) =>
                                                {
                                                    bid = match.Groups["bid"].Value;
                                                    return "";
                                                }, RegexOptions.IgnoreCase).Trim();
                                            string comment = "";
                                            auction = Regex.Replace(auction, "^{(?<comment>.+?)}", (match) =>
                                            {
                                                comment = SuitSymbols(match.Groups["comment"].Value.Trim().Replace("\\n", "\n"));
                                                return "";
                                            }, RegexOptions.Singleline).Trim();

                                            if (bid.Length == 0 && comment.Length == 0) throw new PbnException("Board {0}: No bid or comment found after\n{1}", currentBoard.BoardNumber, currentResult.Auction);
                                            if (openingComment.Length > 0)
                                            {
                                                comment = (openingComment + " " + comment).Trim();
                                                openingComment = "";
                                            }

                                            if (bid.Length > 0)
                                            {
                                                if (bid == "*")
                                                {
                                                    currentResult.Auction.Record(new Bid(0));
                                                    currentResult.Auction.Record(new Bid(0));
                                                    currentResult.Auction.Record(new Bid(0));
                                                }
                                                else
                                                {
                                                    if (bid.StartsWith("$"))
                                                    {
                                                    }
                                                    else
                                                    {
                                                        if (bid.StartsWith("="))
                                                        {
                                                            // er gaat nog een note komen (een alert uitleg) waarvan het nummer correspondeert met dit nummer
                                                        }
                                                        else
                                                        {
                                                            Bid b;
                                                            try
                                                            {
                                                                b = new Bid(bid);
                                                            }
                                                            catch (IndexOutOfRangeException)
                                                            {
                                                                throw new PbnException("Board {0}: unrecognized bid in [auction]: {1}", currentBoard.BoardNumber, bid);
                                                            }
                                                            catch (FatalBridgeException)
                                                            {
                                                                throw new PbnException("Board {0}: unrecognized bid in [auction]: {1}", currentBoard.BoardNumber, bid);
                                                            }

                                                            if (comment.Length > 0) b.HumanExplanation = comment;
                                                            try
                                                            {
                                                                currentResult.Auction.Record(b);
                                                            }
                                                            catch (AuctionException x)
                                                            {
                                                                throw new PbnException("Board {0}: Error in [auction]: {1}", currentBoard.BoardNumber, x.Message);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (currentResult.Auction.Ended)
                                        {
                                            //if (currentResult.Contract != null && currentResult.Contract.Text != currentResult.Auction.FinalContract.Text)
                                            //{
                                            //  throw new PbnException("Board {2}: [contract] ({0}) does not match [auction] ({1})", currentResult.Contract.Text, currentResult.Auction.FinalContract.Text, currentBoard.BoardNumber);
                                            //}

                                            currentResult.Contract = currentResult.Auction.FinalContract;
                                            if (declarer == (Seats)(-1))
                                            {
                                                declarer = currentResult.Auction.Declarer;
                                            }
                                            else if (declarer != currentResult.Auction.Declarer)
                                            {
                                                throw new PbnException("Board {2}: [declarer] ({0}) does not match [auction] ({1})", declarer, currentResult.Auction.Declarer, currentBoard.BoardNumber);
                                            }
                                        }
                                        moreToParse = true;		// make sure the current line (which has been read ahead) is parsed
                                    }
                                    break;

                                case "note":    // explanation of alert within auction
                                    break;

                                case "play":
                                    if (itemValue != "?")
                                    {
                                        var currentResult = currentBoard.CurrentResult(true);
                                        if (itemValue != "-" && currentResult.Contract != null)
                                        {
                                            if (SeatsExtensions.FromXML(itemValue) != currentResult.Contract.Declarer.Next())
                                                throw new PbnException("Board {2}: Lead ({0}) does not match with the declarer ({1})", itemValue, currentResult.Contract.Declarer, currentBoard.BoardNumber);
                                        }

                                        string play = "";
                                        line = (lineNumber < lineCount ? lines[lineNumber++].Trim() : null);
                                        while (line != null && !line.Contains("[") && !line.StartsWith("%"))
                                        {
                                            play = (play + "\n" + line).Trim();
                                            line = (lineNumber < lineCount ? lines[lineNumber++].Trim() : null);
                                        }

                                        if (!play.StartsWith("*"))
                                        {
                                            currentResult.Play = new PlaySequence(currentResult.Auction.FinalContract, 13);

                                            Seats nextToPlay = (itemValue == "-" ? Seats.North : SeatsExtensions.FromXML(itemValue));
                                            Seats starter = nextToPlay;
                                            SeatCollection<string> trick = new SeatCollection<string>();
                                            while (play.Length > 0 && !play.StartsWith("*"))
                                            {
                                                string card = "";
                                                play = Regex.Replace(play, "^(?<card>([a-z0-9][a-z0-9])|(-))($|{| |\\*|\n)", (match) =>
                                                {
                                                    card = match.Groups["card"].Value;
                                                    return "";
                                                }, RegexOptions.IgnoreCase).Trim();
                                                string comment = "";
                                                play = Regex.Replace(play, "^{(?<comment>.+?)}", (match) =>
                                                {
                                                    comment = SuitSymbols(match.Groups["comment"].Value.Trim().Replace("\\n", "\n"));
                                                    return "";
                                                }, RegexOptions.Singleline).Trim();

                                                if (card.Length == 0 && comment.Length == 0) throw new PbnException("Board {0}: No card or comment found after\n{1}", currentBoard.BoardNumber, currentResult.Play);
                                                if (card.Length == 0 && comment.Length > 0)
                                                {
                                                    currentBoard.AfterAuctionComment = comment;
                                                }
                                                else
                                                {
                                                    if (itemValue == "-")
                                                    {
                                                        PlayCard(card + comment, currentResult.Play.whoseTurn, currentResult);
                                                    }
                                                    else
                                                    {
                                                        trick[nextToPlay] = card + comment;
                                                        nextToPlay = nextToPlay.Next();
                                                        if (nextToPlay == starter)
                                                        {		// have read 4 cards; now add them to the board
                                                            var s = currentResult.Play.whoseTurn;
                                                            for (int i = 0; i < 4; i++)
                                                            {
                                                                PlayCard(trick[s], s, currentResult);
                                                                trick[s] = "";
                                                                s = s.Next();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            var s2 = currentResult.Play.whoseTurn;
                                            for (int i = 0; i < 4; i++)
                                            {
                                                PlayCard(trick[s2], s2, currentResult);
                                                trick[s2] = "";
                                                s2 = s2.Next();
                                            }
                                        }

                                        // closing comment?
                                        if (play.Length > 3)
                                        {
                                            string x = @"\{(?<comment>.+?)\}";
                                            currentBoard.ClosingComment = SuitSymbols(Regex.Match(play, x, RegexOptions.Singleline).Groups["comment"].Value.Trim().Replace("\\n", "\n"));
                                        }

                                        if (line != null) moreToParse = true;
                                        if (tricksForDeclarer >= 0 && currentResult.Auction.Ended)
                                        {
                                            currentResult.Contract.tricksForDeclarer = tricksForDeclarer;
                                            currentResult.Contract.tricksForDefense = 13 - tricksForDeclarer;
                                            currentBoard.CalcBoardScore();
                                        }
                                    }
                                    break;

                                //[OptimumScore "NS -100"]
                                case "optimumscore":
                                    break;

                                //[OptimumResultTable "Declarer;Denomination\2R;Result\2R"]
                                //N  C  3
                                //N  D  7
                                //N  H  8
                                //N  S  3
                                //N NT  6
                                //E  C  8
                                //E  D  6
                                //E  H  4
                                //E  S  9
                                //E NT  7
                                //S  C  3
                                //S  D  7
                                //S  H  9
                                //S  S  3
                                //S NT  6
                                //W  C  8
                                //W  D  6
                                //W  H  4
                                //W  S  9
                                //W NT  7
                                case "optimumresulttable":
                                    for (int i = 0; i < 20; i++)
                                    {
                                        line = lines[lineNumber++].Trim();
                                    }
                                    break;

                                case "bidsystemew":
                                    if (string.IsNullOrEmpty(tournament.TrainerConventionCard))
                                    {
                                    }
                                    break;

                                case "bidsystemns":
                                    if (string.IsNullOrEmpty(tournament.TrainerConventionCard))
                                    {
                                    }
                                    break;

                                default:
                                    //throw new InvalidDataException(string.Format("Unrecognized item: '{0}'", itemName));
                                    // ignore new/unknown tags
                                    break;
                            }
                        }

                        continue;
                    }

                    throw new PbnException("Unrecognized line {1}: '{0}'", line, lineNumber);
                } while (moreToParse && line != null);
            }

            foreach (var board in tournament.Boards)
            {
                board.CalcBoardScore();
            }

            tournament.CalcTournamentScores();
            return tournament;
        }

        private static void PlayCard(string card, Seats who, BoardResult currentResult)
        {
            if (card != null && card.Length >= 2)
            {
                var suit = SuitHelper.FromXML(card.Substring(0, 1));
                var rank = Rank.From(card.Substring(1, 1));
                var cardCount = currentResult.Board.Distribution.Length(who);
                if (cardCount < 13 || currentResult.Board.Distribution.Owns(who, suit, rank))
                {
                    if (cardCount == 13 && currentResult.Play.leadSuit != Suits.NoTrump && suit != currentResult.Play.leadSuit)
                    {
                        var remaining = currentResult.Board.Distribution.Length(who, currentResult.Play.leadSuit) - currentResult.Play.Length(who, currentResult.Play.leadSuit);
                        if (remaining >= 1) throw new PbnException("Board {0}: error in [play] trick {3}: {2} must confess to lead suit instead of {1}", currentResult.Board.BoardNumber, card, who, currentResult.Play.currentTrick);
                    }

                    try
                    {
                        currentResult.Play.Record(suit, rank, card.Substring(2));
                    }
                    catch (FatalBridgeException x)
                    {
                        throw new PbnException("Board {0}: error in [play]: {1} {2}", currentResult.Board.BoardNumber, card, x.Message);
                    }
                }
                else
                {
                    throw new PbnException("Board {0}: error in [play]: {1} does not have {2}", currentResult.Board.BoardNumber, who, card.Substring(0, 2));
                }
            }
        }

        private static string SuitSymbols(string comment)
        {
            foreach (var prefix in "\\_")
            {
                for (Suits suit = Suits.Clubs; suit <= Suits.Spades; suit++)
                {
                    comment = comment.Replace(prefix + SuitHelper.ToXML(suit).Substring(0, 1), SuitHelper.ToUnicode(suit).ToString());
                }
            }

            return comment;
        }

        #endregion


        //[ScoreTable "PairId_NS\2R;PairId_EW\2R;Contract\6L;Declarer\1R;Result\2R"]
        //[ScoreTable "Rank\3R;Contract\5L;Declarer\1;Result\2R;Score_NS\5R;IMP_NS\6R;Multiplicity"]
        private struct ScoreTableEntry
        {
            public int Rank;
            public string Contract;
            public Seats Declarer;
            public int Result;
            public int Multiplicity;
            public string Players;
            public int ScoreNS;
            public double ImpNS;
        }
    }

    public class PbnException : NoReportException
    {
        public PbnException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }

    public class PbnTournament : Tournament
    {
        public override async Task<Board2> GetNextBoardAsync(int boardNumber, Guid userId)
        {
            if (boardNumber < 1) throw new ArgumentOutOfRangeException("boardNumber", boardNumber + " (should be 1 or more)");
            Board2 firstHigherBoard = null;
            foreach (var board in this.Boards)
            {
                if (board.BoardNumber == boardNumber)
                {
                    board.ClearCurrentResult();
                    return board;
                }
                else if (board.BoardNumber > boardNumber && (firstHigherBoard == null || firstHigherBoard.BoardNumber > board.BoardNumber))
                {
                    firstHigherBoard = board;
                }
            }

            if (userId == Guid.Empty)
            { // parsing the pbn
              // add an empty board
                Board2 newBoard = new Board2();
                newBoard.BoardNumber = boardNumber;
                this.Boards.Add(newBoard);
                return newBoard;
            }
            else
            {
                return null;
            }
        }

        public override async Task SaveAsync(BoardResult result)
        {
            foreach (var board in this.Boards)
            {
                if (board.BoardNumber == result.Board.BoardNumber)
                {
                    board.Results.Add(result);
                    return;
                }
            }
        }
    }
}
