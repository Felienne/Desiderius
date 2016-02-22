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


   }
}
