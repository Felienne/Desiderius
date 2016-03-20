namespace Desiderius


//types of the game
module Desi =

    type Direction =  North = 0 | East = 1 | South = 2 | West = 3

    let directionFromInt (i:int):Direction = 
       enum i

    let partner(p:Direction):Direction = 
      let partnerInt = (int p + 2) % 4
      enum partnerInt

    let next(p:Direction) = 
        match p with
        | West -> Direction.North
        | East -> Direction.South
        | South -> Direction.West
        | North -> Direction.South

    type Dealer = Dealer of Direction 

    type Rank = A = 14 | K = 13 | Q = 12 | J = 11 | T = 10 | Nine = 9 | Eight = 8 | Seven = 7 | Six = 6 | Five = 5 | Four = 4 | Three = 3 | Two = 2

    type Suit = Spades = 4 | Hearts = 3 | Diamonds = 2 | Clubs = 1| SA = 5

    type Card = Card of Suit * Rank

    type Hand = 
        Hand of Card List
        | Empty

    type Deal = Deal of Dealer * list<Hand> 

    let makeEmptyHanded =  
        Hand (List.empty)
   
    let addCardtoHand(c: Card, h: Hand) = 
        match h with
        | Empty -> Hand [c]
        | Hand (l) -> Hand (c::l)

    //auxiliary functions for analysis of hands

    let cardtoPoint(c:Card): int = 
        match c with
        | Card (s,v) -> 
            match v with
            | Rank.A -> 4
            | Rank.K -> 3
            | Rank.Q -> 2
            | Rank.J -> 1
            | _ -> 0

    let cardsinHand (h:Hand) = 
        match h with
        | Hand list -> list

    let pointsinHand(h:Hand): int = 
        match h with
        | Hand list -> List.fold (+) 0 (List.map cardtoPoint list)

    let isSuit (suit: Suit) (c:Card) = 
        match c with
        | Card (s,r) -> s = suit 

    let cardsofSuitinHand(h:Hand) (s:Suit) : int = 
        match h with
        | Hand h -> List.length (List.filter (isSuit s) h)

    //auxiliary functions for showing hands

    let showCard (c:Card) = 
        match c with 
        | Card (s, r) -> (sprintf "%A" s) + (sprintf "%A" r) + " "

    let showHand (h:Hand) = 
        match h with
        | Empty -> ""
        | Hand list -> (List.map showCard list) |> List.fold (+) "" 



    //Types of the Bidding System

    type Bid = 
          Pass
        | Bid of int * Suit

    type BidHistory = Bid List

    type Condition = 
          NCards of int * int * Suit //min, max, suit
        | NPoints of int * int //min, max
        | And of Condition * Condition
        | Or of Condition * Condition

    let (&) (p:Condition)(q: Condition):Condition = 
        And (p,q)

    let (:=) (p:Condition)(b:Bid):Condition*Bid = 
        (p,b)

    let both (p:Condition)(q: Condition):Condition = //added because  And (NPoints (12,19, Suit.Spades), NCards (4,13,Suit.Spades)) had too much brackets. Was first called makeAnd, but both is nicer
        And (p,q)

    let either (p:Condition)(q: Condition):Condition = //added because  And (NPoints (12,19, Suit.Spades), NCards (4,13,Suit.Spades)) had too much brackets. Was first called makeAnd, but both is nicer
        Or (p,q)

    let all (p:Condition)(q: Condition)(r:Condition):Condition = //added because  And (NPoints (12,19, Suit.Spades), NCards (4,13,Suit.Spades)) had even more brackets.
        And(And(p,q),r)


    let forAllSuits (p:Suit -> Condition): Condition = 
        both (both (p Suit.Clubs) (p Suit.Diamonds))
            (both (p Suit.Spades) (p Suit.Hearts))

    let points (min:int) (max:int) = //same as above
        NPoints (min, max)

    let cards (min:int) (max:int) (s:Suit) = //same as above
        NCards (min, max,s)

    type RuleSet = (Condition * Bid) list

    type BiddingSystem = (BidHistory -> RuleSet) list

    let raise (b:Bid) (i:int):Bid = 
        match b with 
        | Bid (v,s) -> Bid (v+1,s)


    let Acol1(hist:BidHistory):RuleSet =

        [ points 15 17 & forAllSuits (cards 2 13) := Bid (1, Suit.SA)      
          //if possible, we always prefer to open 1SA
          //opening 1SA interesting question: do we want/need universal quantifiers? For all colors > 2
          //do we support the natively or just with a fun?

          //we bid five in order of hight to low, if there is a 6 in there (and the hard is not too strong), we also bid the highest
          points 12 19 & cards 5 13 Suit.Spades := Bid (1, Suit.Spades) 

          points 12 19 & cards 5 13 Suit.Clubs := Bid (1, Suit.Clubs) 
          points 12 19 & cards 5 13 Suit.Hearts := Bid (1, Suit.Hearts)
          points 12 19 & cards 5 13 Suit.Diamonds:= Bid (1, Suit.Diamonds)

          //we bid four in order of low to high
          points 12 19 & cards 4 13 Suit.Clubs := Bid (1, Suit.Clubs) 
          points 12 19 & cards 4 13 Suit.Diamonds := Bid (1, Suit.Diamonds) 
          points 12 19 & cards 4 13 Suit.Hearts := Bid (1, Suit.Hearts) 
          points 12 19 & cards 4 13 Suit.Spades := Bid (1, Suit.Spades)]


    let Acol2(hist:BidHistory):RuleSet = 

        //answers to a one bid here

        //we list the history from new to oldest, so our partner's answer is the second in the current list:
        // partner :: opponent :: rest of history = (me:: opponent:: partner :: etc....)

        match hist with 
        | partnerBid :: x -> //we could have also used List 2 here, but this looks nicer (it is almost the comment I typed!)
        //TODO: add 'tussenbod' here too!

        match partnerBid with
        | Bid (partnerValue, partnerSuit) ->

        //double jump is preemptive
        [ points 6 9 & cards 7 13 Suit.Diamonds := Bid (3, Suit.Diamonds) 
          points 6 9 & cards 7 13 Suit.Hearts := Bid (3, Suit.Hearts)
          points 6 9 & cards 7 13 Suit.Spades := Bid (3, Suit.Spades)

        //single jump is 6 card and strong hand
          points 13 27 & cards 6 13 Suit.Diamonds := Bid (2, Suit.Diamonds)
          points 13 27 & cards 6 13 Suit.Hearts := Bid (2, Suit.Hearts) 
          points 13 27 & cards 6 13 Suit.Spades := Bid (2, Suit.Spades) 

        //answer opening suit
          points 6 9 & cards 4 13 partnerSuit := raise partnerBid 1
          points 10 11 & cards 4 13 partnerSuit := raise partnerBid 2
          points 12 15 & cards 4 13 partnerSuit := raise partnerBid 3

        //bid new color if it can be bid on the one level
        //---> we need a comparison operatior now
          points 6 9 & cards 4 13 Suit.Diamonds := Bid (1, Suit.Diamonds)


        //SA answers
          points 6 9 & cards 0 3 partnerSuit := Bid (1, Suit.SA)  //no trump support
          points 10 11 & forAllSuits (cards 2 13) := Bid (2, Suit.SA)  //sans hand
          points 12 14 & forAllSuits (cards 2 13) := Bid (3, Suit.SA)] //sans hand

    let createAcol:BiddingSystem = Acol1 :: [Acol2]

    let rec fits (cards:Hand) (c:Condition):bool  = 
        match c with
        | NCards (min, max, s) -> cardsofSuitinHand cards s >= min && cardsofSuitinHand cards s <= max 
        | NPoints (min, max) -> pointsinHand cards >= min && pointsinHand cards <= max 
        | And (p,q) -> fits cards p && fits cards q 
        | Or (p,q) -> fits cards p || fits cards q 


    let rec getBidRule (cards:Hand) (r:RuleSet):Bid =
        match r with
        | [] -> Pass // no more rules to apply, Pass
        | ((c,b) :: t) -> match fits cards c  with
                            | true -> b  //the condition matches, so bid this bid 
                            | false -> getBidRule cards (t) //try the remaining rules

    let rec getBid (cards:Hand) (sys:BiddingSystem) (history: BidHistory) (i:int):Bid =
        match sys with 
        | [] -> Pass //there is no ruleset given, we do not know what to do so Pass
        | l -> let f = List.nth sys i //get the ith ruleset, apply it to the history
               getBidRule cards (f history) 

