using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sodes.Base.Test.Helpers;
using Sodes.Bridge.Base;
using System.IO;
using System;
using System.Net;

namespace BridgeFundamentals.UnitTests
{
    [TestClass]
    public class TournamentTest : TestBase
    {

        [TestMethod, TestCategory("CI"), TestCategory("Other"), DeploymentItem("TestData\\WC2005final01.pbn")]
        public void Tournament_SavePbn()
        {
            var original = TournamentLoad("WC2005final01.pbn");
            var allPass = new BoardResult("", original.Boards[0], new Participant("test1", "test1", "test1", "test1"));
            allPass.Auction.Record(Bid.C("p"));
            allPass.Auction.Record(Bid.C("p"));
            allPass.Auction.Record(Bid.C("p"));
            allPass.Auction.Record(Bid.C("p"));
            original.Boards[0].Results.Add(allPass);
            var partialPlay = new BoardResult("", original.Boards[0], new Participant("test2", "test2", "test2", "test2"));
            partialPlay.HandleBidDone(Seats.North, Bid.C("1S"));
            partialPlay.HandleBidDone(Seats.East, Bid.C("p"));
            partialPlay.HandleBidDone(Seats.South, Bid.C("p"));
            partialPlay.HandleBidDone(Seats.West, Bid.C("p"));
            partialPlay.HandleCardPlayed(Seats.East, Suits.Hearts, Ranks.King);
            partialPlay.HandleCardPlayed(Seats.South, Suits.Hearts, Ranks.Two);
            partialPlay.HandleCardPlayed(Seats.West, Suits.Hearts, Ranks.Three);
            partialPlay.HandleCardPlayed(Seats.North, Suits.Hearts, Ranks.Ace);
            partialPlay.HandleCardPlayed(Seats.North, Suits.Spades, Ranks.Ace);
            original.Boards[0].Results.Add(partialPlay);
            var partialAuction = new BoardResult("", original.Boards[0], new Participant("test3", "test3", "test3", "test3"));
            partialAuction.Auction.Record(Bid.C("1S"));
            partialAuction.Auction.Record(Bid.C("p"));
            partialAuction.Auction.Record(Bid.C("p"));
            original.Boards[0].Results.Add(partialAuction);
            Pbn2Tournament.Save(original, File.Create("t1.pbn"));
            var copy = TournamentLoad("t1.pbn");
            Assert.AreEqual(original.EventName, copy.EventName, "EventName");
            Assert.AreEqual<DateTime>(original.Created, copy.Created, "Created");
            Assert.AreEqual<int>(original.Boards.Count, copy.Boards.Count, "Boards.Count");
        }

        [TestMethod, TestCategory("CI"), TestCategory("Other"), DeploymentItem("TestData\\WC2005final01.pbn")]
        public void Tournament_Load_WC2005final01pbn()
        {
            Tournament target = TournamentLoad("WC2005final01.pbn");
            Assert.IsTrue(target.GetNextBoard(1, Guid.Empty).Results[0].Play.AllCards.Count > 0, "pbn: No played cards");
            //TournamentLoader.Save("WC2005final01.trn", target);
            //target = TournamentLoad("WC2005final01.trn");
            //Assert.IsTrue(target.Boards.Count == 16, "No 16 boards");
            //Assert.IsTrue(target.GetBoard(1, false).Distribution.Owns(Seats.North, Suits.Spades, Ranks.Ace));
            //Assert.IsTrue(target.GetBoard(1, false).Results.Count == 2, "Board 1 does not have 2 results");
            ////Assert.IsTrue(target.GetBoard(1, false).Results[0].Participants.Count == 4, "No 4 participants");
            //Assert.IsTrue(target.GetBoard(1, false).Results[0].Auction != null, "No auction");
            //Assert.IsTrue(target.GetBoard(1, false).Results[0].Play != null, "No play");
            //Assert.IsTrue(target.GetBoard(1, false).Results[0].Play.AllCards.Count > 0, "No played cards");
        }

        [TestMethod]
        public void Tournament_Load_Http()
        {
            Tournament target = TournamentLoad("http://bridge.nl/groepen/Wedstrijdzaken/1011/Ruitenboer/RB11_maandag.pbn");
        }

        public static Tournament TournamentLoad(string fileName)
        {
            if (fileName.StartsWith("http://"))
            {
                var url = new Uri(fileName);
                var req = WebRequest.Create(url);
                var resp = req.GetResponse();
                var stream = resp.GetResponseStream();
                return TournamentLoader.LoadAsync(stream).Result;
            }
            else
            {
                return TournamentLoader.LoadAsync(File.OpenRead(fileName)).Result;
            }
        }
    }
}
