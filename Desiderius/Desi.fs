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

type Hand = Hand of list<Card>

type Deal = Deal of Dealer * list<Hand> 

let makeEmpyHanded =  //TODO: Ask Don why this results in null!
    Hand (List.empty)
   
let addCardtoHand(c: Card, h: Hand) = 
    match h with
    | Hand (l) -> Hand (c::l)

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
    | Hand [] -> 0
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

type State =
    Open
    | Answer
    | Middle //what is a good name for the Dutch 'tussenbod'?

type BidHistory = 
    List<Player * Bid>

type Condition = 
      NCards of int * int * Suit //min, max, suit
    | NPoints of int * int //min, max, suit
    | And of Condition * Condition

let both (p:Condition)(q: Condition):Condition = //added because  And (NPoints (12,19, Spades), NCards (4,13,Spades)) had too much brackets. Was first called makeAnd, but both is nicer
    And (p,q)

let forAllSuits (p:Suit -> Condition): Condition = 
    both (both (p Clubs) (p Diamonds))
        (both (p Spades) (p Hearts))

let points (min:int) (max:int) = //same as above
    NPoints (min, max)

let cards (min:int) (max:int) (s:Suit) = //same as above
    NCards (min, max,s)

type BiddingSystem = list<Condition * State * Bid> //initially state was modeled as a condition but it just did feel nice (for example then
                                                   // in fits card the whole history needed to be inputted

let createAcol =
    //if possible, we always prefer to open 1SA
    (both (points 15 17) (forAllSuits (cards 2 13)), Open, Bid (1, SA)) ::

    //opening bids color, in order of bidding
    (both (points 12 19) (cards 4 13 Clubs), Open, Bid (1, Clubs)) ::
    (both (points 12 19) (cards 4 13 Diamonds), Open, Bid (1, Diamonds)) ::
    (both (points 12 19) (cards 4 13 Hearts), Open, Bid (1, Hearts)) ::
    (both (points 12 19) (cards 4 13 Spades), Open, Bid (1, Spades)) ::
    

    
    //opening 1SA interesting question: do we want/need universal quantifiers? For all colors > 2
    //do we support the natively or just with a fun

    //answers
    ((points 0 5), Answer, Pass) ::


    List.Empty

let rec fits (cards:Hand) (c:Condition):bool  = 
    match c with
    | NCards (min, max, s) -> cardsofSuitinHand cards s >= min && cardsofSuitinHand cards s <= max 
    | NPoints (min, max) -> pointsinHand cards >= min && pointsinHand cards <= max 
    | And (p,q) -> fits cards p && fits cards q 

let rec stateAvailable (p:Player) (s:State) (history:BidHistory) : bool = 
    match s with
    //if no one has opened yet (i.e. only passes have occured) we may do so
    | Open -> not (List.exists (fun (x,b) -> b <> Pass) history)
    //we can only answer if our partner has already bid
    | Answer -> List.exists (fun (x,b) -> x = partner p) history

let rec getBid (p:Player) (cards:Hand) (sys:BiddingSystem) (history: BidHistory):Bid =
    match sys with
    | [] -> Pass
    //we need a bid that fits our cards and isavailable (i.e. only 1 person can open)
    | (c,s,b) :: t -> match fits cards c && stateAvailable p s history with
                        | true -> b
                        | false -> getBid p cards t history
