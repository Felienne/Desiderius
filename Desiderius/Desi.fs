//types of the game
module Desi

type Player = West | South | North | East

type Dealer = Dealer of Player //DECISION! We make a dealer type to make it more clear what the player in the deal represents

type Rank = A | K | Q | J | T | Nine | Eight | Seven | Six | Five | Four | Three | Two

type Suit = Spades | Hearts | Diamonds | Clubs   

type Card = Card of Suit * Rank

type Hand = Hand of list<Card>

type Deal = Deal of Dealer * list<Hand> //DECISION! Hands do not have an onwer, but owners are defined from the Dealer (this is why we need to also ave empty hands)

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

let cardsinHand(h:Hand, s:Suit) : int = 
    match h with
    | Hand h -> List.length (List.filter (isSuit s) h)



//Types of the Bidding System

type Bid = 
      Pass
    | Bid of int * Suit

type Condition = 
      NCards of int * int * Suit //min, max, suit
    | NPoints of int * int * Suit //min, max, suit
    | And of Condition * Condition
    
type Match =
      Yes of Bid
    | No

type BiddingSystem = list<Condition* Bid>

let addRule(cond: Condition, bid:Bid, s: BiddingSystem) = 
    (cond, bid) :: s

let createAcol =
    let cond = And(NPoints (12,19, Spades), NCards (4,13,Spades))
    addRule(cond, Bid (1,Spades), List.Empty)

let isYes (m: Match) : bool = 
    match m with 
    | Yes (x) -> true
    | No -> false

let rec fits (cards:Hand) (c:Condition, b:Bid)  = 
    match c with
    | NCards (min, max, s) -> if cardsinHand(cards,s) > min && cardsinHand(cards,s) < max then Yes (b) else No
    | NPoints (min, max, s) -> if pointsinHand(cards) > min && pointsinHand(cards) < max then Yes (b) else No
    | And (p,q) -> if isYes(fits(cards) (p,b)) && isYes(fits (cards) (q,b)) then Yes (b) else No

let rec getBid(cards:Hand, sys:BiddingSystem)= //, history: list<Bid>) =
    match sys with
    | [] -> Pass
    | h :: t -> match fits cards (h) with
                | Yes (x) -> x
                | No -> getBid(cards, t)
