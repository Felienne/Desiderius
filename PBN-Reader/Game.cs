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
         var hands = PBN.Reader.getHandList(deal);
         var dealer = PBN.Reader.getDealer(deal);

         var players = new List<Desiderius.Player.T>(4);


         for (int i = 0; i < players.Capacity; i++)
         {
            var firstHand = hands[i];
            Desi.Player Direction = Desi.playerFromInt(i);

            players.Add(Desiderius.Player.create(Direction, firstHand));
         }

         Console.WriteLine("Your cards are {0}", Desi.showHand(hands[0]));

         Console.WriteLine("What is your bid?");

         string bid = Console.ReadLine();

         Desiderius.Game.T game = Desiderius.Game.create(Desi.Suit.Spades);

         switch (bid)
         {
            case "S": game = Desiderius.Game.create(Desi.Suit.Spades);
                        break;
            case "H": game = Desiderius.Game.create(Desi.Suit.Hearts);
                        break;
            case "D": game = Desiderius.Game.create(Desi.Suit.Diamonds);
                        break;
            case "C": game = Desiderius.Game.create(Desi.Suit.Spades);
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
