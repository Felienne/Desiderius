using System;
using System.Collections.Generic;
using Microsoft.FSharp.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Desi_tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void getDealerfromDealString()
        {
            string deal = "[Deal \"N:.63.AKQ987.A9732 A8654.KQ5.T.QJT6 J973.J98742.3.K4 KQT2.AT.J6542.85\"]";

            //get dealer

            var expected = Desi.Dealer.NewDealer(Desi.Player.North);
            var result = PBN.Reader.getDealer(deal);

            Assert.AreEqual(expected,result);

        }

         [TestMethod]
        public void getHandsStringfromDealString()
        {
            string deal = "[Deal \"N:.63.AKQ987.A9732 A8654.KQ5.T.QJT6 J973.J98742.3.K4 KQT2.AT.J6542.85\"]";

            //get hands

            var expected = ".63.AKQ987.A9732 A8654.KQ5.T.QJT6 J973.J98742.3.K4 KQT2.AT.J6542.85";
            var result = PBN.Reader.getHandsString(deal);

            Assert.AreEqual(expected, result);

        }


         [TestMethod]
         public void getHandsListfromDealString()
         {
             string deal = "[Deal \"N:.63.AKQ987.A9732 A8654.KQ5.T.QJT6 J973.J98742.3.K4 KQT2.AT.J6542.85\"]";

             var expectedFirstCard = Desi.Card.NewCard(Desi.Suit.Clubs, Desi.Rank.Two);

             var result = PBN.Reader.getHandList(deal);

             Assert.AreEqual(4, result.Count, "4 hands were given");


             var firstHand = result[0];

             var firstCard = firstHand.Item[0];

             Assert.AreEqual(expectedFirstCard, firstCard);
             
         }


         [TestMethod]
         public void getHandsListWhenOnlyTwoArePresent()
         {
             string deal = "[Deal \"W:KQT2.AT.J6542.85 - A8654.KQ5.T.QJT6 -\"]";

             var expectedFirstCard = Desi.Card.NewCard(Desi.Suit.Clubs, Desi.Rank.Five);

             var result = PBN.Reader.getHandList(deal);
             var firstHand = result[0];

             var nonEmptyHands = result.FindAll(x => !x.Item.IsEmpty);

             Assert.AreEqual(2, nonEmptyHands.Count , "2 hands were given");

             var firstCard = firstHand.Item[0];

             Assert.AreEqual(expectedFirstCard, firstCard);

         }


         [TestMethod]
         public void getPointsforAHand()
         {
             string deal = "[Deal \"W:KQT2.AT.J6542.85 - A8654.KQ5.T.QJT6 -\"]";

             var expectedPointsWest = 10;

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];

             var pointsWest = Desi.pointsinHand(westHand);

             Assert.AreEqual(expectedPointsWest, pointsWest);

         }

         [TestMethod]
         public void getSpadesforAHand()
         {
             string deal = "[Deal \"W:KQT2.AT.J6542.85 - A8654.KQ5.T.QJT6 -\"]";

             var expectedSpadesWest = 4;

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];

             var spadesWest = Desi.cardsinHand(westHand, Desi.Suit.Spades);

             Assert.AreEqual(expectedSpadesWest, spadesWest);

         }

         [TestMethod]
         public void getClubsforAHand()
         {
             string deal = "[Deal \"W:KQT2.AT.J6542.85 - A8654.KQ5.T.QJT6 -\"]";

             var expectedClubsWest = 2;

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];

             var clubsWest = Desi.cardsinHand(westHand, Desi.Suit.Clubs);

             Assert.AreEqual(expectedClubsWest, clubsWest);

         }

        //---------------Test for a first bidding system

         [TestMethod]
         public void ACOL_bid_1_spades()
         {
             string deal = "[Deal \"W:AKQT52.AT.KJ6.85 - A8654.KQ5.T.QJT6 -\"]";
             var expectedBid = Desi.Bid.NewBid(1, Desi.Suit.Spades);

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];

             var pointsWest = Desi.pointsinHand(westHand);

             var bid = Desi.getBid(westHand, Desi.createAcol);

             Assert.AreEqual(expectedBid, bid);

         }
    }
}
