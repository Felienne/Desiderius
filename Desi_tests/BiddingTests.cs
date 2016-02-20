using System;
using System.Collections.Generic;
using Desiderius;
using Microsoft.FSharp.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Desi_tests
{
    [TestClass]
    public class BiddingTests
    {
        public static FSharpList<Desi.Bid> emptyHistory = ListModule.OfSeq(new List<Desi.Bid>());

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

             var firstCard = ((Desi.Hand.Hand)(firstHand)).Item[0];

             Assert.AreEqual(expectedFirstCard, firstCard);
             
         }


         [TestMethod]
         public void getHandsListWhenOnlyTwoArePresent()
         {
             string deal = "[Deal \"W:KQT2.AT.J6542.85 - A8654.KQ5.T.QJT6 -\"]";

             var expectedFirstCard = Desi.Card.NewCard(Desi.Suit.Clubs, Desi.Rank.Five);

             var result = PBN.Reader.getHandList(deal);
             var firstHand = result[0];

             var firstCard = ((Desi.Hand.Hand)(firstHand)).Item[0];

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
             string deal = "[Deal \"W:AQT52.AT.KJ6.852 - A8654.KQ5.T.QJT6 -\"]";
             var expectedBid = Desi.Bid.NewBid(1, Desi.Suit.Spades);

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];

             var pointsWest = Desi.pointsinHand(westHand);

             var history = emptyHistory;

             var bid = Desi.getBid(westHand, Desi.createAcol, history,0);

             Assert.AreEqual(expectedBid, bid);

         }

         [TestMethod]
         public void ACOL_bid_1_clubs_if_also_4_spades()
         {


             string deal = "[Deal \"W:J752.KT.KJ6.AJ85 - A8654.KQ5.T.QJT6 -\"]";

             var expectedBid = Desi.Bid.NewBid(1, Desi.Suit.Clubs);

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];

             var pointsWest = Desi.pointsinHand(westHand);

             var history = emptyHistory;

             var bid = Desi.getBid( westHand, Desi.createAcol, history,0);

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

            var history = emptyHistory;

             var bid = Desi.getBid( westHand, Desi.createAcol, history,0);

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

             var history = emptyHistory;

             var bid = Desi.getBid( westHand, Desi.createAcol, history,0);

             Assert.AreNotEqual(SAbid, bid);
         }


         [TestMethod]
         public void ACOL_Prefer_5_over_4()
         {
             string deal = "[Deal \"W:AQ752.A2.AJ86.K2 - A8654.KQ5.T.QJT6 -\"]";

             var Spades1 = Desi.Bid.NewBid(1, Desi.Suit.Spades);

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];

             var history = emptyHistory;

             var bid = Desi.getBid( westHand, Desi.createAcol, history,0);

             Assert.AreEqual(Spades1, bid);
         }

         [TestMethod]
         public void ACOL_Two_Fivers_Open_Highest()
         {
             string deal = "[Deal \"W:AQ752.AT2.K.AJ865 - A8654.KQ5.T.QJT6 -\"]";

             var Spades1 = Desi.Bid.NewBid(1, Desi.Suit.Spades);

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];

             var history = emptyHistory;

             var bid = Desi.getBid( westHand, Desi.createAcol, history,0);

             Assert.AreEqual(Spades1, bid);
         }


        //------------------tests for answering bids

         [TestMethod]
         public void ACOL_Open_1_Clubs_Answer_Pass()
         {
             string deal = "[Deal \"W:AJ72.AT2.K.AJ865 - A8654.532.T.JT63 -\"]";

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];
             var eastHand = result[2];

             var pointsWest = Desi.pointsinHand(westHand);
             var pointsEast = Desi.pointsinHand(eastHand);

             var history = emptyHistory;

             var bidResultWest = Desi.getBid(westHand, Desi.createAcol, history,0);

             Assert.AreEqual(Desi.Bid.NewBid(1, Desi.Suit.Clubs), bidResultWest);

             history = ListModule.OfSeq(new List<Desi.Bid> {bidResultWest});

             var bidResultEast = Desi.getBid(westHand, Desi.createAcol, history, 1);

             var pass = Desi.Bid.Pass;

             Assert.AreEqual(pass, bidResultEast);
         }


         [TestMethod]
         public void ACOL_Open_1_Clubs_Answer_Support_Two_Level()
         {
             string deal = "[Deal \"W:J872.AT2.K4.A865 - A864.532.T.AJT63 -\"]";

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];
             var eastHand = result[2];

             var pointsWest = Desi.pointsinHand(westHand);
             var pointsEast = Desi.pointsinHand(eastHand);

             var history = emptyHistory;
             var bidResultWest = Desi.getBid(westHand, Desi.createAcol, history, 0);
             Assert.AreEqual(Desi.Bid.NewBid(1, Desi.Suit.Clubs), bidResultWest);

             history = ListModule.OfSeq(new List<Desi.Bid> { bidResultWest });
             var bidResultEast = Desi.getBid(eastHand, Desi.createAcol, history, 1);
             var supportBid = Desi.Bid.NewBid(2, Desi.Suit.Clubs);
             Assert.AreEqual(supportBid, bidResultEast);
         }

         [TestMethod]
         public void ACOL_Open_1_Hearts_Answer_Support_Two_Level()
         {
             string deal = "[Deal \"W:K62.J872.AT2.A85 - A864.AJT3.8532.T -\"]";

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];
             var eastHand = result[2];

             var pointsWest = Desi.pointsinHand(westHand);
             var pointsEast = Desi.pointsinHand(eastHand);

             var history = emptyHistory;
             var bidResultWest = Desi.getBid(westHand, Desi.createAcol, history, 0);
             Assert.AreEqual(Desi.Bid.NewBid(1, Desi.Suit.Hearts), bidResultWest);

             history = ListModule.OfSeq(new List<Desi.Bid> { bidResultWest });
             var bidResultEast = Desi.getBid(eastHand, Desi.createAcol, history, 1);
             var supportBid = Desi.Bid.NewBid(2, Desi.Suit.Hearts);
             Assert.AreEqual(supportBid, bidResultEast);
         }

         [TestMethod]
         public void ACOL_Open_1_Hearts_Answer_SA()
         {
             string deal = "[Deal \"W:K62.J872.AT2.A85 - A864.T8.532.AJT3 -\"]";

             var result = PBN.Reader.getHandList(deal);
             var westHand = result[0];
             var eastHand = result[2];

             var pointsWest = Desi.pointsinHand(westHand);
             var pointsEast = Desi.pointsinHand(eastHand);

             var history = emptyHistory;

             var bidResultWest = Desi.getBid(westHand, Desi.createAcol, history, 0);
             
             Assert.AreEqual(Desi.Bid.NewBid(1, Desi.Suit.Hearts), bidResultWest);

             history = ListModule.OfSeq(new List<Desi.Bid> { bidResultWest });
             var bidResultEast = Desi.getBid(eastHand, Desi.createAcol, history, 1);
             var supportBid = Desi.Bid.NewBid(1, Desi.Suit.SA);
             Assert.AreEqual(supportBid, bidResultEast);
         }





        ////----------------tests for "tussenbods"
        // [TestMethod]
        // public void ACOL_pass_if_opponent_opens()
        // {
        //     string deal = "[Deal \"N:.63.AKQ98.A9732 AK86.KQ5.T.QJT6 J973.J98742.3.K4 QT254.AT.J6542.85\"]";

        //     var expectedBid = Desi.Bid.Pass; //North opens, so even though East has 4 spades and 15 points, he does not open 1 of Spades

        //     var result = PBN.Reader.getHandList(deal);
        //     var eastHand = result[1];

        //     var pointsEast = Desi.pointsinHand(eastHand);

        //     var history = ListModule.OfSeq(new List<Tuple<Desi.Player, Desi.Bid>>

        //     {
        //        new Tuple<Desi.Player, Desi.Bid>(Desi.Player.North, Desi.Bid.NewBid(1, Desi.Suit.Spades))
        //     }
                 
                 
        //     );

        //     var bid = Desi.getBid(Desi.Player.East, eastHand, Desi.createAcol, history,0);

        //     Assert.AreEqual(expectedBid, bid);

        // }
    }
}
