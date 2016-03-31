using System;

namespace Sodes.Bridge.Base
{
    /// <summary>
    /// Summary description for Conventie.
    /// </summary>
    public enum Conventies
    {
        EersteConventie,
        Jacoby,
        RomanKeyCards,
        Multi,
        Ghestem,
        Intermediate,
        Balancing,
        Muiderberg,
        FourthSuitForcing,
        MultiLandy,
        RedoubletNaInfodoublet,
        SfVerdediging,
        TruscottAfterMinor,
        NoSplinter,
        Splinter,
        ControlShowing,
        LongSuitTrial,      // pass 1H pass 2H pass 3D: 3D shows a bit more than 3H
        SupportCue,          // pass 1H 1S pass 2H: 2H shows spade support
        CheckBackStayman  // 1m pass 1M pass 1NT pass 2C asks for 3 card in the major
        ,
        Niemeijer
            ,
        Rubensohl
            ,
        Lebensohl
            ,
        StaymanRelay
            ,
        NT2S_8pt_or_6card_low
            ,
        NT2S_7_9pt_with_5card_low
            ,
        Stayman
            ,
        LandyDefense
            ,
        GhestemDefense
            ,
        WeakTwoDefense
            ,
        PreemptDefense
            ,
        Michaels
            ,
        WeakTwoMajors
            ,
        Preempt
            ,
        NT_MinorTransfers
            ,
        BergenRaises
            ,
        TruscottClassic
            ,
        WeakJumpOvercalls
            ,
        SandwichNT
            ,
        Landy
            ,
        Flannery
            ,
        FlanneryDefense
            ,
        Strong2D
            ,
        TruscottAfterPass
            ,
        Smolen
            ,
        MinorSuitStayman
            ,
        Gambling3NT
            ,
        Cappelletti
            ,
        DONT
            ,
        Asptro
            ,
        ReverseDrury
            ,
        PuppetStayman
            ,
        NewMinorForcing
            ,
        LeapingMichaels
            ,
        FiveCardAfterOverCall
            ,
        ConventionsOnAfter1NTx
            ,
        WeakTwoDiamonds
            ,
        WeakJumpShift3Level
            ,
        WeakJumpShift2Level
            ,
        NT_MinorTransfers2
            ,
        Strong2C
            ,
        Quantative
            ,
        SemiStrong2C
            ,
        LightPreempt
            ,
        NT_MinorTransfers3
            ,
        ThirdSuitForcing
            ,
        JacobyDefense
            ,
        GhestemNT
            ,
        TwoWayCheckbackStayman
            ,
        PuppetStayman2NT
            ,
        PuppetStayman1NT
            ,
        RomanKeyCards1430
            ,
        MontrealRelay
            ,
        Blackwood
            ,
        RomanBlackwood
            ,
        DopiRopi
            ,
        DipoRipo
            ,
        DopeRope
            ,
        DepoRepo
            ,
        StaymanBasic
            ,
        InvertedMinors
            ,
        StaymanSmart
            ,
        Ghestem1D3D
            ,
        LebensohlReverse
            ,
        FourthSuitGameForcing
            ,
        WeakJumpInCompetition
            ,
        SupportCueAdvanced          // pass 1H 1S pass 2H: 2H shows spade support or balanced hand without stopper or strong hand
            ,
        TransferEscapes             // pass 1H 1NT x 2H: 2H shows spades
            ,
        LaatsteConventie
    }

    public static class BridgeConventions
    {
        public static string[] ConventionName = new string[] {
            "",
            "Jacoby",
            "Roman Keycards",
            "Multi",
            "Ghestem",
            "Intermediate",
            "Balancing",
            "Muiderberg",
            "Fourth suit forcing",
            "Multi-Landy",
            "RedoubleAfterInfoDouble",
            "SF verdediging",
            "TruscottAfterMinor",
            "NoSplinter",
            "Splinter",
            "ControlShowing",
            "Long suit trial",
            "Support cue",
            "Checkback Stayman",
            "Niemeijer" ,
            "Rubensohl" ,
            "Lebensohl",
            "StaymanRelay"
            ,"1NT-2S: 8pt or 6-card low"
            ,"1NT-2S: 7-9pt with 5-card low"
            ,"Stayman"
            ,"LandyDefense"
            ,"GhestemDefense"
            ,"WeakTwoDefense"
            ,"PreemptDefense"
            ,"Michaels"
            ,"WeakTwoMajors"
            ,"Preempt"
            ,"NT_MinorTransfers"
            ,"BergenRaises"
            ,"TruscottClassic"
            ,"WeakJumpOvercalls"
            ,"SandwichNT"
            ,"Landy"
            ,"Flannery"
            ,"FlanneryDefense"
            ,"Strong2D"
            ,"TruscottAfterPass"
            ,"Smolen"
            ,"MinorSuitStayman"
            ,"Gambling3NT"
            ,"Cappelletti"
            ,"DONT"
            ,"Asptro"
            ,"ReverseDrury"
            ,"NotUsedAnymore"
            ,"NewMinorForcing"
            ,"LeapingMichaels"
            ,"FiveCardAfterOverCall"
            ,"ConventionsOnAfter1NTx"
            ,"WeakTwoDiamonds"
            ,"WeakJumpShift3Level"
            ,"WeakJumpShift2Level"
            ,"NT_MinorTransfers2"
            ,"Strong2C"
            ,"Quantative"
            ,"SemiStrong2C"
            ,"LightPreempt"
            ,"NT_MinorTransfers3"
            ,"ThirdSuitForcing"
            ,"JacobyDefense"
            ,"GhestemNT"
            ,"Two-way Checkback Stayman"
            ,"PuppetStayman2NT"
            ,"PuppetStayman1NT"
            ,"RomanKeyCards1430"
            ,"MontrealRelay"
            ,"Blackwood"
            ,"RomanBlackwood"
            ,"DopiRopi"
            ,"DipoRipo"
            ,"DopeRope"
            ,"DepoRepo"
            ,"StaymanBasic"
            , "Inverted Minors"
            ,"StaymanSmart"
            , "Ghestem1D3D"
            , "LebensohlReverse"
            , "FourthSuitGameForcing"
            , "WeakJumpInCompetition"
            , "SupportCueAdvanced"      // pass 1H 1S pass 2H: 2H shows spade support or balanced hand without stopper or strong hand
            , "TransferEscapes"         // pass 1H 1NT x 2H: 2H shows spades
            ,""
        };

        public static string[] Explanations = new string[] {
            "",
            "After a 1NT opening of partner, bid 2D to show 5H or bid 2H to show 5S",
            "aka RKC, ask for aces (trump King is 5th ace)",
            "2D opening bid showing either a weak two-bid in either major or 23-24 NT or strong in C or D: ",
            "A system of two-suited overcalls",		// Ghestem
            "Jump overcall promising 11-15 HCP",	// Intermediate
            "Balancing",					// Balancing
            "Muiderberg",
            "4th suit is forcing for one round",
            "Multi-Landy",
            "RedoubleAfterInfoDouble",
            "SF verdediging",
            "TruscottAfterMinor",
            "NoSplinter",
            "Splinter",
            "ControlShowing",
            "Long suit trial",
            "Support cue",
            "Checkback Stayman",
            "Niemeijer" ,
            "Rubensohl" ,
            "Lebensohl",
            "Stayman without requirement for 4-card in major"
            ,"1NT-2S: 8pt or 6-card low"
            ,"1NT-2S: 7-9pt with 5-card low"
            ,"Stayman possibly with weak hand"
            ,"LandyDefense"
            ,"GhestemDefense"
            ,"WeakTwoDefense"
            ,"PreemptDefense"
            ,"Michaels"
            ,"2H/2S opening showing 6-11hcp with 6 hearts/spades"
            ,"Preempt"
            ,"after 1NT: 2S=6+C, 2NT=8-9, 3C=6+D"
            ,"BergenRaises"
            ,"TruscottClassic"
            ,"WeakJumpOvercalls"
            ,"SandwichNT"
            ,"Landy"
            ,"Flannery"
            ,"FlanneryDefense"
            ,"23+ (should be combined with semi-forcing 2C)"
            ,"TruscottAfterPass"
            ,"Smolen"
            ,"MinorSuitStayman"
            ,"Gambling3NT"
            ,"Cappelletti"
            ,"DONT"
            ,"Asptro"
            ,"ReverseDrury"
            ,"Not used anymore. Replace with Puppet Stayman after 1NT or 2NT"
            ,"NewMinorForcing"
            ,"LeapingMichaels"
            ,"1C 1D 1H/S or 1D 1H 1S shows 5-card"
            ,"Use same conventions as after a regular 1NT opening"
            ,"2D opening showing 6-11hcp with 6 diamonds"
            ,"1X p 3Y shows weak hand with long (6+) Y and short (1-) X where Y < X (1H p 3C)"
            ,"1X p 2Y shows weak hand with long (6+) Y and short (1-) X where Y > X (1C p 2H)"
            ,"after 1NT: 2S=6+ C or D, 2NT=8-9"
            ,"22+"
            ,"1NT-4NT and 2NT-4NT: asks opener to bid 6NT with a maximum"
            ,"20+"
            ,"Even lighter preempts when partner has already passed"
            ,"after 1NT: 2S=6+C, 2NT=6+D"			// NT_MinorTransfers3
            ,"After 1D 1H 2D, 2S is the only strong bid"		// ThirdSuitForcing
            ,"After a Jacoby transfer the transferred suit shows an info double"		// JacobyDefense
            ,"Ghestem after 1X pass 1NT"
            ,"Two-way Checkback Stayman"
            ,"Puppet Stayman after 2NT"
            ,"Puppet Stayman (2C) after 1NT"
            ,"aka RKC, ask for aces (trump King is 5th ace), answers of 5C and 5D switched"		// RomanKeyCards1430
            ,"MontrealRelay"
            ,"ace-asking 5C=0 or 4,5D=1,5H=2,5S=3"
            ,"ace-asking 5C=0 or 3,5D=1 or 4,5H=2"
            ,"defense against overcalls during ace-asking: (re)double=0/3,pass=1/4,first bid=2"
            ,"defense against overcalls during ace-asking: (re)double=1/4,pass=0/3,first bid=2"
            ,"defense against overcalls during ace-asking: (re)double=odd,pass=even"
            ,"defense against overcalls during ace-asking: (re)double=even,pass=odd"
            , "Stayman 8+hcp"
            , "1m 2m is 10+, 1m 3m is 5-9"
            , "Stayman 8+hcp, no 2NT answer"
            , "After 1D, 3D shows 5H5S and 3C shows preempt in clubs"
            , "After reverse at 2 level shows a weak hand"
            , "4th suit is forcing to game (additional to FourthSuitForcing)"
            , "After an overcall a jump is weak"
            , "shows support for partners suit or balanced hand without stopper or strong hand"     // SupportCueAdvanced
            , "Use transfers to escape from 1X 1NT x"         // TransferEscapes
            , ""
        };

        public static string[] KnownConventions()
        {
            string known = "";
            for (Conventies c = Conventies.EersteConventie + 1; c < Conventies.LaatsteConventie; c++)
            {
                if (known.Length > 0) known += "|";
                known += BridgeConventions.ConventionName[(int)c];
            }
            return known.Split('|');
        }

        public static bool Depricated(string conventionName)
        {
            foreach (var name in BridgeConventions.ConventionName)
            {
                if (conventionName == name) return false;
            }

            return true;
        }

        public static string Explanation(string conventionName)
        {
            for (int c = 0; c < BridgeConventions.ConventionName.Length; c++)
            {
                if (BridgeConventions.ConventionName[c] == conventionName) return BridgeConventions.Explanations[c];
            }

            return "Depricated. Please remove.";
        }

        public static Conventies Convert(string conventionName)
        {
            for (int c = 0; c < BridgeConventions.ConventionName.Length; c++)
            {
                if (BridgeConventions.ConventionName[c] == conventionName) return (Conventies)c;
            }

            throw new ArgumentOutOfRangeException("conventionName", conventionName + " unknown convention");
        }

        public static bool IsValidBaseCard(string baseCard)
        {
            return ("FiveCardMajor3C;FiveCardMajor2C;FiveCardMajorOr4H4S;TwoOverOne".Contains(baseCard));
        }

        public static bool IsValidConventionList(string conventions)
        {
            foreach (var convention in conventions.Split(';'))
            {
                BridgeConventions.Convert(convention);
            }

            return true;
        }
    }
}
