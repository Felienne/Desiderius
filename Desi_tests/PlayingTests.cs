using System;
using System.Collections.Generic;
using System.Diagnostics;
using Desiderius;
using Microsoft.FSharp.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Desi_tests
{
   [TestClass]
   public class PlayingTests
   {
      //define some shorthand vars for cards as they are needed in many tests.
      private Desi.Card aceOfSpades = Desi.Card.NewCard(Desi.Suit.Spades, Desi.Rank.A);
      private Desi.Card twoOfSpades = Desi.Card.NewCard(Desi.Suit.Spades, Desi.Rank.Two);
      private Desi.Card queenOfSpades = Desi.Card.NewCard(Desi.Suit.Spades, Desi.Rank.Q);
      private Desi.Card eigthOfSpades = Desi.Card.NewCard(Desi.Suit.Spades, Desi.Rank.Eight);

      private Desi.Card kingofClubs = Desi.Card.NewCard(Desi.Suit.Clubs, Desi.Rank.K);
      private Desi.Card twoofClubs = Desi.Card.NewCard(Desi.Suit.Clubs, Desi.Rank.Two);
      private Desi.Card queenofClubs = Desi.Card.NewCard(Desi.Suit.Clubs, Desi.Rank.Q);
      private Desi.Card tenOfClubs = Desi.Card.NewCard(Desi.Suit.Clubs, Desi.Rank.A);

      private Desi.Card aceOfHearts = Desi.Card.NewCard(Desi.Suit.Hearts, Desi.Rank.A);
      private Desi.Card twoOfHearts = Desi.Card.NewCard(Desi.Suit.Hearts, Desi.Rank.Two);
      private Desi.Card queenOfHearts = Desi.Card.NewCard(Desi.Suit.Hearts, Desi.Rank.Q);
      private Desi.Card eigthOfHearts = Desi.Card.NewCard(Desi.Suit.Hearts, Desi.Rank.Eight);

      //make shorthand games for the four trumpColors
      private Desiderius.ContractGame.T spadesGame = Desiderius.ContractGame.create(Desi.Suit.Spades, Desiderius.ContractGame.emptyPlayers);
      private Desiderius.ContractGame.T heartsGame = Desiderius.ContractGame.create(Desi.Suit.Hearts, Desiderius.ContractGame.emptyPlayers);
      private Desiderius.ContractGame.T diamondsGame = Desiderius.ContractGame.create(Desi.Suit.Diamonds, Desiderius.ContractGame.emptyPlayers);
      private Desiderius.ContractGame.T clubsGame = Desiderius.ContractGame.create(Desi.Suit.Clubs, Desiderius.ContractGame.emptyPlayers);
      private Desiderius.ContractGame.T SAGame = Desiderius.ContractGame.create(Desi.Suit.SA, Desiderius.ContractGame.emptyPlayers); 


      [TestMethod]
      public void allTrumpsHighestWins()
      {
         //put in four trumps in the trick
         var trick = new List<Desi.Card>() {aceOfSpades,twoOfSpades,queenOfSpades,eigthOfSpades};
         var fsharpTrick = ListModule.OfSeq(trick);

         var winningCard = Desiderius.ContractGame.winningCard(spadesGame, fsharpTrick);
         Assert.AreEqual(aceOfSpades, winningCard);

      }

      [TestMethod]
      public void allnonTrumpsHighestWins()
      {
         //put in four non-trumps in the trick
         var trick = new List<Desi.Card>() { aceOfSpades, twoOfSpades, queenOfSpades, eigthOfSpades };
         var fsharpTrick = ListModule.OfSeq(trick);

         var winningCard = Desiderius.ContractGame.winningCard(clubsGame, fsharpTrick);
         Assert.AreEqual(aceOfSpades, winningCard);
      }

      [TestMethod]
      public void trumpCardBeatsHighernonTrump()
      {
         //put in one trump and 3 non-trumps in the trick
         var trick = new List<Desi.Card>() { aceOfHearts, twoOfSpades, queenOfHearts, eigthOfHearts };
         var fsharpTrick = ListModule.OfSeq(trick);

         var winningCard = Desiderius.ContractGame.winningCard(spadesGame, fsharpTrick);
         Assert.AreEqual(twoOfSpades, winningCard);

      }

      [TestMethod]
      public void SAgameHighestOpenSuitCardsWins()
      {
         //put in cards of different colors
         var trick = new List<Desi.Card>() { twoOfHearts, twoOfSpades, queenOfHearts, eigthOfHearts };
         var fsharpTrick = ListModule.OfSeq(trick);

         var winningCard = Desiderius.ContractGame.winningCard(SAGame, fsharpTrick);
         Assert.AreEqual(queenOfHearts, winningCard);

      }

      [TestMethod]
      public void SAgameHighestOpenSuitCardsWinsInFaceOfHigherCard()
      {
         //put in cards of different colors
         var trick = new List<Desi.Card>() { twoOfHearts, twoOfSpades, queenOfHearts, aceOfSpades };
         var fsharpTrick = ListModule.OfSeq(trick);

         var winningCard = Desiderius.ContractGame.winningCard(SAGame, fsharpTrick);
         Assert.AreEqual(queenOfHearts, winningCard);

      }



      [TestMethod]
      public void openWithHighestCard()
      {
         string deal = "[Deal \"N:A5.63.KQ987.9732 A8654.KQ5.T.QJT6 J973.J98742.3.K4 KQT2.AT.J6542.85\"]";
         var northHand = Desiderius.PBN.getHandList(deal)[0];

         var valid = Desiderius.PBN.validDeal(Desiderius.PBN.getHandList(deal));

         var player = Desiderius.Player.create(Desi.Direction.North, northHand);
         var nextCard = Desiderius.Player.nextCard(player, Desiderius.ContractGame.emptyHistory, Desi.Suit.Clubs);

         Assert.AreEqual(aceOfSpades, nextCard);
      }

      [TestMethod]
      public void openWithHighestCardOnSecondTrick()
      {
         string deal = "[Deal \"N:A5.63.KQ987.9732 A8654.KQ5.T.QJT6 J973.J98742.3.K4 KQT2.AT.J6542.85\"]";
         var northHand = Desiderius.PBN.getHandList(deal)[0];

         var history = Desiderius.ContractGame.emptyHistory;
         history = Desiderius.ContractGame.addToHistory(history, twoOfHearts);
         history = Desiderius.ContractGame.addToHistory(history, kingofClubs);
         history = Desiderius.ContractGame.addToHistory(history, queenOfSpades);
         history = Desiderius.ContractGame.addToHistory(history, aceOfHearts);

         var player = Desiderius.Player.create(Desi.Direction.North, northHand);

         var nextCard = Desiderius.Player.nextCard(player, history, Desi.Suit.Clubs);

         Assert.AreEqual(aceOfSpades, nextCard);
      }


      [TestMethod]
      public void followSuit()
      {
         string deal = "[Deal \"N:A5.83.KQ987.9732 A8654.KQ5.T.QJT6 J973.J98742.3.K4 KQT2.AT.J6542.85\"]";
         var northHand = Desiderius.PBN.getHandList(deal)[0];

         //create a played game:

         var history = Desiderius.ContractGame.addToHistory(Desiderius.ContractGame.emptyHistory,twoOfHearts);

         var player = Desiderius.Player.create(Desi.Direction.North, northHand);
         var nextCard = Desiderius.Player.nextCard(player, history, Desi.Suit.Clubs);


         Assert.AreEqual(eigthOfHearts, nextCard);
      }

      [TestMethod]
      public void ifFollowSuitDoItHigherIfYouCan()
      {
         string deal = "[Deal \"N:A5.83.KQ987. A8654.KQ5.T.QJT6 J973.J98742.3.K97432 KQT2.AT.J6542.85\"]";
         var northHand = Desiderius.PBN.getHandList(deal)[0];

         //create a played game:

         var history = Desiderius.ContractGame.addToHistory(Desiderius.ContractGame.emptyHistory, twoOfSpades);

         var player = Desiderius.Player.create(Desi.Direction.North, northHand);
         var nextCard = Desiderius.Player.nextCard(player, history, Desi.Suit.Hearts);

         Assert.AreEqual(aceOfSpades, nextCard);
      }

      [TestMethod]
      public void ifFollowSuitPlaysLowestIfYouCantGoHigher()
      {
         string deal = "[Deal \"N:A5.82.KQ987. A8654.KQ5.T.QJT6 J973.J98742.3.K97432 KQT2.AT.J6542.85\"]";
         var northHand = Desiderius.PBN.getHandList(deal)[0];

         //create a played game:

         var history = Desiderius.ContractGame.addToHistory(Desiderius.ContractGame.emptyHistory, queenOfHearts);

         var player = Desiderius.Player.create(Desi.Direction.North, northHand);
         var nextCard = Desiderius.Player.nextCard(player, history, Desi.Suit.Clubs);

         Assert.AreEqual(twoOfHearts, nextCard);
      }

      [TestMethod]
      public void ifNotFollowSuitPlayTrump()
      {
         string deal = "[Deal \"N:A5.83.KQ987. A8654.KQ5.T.QJT6 J973.J98742.3.K97432 KQT2.AT.J6542.85\"]";
         var northHand = Desiderius.PBN.getHandList(deal)[0];

         //create a played game:

         var history = Desiderius.ContractGame.addToHistory(Desiderius.ContractGame.emptyHistory, twoofClubs);

         var player = Desiderius.Player.create(Desi.Direction.North, northHand);
         var nextCard = Desiderius.Player.nextCard(player, history, Desi.Suit.Hearts);

         Assert.AreEqual(eigthOfHearts, nextCard);
      }





   }
}
