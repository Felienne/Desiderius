using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Desiderius;
using Microsoft.FSharp.Collections;

namespace PBN_Reader
{
    class PlayingAgent
    {

      static void Main(string[] args)
      {
         string deal = "[Deal \"N:A25.63.AKQ987.A97 A8.KQ.T.QJT J97.J987.3.K KQT.AT.J.\"]";
         var hands = Desiderius.PBN.getHandList(deal);
         var dealer = Desiderius.PBN.getDealer(deal);

         var players = new List<Desiderius.Player.T>(4);


         for (int i = 0; i < players.Capacity; i++)
         {
            //var firstHand = hands[i];
            var Direction = Desi.directionFromInt(i);

            //players.Add(Desiderius.Player.create(Direction, firstHand));
         }

         //Console.WriteLine("Your cards are {0}", Desi.showHand(hands[0]));

         Console.WriteLine("What is your bid?");

         string bid = Console.ReadLine();

         var fsharpPlayers = ListModule.OfSeq(players);
         Desiderius.ContractGame.T game = Desiderius.ContractGame.create(Desi.Suit.Spades, fsharpPlayers);


         switch (bid)
         {
            case "S": game = Desiderius.ContractGame.create(Desi.Suit.Spades, fsharpPlayers);
                        break;
            case "H": game = Desiderius.ContractGame.create(Desi.Suit.Hearts, fsharpPlayers);
                        break;
            case "D": game = Desiderius.ContractGame.create(Desi.Suit.Diamonds, fsharpPlayers);
                        break;
            case "C": game = Desiderius.ContractGame.create(Desi.Suit.Spades, fsharpPlayers);
                        break;
         }


         Console.WriteLine("Your bid is {0}", bid);
         Console.WriteLine("Let the games begin!");

         var startingDirection = Desi.next(dealer.Item);

         var startingPlayer = players.First(x => x.Direction == startingDirection);

         var data1 = ListModule.OfSeq(new List<Desi.Card>());

         var dataLijst = new List<FSharpList<Desi.Card>>() {data1};

         var data = ListModule.OfSeq(dataLijst);

         //var data = Desiderius.Game.emptyHistory; //stupid null bug again :/

         var card = Desiderius.Player.nextCard(startingPlayer,data,game.Trump);

         Console.WriteLine(Desi.showCard(card));

         Console.ReadLine();

        }
    }
}
