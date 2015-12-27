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

             var spadesWest = Desi.cardsofSuitinHand(westHand, Desi.Suit.Spades);

             Assert.AreEqual(expectedSpadesWest, spadesWest);

         }

         [TestMethod]
         public void getClubsforAHand()
         {
             string deal = "[Deal \"W:KQT2.AT.J6542.85 - A8654.KQ5.T.QJT6 -\"]";

             var expectedClubsWest = 2;

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];

             var clubsWest = Desi.cardsofSuitinHand(westHand, Desi.Suit.Clubs);

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

             var history = ListModule.OfSeq(new List<Tuple<Desi.Player, Desi.Bid>>());
        
             var bid = Desi.getBid(Desi.Player.West, westHand, Desi.createAcol, history);

             Assert.AreEqual(expectedBid, bid);

         }

         [TestMethod]
         public void ACOL_bid_1_clubs_if_also_4_spades()
         {


             string deal = "[Deal \"W:Q752.AT.KJ6.AJ85 - A8654.KQ5.T.QJT6 -\"]";

             var expectedBid = Desi.Bid.NewBid(1, Desi.Suit.Clubs);

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];

             var pointsWest = Desi.pointsinHand(westHand);

             var history = ListModule.OfSeq(new List<Tuple<Desi.Player, Desi.Bid>>());

             var bid = Desi.getBid(Desi.Player.West, westHand, Desi.createAcol, history);

             Assert.AreEqual(expectedBid, bid);
         }

         [TestMethod]
         public void ACOL_bid_1_SA()
         {
             string deal = "[Deal \"W:A752.AT2.K6.AJ854 - A8654.KQ5.T.QJT6 -\"]";
             
             var expectedBid = Desi.Bid.NewBid(1, Desi.Suit.SA);

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];

             var pointsWest = Desi.pointsinHand(westHand);

             var history = ListModule.OfSeq(new List<Tuple<Desi.Player, Desi.Bid>>());

             var bid = Desi.getBid(Desi.Player.West, westHand, Desi.createAcol, history);

             Assert.AreEqual(expectedBid, bid);
         }

         [TestMethod]
         public void ACOL_No_1_SA_bid_because_singleton()
         {
             string deal = "[Deal \"W:AQ752.AT2.K.AJ865 - A8654.KQ5.T.QJT6 -\"]";

             var SAbid = Desi.Bid.NewBid(1, Desi.Suit.SA);

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];

             var pointsWest = Desi.pointsinHand(westHand);

             var history = ListModule.OfSeq(new List<Tuple<Desi.Player, Desi.Bid>>());

             var bid = Desi.getBid(Desi.Player.West, westHand, Desi.createAcol, history);

             Assert.AreNotEqual(SAbid, bid);
         }

        //------------------tests for answering bids

         [TestMethod]
         public void ACOL_pass_if_opponent_opens()
         {
             string deal = "[Deal \"N:.63.AKQ98.A9732 AK86.KQ5.T.QJT6 J973.J98742.3.K4 QT254.AT.J6542.85\"]";

             var expectedBid = Desi.Bid.Pass; //North opens, so even though East has 4 spades and 15 points, he does not open 1 of Spades

             var result = PBN.Reader.getHandList(deal);
             var eastHand = result[1];

             var pointsEast = Desi.pointsinHand(eastHand);

             var history = ListModule.OfSeq(new List<Tuple<Desi.Player, Desi.Bid>>

             {
                new Tuple<Desi.Player, Desi.Bid>(Desi.Player.North, Desi.Bid.NewBid(1, Desi.Suit.Spades))
             }
                 
                 
             );

             var bid = Desi.getBid(Desi.Player.East, eastHand, Desi.createAcol, history);

             Assert.AreEqual(expectedBid, bid);

         }
    }
}
