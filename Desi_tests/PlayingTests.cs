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

      private Desi.Card aceOfHearts = Desi.Card.NewCard(Desi.Suit.Hearts, Desi.Rank.A);
      private Desi.Card twoOfHearts = Desi.Card.NewCard(Desi.Suit.Hearts, Desi.Rank.Two);
      private Desi.Card queenOfHearts = Desi.Card.NewCard(Desi.Suit.Hearts, Desi.Rank.Q);
      private Desi.Card eigthOfHearts = Desi.Card.NewCard(Desi.Suit.Hearts, Desi.Rank.Eight);

      //make shorthand games for the four trumpColors
      private Desiderius.Game.T spadesGame = Desiderius.Game.create(Desi.Suit.Spades);
      private Desiderius.Game.T heartsGame = Desiderius.Game.create(Desi.Suit.Hearts);
      private Desiderius.Game.T diamondsGame = Desiderius.Game.create(Desi.Suit.Diamonds);
      private Desiderius.Game.T clubsGame = Desiderius.Game.create(Desi.Suit.Clubs);
      private Desiderius.Game.T SAGame = Desiderius.Game.create(Desi.Suit.SA); 

      [TestMethod]
      public void allTrumpsHighestWins()
      {
         //put in four trumps in the trick
         var trick = new List<Desi.Card>() {aceOfSpades,twoOfSpades,queenOfSpades,eigthOfSpades};
         var fsharpTrick = ListModule.OfSeq(trick);

         var winningCard = Desiderius.Game.winningCard(spadesGame, fsharpTrick);
         Assert.AreEqual(aceOfSpades, winningCard);

      }

      [TestMethod]
      public void allnonTrumpsHighestWins()
      {
         //put in four non-trumps in the trick
         var trick = new List<Desi.Card>() { aceOfSpades, twoOfSpades, queenOfSpades, eigthOfSpades };
         var fsharpTrick = ListModule.OfSeq(trick);

         var winningCard = Desiderius.Game.winningCard(clubsGame, fsharpTrick);
         Assert.AreEqual(aceOfSpades, winningCard);
      }

      [TestMethod]
      public void trumpCardBeatsHighernonTrump()
      {
         //put in one trump and 3 non-trumps in the trick
         var trick = new List<Desi.Card>() { aceOfHearts, twoOfSpades, queenOfHearts, eigthOfHearts };
         var fsharpTrick = ListModule.OfSeq(trick);

         var winningCard = Desiderius.Game.winningCard(spadesGame, fsharpTrick);
         Assert.AreEqual(twoOfSpades, winningCard);

      }

      [TestMethod]
      public void SAgameHighestOpenSuitCardsWins()
      {
         //put in cards of different colors
         var trick = new List<Desi.Card>() { twoOfHearts, twoOfSpades, queenOfHearts, eigthOfHearts };
         var fsharpTrick = ListModule.OfSeq(trick);

         var winningCard = Desiderius.Game.winningCard(SAGame, fsharpTrick);
         Assert.AreEqual(queenOfHearts, winningCard);

      }

      [TestMethod]
      public void SAgameHighestOpenSuitCardsWinsInFaceOfHigherCard()
      {
         //put in cards of different colors
         var trick = new List<Desi.Card>() { twoOfHearts, twoOfSpades, queenOfHearts, aceOfSpades };
         var fsharpTrick = ListModule.OfSeq(trick);

         var winningCard = Desiderius.Game.winningCard(SAGame, fsharpTrick);
         Assert.AreEqual(queenOfHearts, winningCard);

      }


   }
}
