﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Desiderius;
using Microsoft.FSharp.Collections;

namespace PBN
{
    public class Reader
    {



        //public static Desi.Rank getValuefromChar(char v){     
        //    switch (v)
        //   {
        //        case 'A': return Desi.Rank.A;
        //        case 'K': return Desi.Rank.K;
        //        case 'Q': return Desi.Rank.Q;
        //        case 'J': return Desi.Rank.J;
        //        case 'T': return Desi.Rank.T;
        //        case '9': return Desi.Rank.Nine;
        //        case '8': return Desi.Rank.Eight;
        //        case '7': return Desi.Rank.Seven;
        //        case '6': return Desi.Rank.Six;
        //        case '5': return Desi.Rank.Five;
        //        case '4': return Desi.Rank.Four;
        //        case '3': return Desi.Rank.Three;
        //        case '2': return Desi.Rank.Two;
        //    }

        //    return Desi.Rank.Two; //TODO!!! This should be fixed, we could move this to fsharp and use the parse function
        //    //if I have figured out how it works
        //}

        ///// <summary>
        ///// Turns a PBN string describing a hand (exmple: .63.AKQ987.A9732) into a Desi Hand
        ///// </summary>
        ///// <param name="x"></param>
        ///// <returns></returns>
        ///// 


        //private static Desi.Hand toHand(string handString)
        //{
        //    var result = Desi.makeEmptyHanded;
               
        //    //DECISION! We do represent Empty hands in the list, but they contain an empty list.

        //    if (handString != "-") //no hand given
        //    {
        //        var SuitList = handString.Split('.').ToList();

        //        var Suits = new List<Desi.Suit>() { Desi.Suit.Spades, Desi.Suit.Hearts, Desi.Suit.Diamonds, Desi.Suit.Clubs };

        //        for (int i = 0; i < 4; i++)
        //        {
        //            foreach (char individualCard in SuitList[i])
        //            {
        //                var card = Desi.Card.NewCard(Suits[i], getValuefromChar(individualCard));

        //                result = Desi.addCardtoHand(card, result);
        //            }
        //        }

        //        //TODO: add assert here that a hand always has 13 cards, or exeception
        //    }

  

        //    return result;
        //}


        //public static List<Desi.Hand> getHandList(string input)
        //{
        //    var hands = getHandsString(input);
        //    var ListofHandStrings = hands.Split(' ').ToList();

        //    //this list now contains 4 hand strings, with the suit separated by periods (i.e: .63.AKQ987.A9732)

        //    return ListofHandStrings.Select(x => toHand(x)).ToList();

        //}





        

    }
}
