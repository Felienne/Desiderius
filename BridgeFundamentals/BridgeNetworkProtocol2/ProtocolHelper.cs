using Sodes.Bridge.Base;

namespace BridgeNetworkProtocol2
{
    public static class ProtocolHelper
    {
        internal static string Translate(Vulnerable v)
        {
            switch (v)
            {
                case Vulnerable.Neither:
                    return "Neither";
                case Vulnerable.NS:
                    return "N/S";
                case Vulnerable.EW:
                    return "E/W";
                case Vulnerable.Both:
                    return "Both";
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        internal static string Translate(Seats s, Distribution d)
        {
            // "North's cards : S A K J 6.H A K J.D 8 6 2.C A 7 6."
            var cards = string.Format("{0}'s cards : ", s.ToXMLFull());
            for (Suits suit = Suits.Spades; suit >= Suits.Clubs; suit--)
            {
                cards += suit.ToXML();
                for (Ranks rank = Ranks.Ace; rank >= Ranks.Two; rank--)
                {
                    if (d.Owns(s, suit, rank)) cards += " " + rank.ToXML();
                }
                cards += ".";
            }

            return cards;
        }

        public static void HandleProtocolBid(string message, BridgeEventBus bus)
        {
            // North passes
            // North doubles
            // North redoubles
            // North bids 1H
            // North bids 1H Alert. 13 to 19 total points.
            // North bids 1H Alert.C=0-8,D=4-8,H=0-5,S=0-5,HCP=17-19,Total=19-21.
            // North bids 1H.Infos.C=0-8,D=4-8,H=0-5,S=0-5,HCP=17-19,Total=19-21.
            // North bids 1H Infos.C=0-8,D=4-8,H=0-5,S=0-5,HCP=17-19,Total=19-21.
            bool bidWasAlerted = false;
            string bidPhrase;
            string alertPhrase = string.Empty;
            int startAlert = message.ToLower().IndexOf("alert.");
            if (startAlert == -1) startAlert = message.ToLower().IndexOf("infos.");
            if (startAlert >= 0)
            {
                bidWasAlerted = true;
                bidPhrase = message.Substring(0, startAlert).Trim();
                //alertPhrase = Helpers.AlertFromTM(message.Substring(startAlert + 6).Trim());
            }
            else
            {
                bidWasAlerted = false;
                bidPhrase = message.Trim();
            }

            // 24-07-09: TableMoniteur adds a . after a bid: "North doubles."
            if (bidPhrase.EndsWith(".")) bidPhrase = bidPhrase.Substring(0, bidPhrase.Length - 1);

            string[] answer = bidPhrase.Split(' ');
            Seats bidder = SeatsExtensions.FromXML(answer[0]);
            var bid = new Bid(answer[answer.Length - 1], alertPhrase);
            if (bidWasAlerted)      // && alertPhrase.Length == 0)
            {
                bid.NeedsAlert();
            }

            bus.HandleBidDone(bidder, bid);
        }

        public static void HandleProtocolPlay(string message, BridgeEventBus bus)
        {
            // North plays 3C
            string[] answer = message.Split(' ');
            var bidder = SeatsExtensions.FromXML(answer[0]);
            var suit = SuitHelper.FromXML(answer[2][1]);
            var rank = Rank.From(answer[2][0]);
            bus.HandleCardPosition(bidder, suit, rank);
            bus.HandleCardPlayed(bidder, suit, rank);
        }

        internal static string Translate(Bid bid, Seats source)
        {
            string bidText = SeatsExtensions.ToXMLFull(source) + " ";
            switch (bid.Special)
            {
                case SpecialBids.Pass:
                    bidText += "passes";
                    break;
                case SpecialBids.Double:
                    bidText += "doubles";
                    break;
                case SpecialBids.Redouble:
                    bidText += "redoubles";
                    break;
                case SpecialBids.NormalBid:
                    bidText += "bids " + ((int)bid.Level).ToString() + (bid.Suit == Suits.NoTrump ? "NT" : bid.Suit.ToString().Substring(0, 1));
                    break;
            }

#if Olympus
                bidText += " Infos." + this.AlertToTM(bid.Explanation, source);
#else
            if (bid.Alert
                //&& this.sendAlerts
                )
            {
                bidText += " Alert. " + AlertToTM(bid.Explanation, source);
            }
#endif
            return bidText;
        }

        private static string AlertToTM(string alert, Seats whoseRule)
        {
            string result = "";
#if Olympus
            // pH0510*=H5*!S4*(C4+D4)
            // C=0-9,D=0-9,H=5-5,S=0-3,HCP=04-11,Total=06-11
            var parseInfo = Rule.Conclude(alert, this.InterpretFactor, this.ConcludeFactor, whoseRule, false);
            for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
            {
                result += string.Format("{0}={1:0}-{2:0},", s.ToParser(), parseInfo.L[s].Min, parseInfo.L[s].Max > 9 ? 9 : parseInfo.L[s].Max);
            }

            result += string.Format("HCP={0:00}-{1:00},", parseInfo.P.Min, parseInfo.P.Max);
            result += string.Format("Total={0:00}-{1:00}.", parseInfo.FitPoints.Min, parseInfo.FitPoints.Max);
#endif
            return result;
        }

        //private FactorInterpretation InterpretFactor(string factor, Seats whoseRule)
        //{
        //    return FactorInterpretation.Maybe;
        //}

        //private SpelerBeeld ConcludeFactor(string factor, FactorInterpretation expected, Seats whoseRule, bool afterwards)
        //{
        //    if (factor.Length == 0) throw new FatalBridgeException("empty factor not allowed");

        //    var commentStart = factor.IndexOf('[');
        //    if (commentStart >= 0)
        //    {
        //        var commentEnd = factor.IndexOf(']');
        //        factor = factor.Remove(commentStart, commentEnd - commentStart + 1);
        //    }

        //    var FactorInfo = new SpelerBeeld();

        //    if (factor.Length == 0) return FactorInfo; ;

        //    var bidInfo = new SpelerBeeld();
        //    MinMax Range = new MinMax(0, 40);
        //    Suits Kleur;

        //    switch (expected)
        //    {
        //        case FactorInterpretation.False:
        //            switch (factor[0])
        //            {
        //                case 'K':
        //                case 'R':
        //                case 'H':
        //                case 'S':
        //                case 'D':
        //                case 'C':
        //                    Kleur = SuitHelper.FromXML(factor[0]);
        //                    Range.Max = int.Parse(factor.Substring(1)) - 1;
        //                    Range.Min = 0;
        //                    FactorInfo.L[Kleur].KGV(Range);
        //                    break;

        //                case 'p':
        //                    char Code = factor[1];
        //                    Range.Min = int.Parse(factor.Substring(2, 2));
        //                    Range.Max = int.Parse(factor.Substring(4, 2));

        //                    if (Code == 'c')
        //                        Range.Min += 2;

        //                    if (Range.Min == 0 && Range.Max <= 30)
        //                    {
        //                        Range.Min = Range.Max + 1;
        //                        Range.Max = 40;
        //                        FactorInfo.P.KGV(Range);
        //                        if (Code == 'C' || Code == 'D' || Code == 'H' || Code == 'S' || Code == 'K' || Code == 'R' || Code == 'N')
        //                            FactorInfo.ToonVerdeling(bidInfo, SuitHelper.FromXML(Code), false);
        //                    }
        //                    else if (Range.Max >= 30)
        //                    {
        //                        Range.Max = Range.Min - 1;
        //                        Range.Min = 0;
        //                        FactorInfo.P.KGV(Range);
        //                        if (Code == 'C' || Code == 'D' || Code == 'H' || Code == 'S' || Code == 'K' || Code == 'R' || Code == 'N')
        //                            FactorInfo.ToonVerdeling(bidInfo, SuitHelper.FromXML(Code), false);
        //                    }

        //                    break;

        //                default:
        //                    break;
        //            }

        //            break;

        //        case FactorInterpretation.True:
        //            switch (factor[0])
        //            {
        //                case 'p':
        //                    char Code = factor[1];
        //                    Range.Min = int.Parse(factor.Substring(2, 2));
        //                    Range.Max = int.Parse(factor.Substring(4, 2));
        //                    if (Code == 'c') Range.Min += 2;

        //                    FactorInfo.P.KGV(Range);
        //                    if (Code == 'c' || Code == 'C' || Code == 'D' || Code == 'H' || Code == 'S' || Code == 'K' || Code == 'R' || Code == 'N')
        //                        FactorInfo.ToonVerdeling(bidInfo, SuitHelper.FromXML(Code), false);

        //                    break;

        //                case 't':
        //                    int i = int.Parse(factor.Substring(1, 2));
        //                    switch (i)
        //                    {
        //                        case 1:
        //                        case 7:
        //                        case 8:    // 10-11-95    Doe alsof ik een NoTrump verdeling heb
        //                            for (Suits s = Suits.Clubs; s <= Suits.Spades; s++)
        //                            {
        //                                Range.Min = 2;
        //                                Range.Max = i == 1 || s.IsMajor() ? 5 : 6;
        //                                FactorInfo.L[s].KGV(Range);
        //                            }
        //                            /*
        //                                Range.Max = 4;
        //                                FactorInfo.L[Suits.Hearts].KGV(@Range);    { Tenzij er een hoge 5-kaart in de 1 Suits.NoTrump mag?  }
        //                                FactorInfo.L[Suits.Spades].KGV(@Range);    */
        //                            break;
        //                        case 3:
        //                        case 4:
        //                        case 5:
        //                        case 6:
        //                            FactorInfo.P.Min = 10;  // Zodat na 1S 4S de 4S bieder 12-14 punten aangeeft, i.p.v. 13-14
        //                            FactorInfo.P.Max = 19;
        //                            switch (i)
        //                            {
        //                                case 3:
        //                                    FactorInfo.L[Suits.Clubs].Min = 3;
        //                                    break;

        //                                case 4:
        //                                case 5:
        //                                    FactorInfo.L[Suits.Diamonds].Min = 4;
        //                                    break;

        //                                case 6:
        //                                    FactorInfo.L[Suits.Spades].Min = 5;
        //                                    break;
        //                            }
        //                            break;
        //                    }

        //                    break;

        //                case 'K':
        //                case 'R':
        //                case 'H':
        //                case 'S':
        //                case 'D':
        //                case 'C':
        //                    Kleur = SuitHelper.FromXML(factor[0]);
        //                    Range.Min = int.Parse(factor.Substring(1));
        //                    Range.Max = 13;
        //                    FactorInfo.L[Kleur].KGV(Range);
        //                    break;

        //                case '=':
        //                    Kleur = SuitHelper.FromXML(factor[1]);
        //                    Range.Min = int.Parse(factor.Substring(2));
        //                    Range.Max = Range.Min;
        //                    FactorInfo.L[Kleur].KGV(Range);
        //                    break;

        //            }

        //            break;
        //    }

        //    return FactorInfo;
        //}
    }
}
