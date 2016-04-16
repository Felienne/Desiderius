namespace Desiderius

open Desi

module Acol = 
  let Acol1(hist : BidHistory) : RuleSet = 
    [ points 15 17 & forAllSuits (cards 2 13) := Bid(1, Suit.SA)
      //if possible, we always prefer to open 1SA
      //we bid five in order of high to low, if there is a 6 in there 
      //(and the hard is not too strong), 
      //we also bid the highest
      points 12 19 & cards 5 13 Suit.Spades := Bid(1, Suit.Spades)
      points 12 19 & cards 5 13 Suit.Hearts := Bid(1, Suit.Hearts)
      points 12 19 & cards 5 13 Suit.Diamonds := Bid(1, Suit.Diamonds)
      points 12 19 & cards 5 13 Suit.Clubs := Bid(1, Suit.Clubs)
      //we bid four in order of low to high
      points 12 19 & cards 4 13 Suit.Clubs := Bid(1, Suit.Clubs)
      points 12 19 & cards 4 13 Suit.Diamonds := Bid(1, Suit.Diamonds)
      points 12 19 & cards 4 13 Suit.Hearts := Bid(1, Suit.Hearts)
      points 12 19 & cards 4 13 Suit.Spades := Bid(1, Suit.Spades) ]
  
  let Acol2(hist : BidHistory) : RuleSet = 
    //answers to a one bid here
    //we list the history from new to oldest, so our partner's answer is the second in the current list:
    // partner :: opponent :: rest of history = (me:: opponent:: partner :: etc....)
    match hist with
    | partnerBid :: x -> //we could have also used List 2 here, but this looks nicer (it is almost the comment I typed!)
      //TODO: add 'tussenbod' here too!
      match partnerBid with
      | Bid(partnerValue, partnerSuit) -> 
        [ //Jackoby 
          If(partnerBid = Bid(1, Suit.SA)) & points 6 9 & cards 5 13 Suit.Hearts := Bid(2, Suit.Diamonds)
          If(partnerBid = Bid(1, Suit.SA)) & points 6 9 & cards 5 13 Suit.Spades := Bid(2, Suit.Hearts)
          //next up: Staymen
          //double jump is preemptive
          points 6 9 & cards 7 13 Suit.Diamonds := Bid(3, Suit.Diamonds)
          points 6 9 & cards 7 13 Suit.Hearts := Bid(3, Suit.Hearts)
          points 6 9 & cards 7 13 Suit.Spades := Bid(3, Suit.Spades)
          //single jump is 6 card and strong hand
          points 13 27 & cards 6 13 Suit.Diamonds := Bid(2, Suit.Diamonds)
          points 13 27 & cards 6 13 Suit.Hearts := Bid(2, Suit.Hearts)
          points 13 27 & cards 6 13 Suit.Spades := Bid(2, Suit.Spades)
          //answer opening suit
          points 6 9 & cards 4 13 partnerSuit := raise partnerBid 1
          points 10 11 & cards 4 13 partnerSuit := raise partnerBid 2
          points 12 15 & cards 4 13 partnerSuit := raise partnerBid 3
          //bid new color if it can be bid on the one level
          If(partnerSuit < Suit.Diamonds) & points 6 9 & cards 4 13 Suit.Diamonds := Bid(1, Suit.Diamonds)
          If(partnerSuit < Suit.Hearts) & points 6 9 & cards 4 13 Suit.Hearts := Bid(1, Suit.Hearts)
          If(partnerSuit < Suit.Spades) & points 6 9 & cards 4 13 Suit.Spades := Bid(1, Suit.Spades)
          //answer with SA
          points 6 9 & cards 0 3 partnerSuit := Bid(1, Suit.SA) //no trump support
          points 10 11 & forAllSuits (cards 2 13) := Bid(2, Suit.SA) //sans hand
          points 12 14 & forAllSuits (cards 2 13) := Bid(3, Suit.SA) ] //sans hand
  
  let createAcol : BiddingSystem = Acol1 :: [ Acol2 ]
