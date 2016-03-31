using System;
using System.Runtime.Serialization;

namespace Sodes.Bridge.Base
{
    /// <summary>
    /// Summary description for contract.
    /// </summary>
    [DataContract]
    public class Contract
    {
        private Bid theBid;
        private bool _doubled;
        private bool _redoubled;
        private Seats _declarer;
        private int declarerTricks;
        private int defenseTricks;
        private Vulnerable theVulnerability;

        public Contract(Bid b, bool d, bool r, Seats de, bool v)
        {
            theBid = b; _doubled = d; _redoubled = r; _declarer = de;
            this.theVulnerability = v ? Vulnerable.Both : Vulnerable.Neither;
            //declarerTricks = 0; defenseTricks = 0;
        }
        public Contract(Bid b, bool d, bool r, Seats declarer, Vulnerable v)
        {
            theBid = b; _doubled = d; _redoubled = r; _declarer = declarer;
            this.theVulnerability = v;
            //declarerTricks = 0; defenseTricks = 0;
        }
        public Contract(string fromXML, Seats de, Vulnerable v)
        {
            _declarer = de;
            this.theVulnerability = v;
            //declarerTricks = 0; defenseTricks = 0;
            fromXML = fromXML.ToLower();
            if (fromXML == "pass")
            {
                theBid = new Bid(SpecialBids.Pass);
                //_doubled = false; _redoubled = false;
            }
            else
            {
                string doubled = "";
                if (fromXML.IndexOf("x") > 0)
                {
                    doubled = fromXML.Substring(fromXML.IndexOf("x"));
                    fromXML = fromXML.Substring(0, fromXML.IndexOf("x"));
                }
                theBid = new Bid(fromXML.ToUpper());
                if (doubled == "xx")
                {
                    _doubled = true;
                    _redoubled = true;
                }
                else
                    if (doubled == "x")
                    {
                        _doubled = true;
                        _redoubled = false;
                    }
                    else
                    {
                        _doubled = false;
                        _redoubled = false;
                    }
            }
        }

        /// <summary>
        /// Just for the serializer
        /// </summary>
        private Contract()
        {
        }

        [DataMember]
        public Bid Bid { get { return theBid; } set { theBid = value; } }

        [DataMember]
        public bool Doubled { get { return _doubled; } set { _doubled = value; } }

        [DataMember]
        public bool Redoubled { get { return _redoubled; } set { _redoubled = value; } }

        [DataMember]
        public Seats Declarer { get { return _declarer; } set { _declarer = value; } }

        [DataMember]
        public Vulnerable Vulnerability { get { return this.theVulnerability; } set { this.theVulnerability = value; } }

        /// <summary>
        /// Localized contract text
        /// </summary>
        [IgnoreDataMember]
        public string Text
        {
            get
            {
                string s = Declarer.ToString2().Substring(0, 1) + ": ";
                s += Bid.ToText();
                s += (Doubled ? "x" : "");
                s += (Redoubled ? "x" : "");
                s += " ";
                if (Bid.IsRegular && this.tricksForDeclarer + this.tricksForDefense == 13)
                {
                    int result = tricksForDeclarer - 6 - (int)Bid.Level;
                    s += (result == 0) ? "C" : (result > 0 ? "+" : "-") + ((int)Math.Abs(result)).ToString();
                }

                return s;
            }
        }

        public string ToXML() { return Bid.ToXML() + (Doubled ? "x" : "") + (Redoubled ? "x" : ""); }

        [IgnoreDataMember]
        public int tricksForDeclarer
        {
            get { return declarerTricks; }
            set
            {
                if (value < 0 || value > 13) throw new FatalBridgeException("Illegal number of tricks");
                declarerTricks = value;
            }
        }

        [IgnoreDataMember]
        public int tricksForDefense
        {
            get { return defenseTricks; }
            set
            {
                if (value < 0 || value > 13) throw new FatalBridgeException("Illegal number of tricks");
                defenseTricks = value;
            }
        }

        public int Score
        {
            get
            {
                int scoreDeclarer = 0;
                if (Bid.IsRegular)
                {
                    int contractLevel = (int)(Bid.Level);
                    int contractResult = declarerTricks - (6 + contractLevel);
                    if (contractResult < 0)
                    {      // downslagen
                        if (DeclarerIsVulnerable)
                        {
                            if (Doubled)
                            {
                                if (Redoubled)
                                    scoreDeclarer = 200 + 600 * contractResult;
                                else
                                    scoreDeclarer = 100 + 300 * contractResult;
                            }
                            else
                                scoreDeclarer = 100 * contractResult;
                        }
                        else
                        {
                            if (Doubled)
                            {
                                if (Redoubled)
                                {
                                    if (contractResult >= -3)
                                        scoreDeclarer = 200 + 400 * contractResult;
                                    else
                                        scoreDeclarer = 800 + 600 * contractResult;
                                }
                                else
                                {
                                    if (contractResult >= -3)
                                        scoreDeclarer = 100 + 200 * contractResult;
                                    else
                                        scoreDeclarer = 400 + 300 * contractResult;
                                }
                            }
                            else
                                scoreDeclarer = 50 * contractResult;
                        }
                    }
                    else  // contract gemaakt
                    {
                        int contract_waarde, manche_waarde, slem_waarde;
                        contract_waarde = 50;
                        if (this.Doubled) contract_waarde += 50;
                        if (this.Redoubled) contract_waarde += 50;

                        if (!DeclarerIsVulnerable)
                        {
                            manche_waarde = 250;
                            slem_waarde = 500;
                        }
                        else
                        {
                            manche_waarde = 450;
                            slem_waarde = 750;
                        }
                        int slag_waarde;
                        switch (Bid.Suit)
                        {
                            case Suits.Clubs:
                            case Suits.Diamonds:
                                slag_waarde = 20; break;
                            case Suits.Hearts:
                            case Suits.Spades:
                                slag_waarde = 30; break;
                            default:  // case Suits.NoTrump:
                                {
                                    slag_waarde = 30;
                                    scoreDeclarer += 10;
                                    if (Doubled) scoreDeclarer += 10;
                                    if (Redoubled) scoreDeclarer += 20;
                                    break;
                                }
                        }
                        if (Doubled) slag_waarde *= 2;
                        if (Redoubled) slag_waarde *= 2;
                        scoreDeclarer += contractLevel * slag_waarde;
                        if (scoreDeclarer >= 100) scoreDeclarer += manche_waarde;
                        scoreDeclarer += contract_waarde;
                        if (contractLevel > 5) scoreDeclarer += slem_waarde;
                        if (contractLevel > 6) scoreDeclarer += slem_waarde;
                        if (contractResult > 0)
                        {    // overslagen
                            if (Doubled)
                            {
                                slag_waarde = (Redoubled ? 200 : 100);
                                //                if (!Redoubled)
                                //                  slag_waarde = 100;
                                //                else
                                //                  slag_waarde = 200;
                                if (DeclarerIsVulnerable) slag_waarde *= 2;
                            }
                            scoreDeclarer += contractResult * slag_waarde;
                        }
                    }
                }
                return scoreDeclarer;
            }
        }
        //------------------------------------------------------
        public int MagNogMissen()
        {
            return System.Math.Max(0, 7 - (int)Bid.Level - this.defenseTricks);
        }
        //----------------------------------------------------------------------
        public int NogNodig()
        {
            return System.Math.Max(0, (int)Bid.Level + 6 - tricksForDeclarer - AlDown());
            // als ik al down ben, heb ik minder slagen nodig om het maximaal haalbare resultaat te krijgen.
        }
        //--------------------------------------------------------------------------------
        public int AlDown()
        {
            return System.Math.Max(0, this.defenseTricks  // al verloren
              - (7 - (int)Bid.Level)        // mag verliezen
              );
        }
        private static bool IsDeclarerVulnerable(Seats declarer, Vulnerable vulnerability)
        {
            return (vulnerability == Vulnerable.Both
              || (vulnerability == Vulnerable.NS && (declarer == Seats.North || declarer == Seats.South))
              || (vulnerability == Vulnerable.EW && (declarer == Seats.East || declarer == Seats.West)));
        }

        [IgnoreDataMember]
        public bool DeclarerIsVulnerable { get { return IsDeclarerVulnerable(this._declarer, this.theVulnerability); } }

        public bool IsLeader(Seats x) { return x == Declarer || x.Partner() == Declarer; }

        public Contract Clone()
        {
            return (Contract)this.MemberwiseClone();
        }
    }
}
