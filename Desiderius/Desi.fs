//types of the game
module Desi

type Player = West | South | North | East

let partner(p:Player) = 
    match p with
    | West -> East
    | East -> West
    | South -> North
    | North -> South

type Dealer = Dealer of Player 

type Rank = A | K | Q | J | T | Nine | Eight | Seven | Six | Five | Four | Three | Two

type Suit = Spades | Hearts | Diamonds | Clubs | SA 

type Card = Card of Suit * Rank

type Hand = 
    Hand of list<Card>
    | Empty

type Deal = Deal of Dealer * list<Hand> 

let makeEmptyHanded =  //TODO: Ask Don why this results in null!
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
        | A -> 4
        | K -> 3
        | Q -> 2
        | J -> 1
        | _ -> 0

let pointsinHand(h:Hand): int = 
    match h with
    | Hand list -> List.fold (+) 0 (List.map cardtoPoint list)

let isSuit (suit: Suit) (c:Card) = 
    match c with
    | Card (s,r) -> s = suit 

let cardsofSuitinHand(h:Hand) (s:Suit) : int = 
    match h with
    | Hand h -> List.length (List.filter (isSuit s) h)



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

let both (p:Condition)(q: Condition):Condition = //added because  And (NPoints (12,19, Spades), NCards (4,13,Spades)) had too much brackets. Was first called makeAnd, but both is nicer
    And (p,q)

let either (p:Condition)(q: Condition):Condition = //added because  And (NPoints (12,19, Spades), NCards (4,13,Spades)) had too much brackets. Was first called makeAnd, but both is nicer
    Or (p,q)

let all (p:Condition)(q: Condition)(r:Condition):Condition = //added because  And (NPoints (12,19, Spades), NCards (4,13,Spades)) had even more brackets.
    And(And(p,q),r)


let forAllSuits (p:Suit -> Condition): Condition = 
    both (both (p Clubs) (p Diamonds))
        (both (p Spades) (p Hearts))

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

    [ (points 15 17 & forAllSuits (cards 2 13), Bid (1, SA))        
      //if possible, we always prefer to open 1SA
      //opening 1SA interesting question: do we want/need universal quantifiers? For all colors > 2
      //do we support the natively or just with a fun?

      //we bid five in order of hight to low, if there is a 6 in there (and the hard is not too strong), we also bid the highest
      (both (points 12 19) (cards 5 13 Spades) , Bid (1, Spades))
      (both (points 12 19) (cards 5 13 Clubs) , Bid (1, Clubs)) 
      (both (points 12 19) (cards 5 13 Hearts) , Bid (1, Hearts)) 
      (both (points 12 19) (cards 5 13 Diamonds), Bid (1, Diamonds))

      //we bid four in order of low to high
      (both (points 12 19) (cards 4 13 Clubs) , Bid (1, Clubs)) 
      (both (points 12 19) (cards 4 13 Diamonds) , Bid (1, Diamonds)) 
      (both (points 12 19) (cards 4 13 Hearts) , Bid (1, Hearts)) 
      (both (points 12 19) (cards 4 13 Spades) , Bid (1, Spades))]


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
    [ (both (points 6 9) (cards 7 13 Diamonds), Bid (3, Diamonds)) 
      (both (points 6 9) (cards 7 13 Hearts), Bid (3, Hearts)) 
      (both (points 6 9) (cards 7 13 Spades), Bid (3, Spades)) 

    //single jump is 6 card and strong hand
      (both (points 13 27) (cards 6 13 Diamonds), Bid (2, Diamonds)) 
      (both (points 13 27) (cards 6 13 Hearts), Bid (2, Hearts)) 
      (both (points 13 27) (cards 6 13 Spades), Bid (2, Spades)) 

    //answer opening suit
      (both (points 6 9) (cards 4 13 partnerSuit), raise partnerBid 1) 
      (both (points 10 11) (cards 4 13 partnerSuit), raise partnerBid 2) 
      (both (points 12 15) (cards 4 13 partnerSuit), raise partnerBid 3) 

    //bid new color if it can be bid on the one level
    //---> we need a comparison operatior now
      (both (points 6 9) (cards 4 13 Diamonds), Bid (1, Diamonds)) 
//     (both (points 10 27) (cards 5 13 Hearts), Bid (2, Hearts)) ::
//     (both (points 10 27) (cards 5 13 Spades), Bid (2, Spades)) ::

    //SA answers
      (both (points 6 9) (cards 0 3 partnerSuit), Bid (1, SA))  //no trump support
      (both (points 10 11) (forAllSuits (cards 2 13)), Bid (2, SA))  //sans hand
      (both (points 12 14) (forAllSuits (cards 2 13)), Bid (3, SA))] //sans hand

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

