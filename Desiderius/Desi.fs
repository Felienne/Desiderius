// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

type Dealer = West | South | North | East

type Value = A | K | Q | J | T | Nine | Eight | Seven | Six | Five | Four | Three | Two

type Suit = Spades | Hearts | Diamonds | Clubs   

type Card = Suit * Value

type Hand = list<Card>

type Deal = Deal of Dealer * list<Hand>

let getDealer (input:string) : Dealer = 
    match input.Chars(7) with
    |'W' -> West
    |'E' -> East
    |'N' -> North
    |'S' -> South


let getDeal (input:string) : Deal =
    Deal (getDealer input, list.Empty) 

let getStringBetween(input:string, firstSub:string, secondSub:string): string = 
    let firstSubLocation = input.IndexOf(firstSub);
    let lastSubLocation = input.IndexOf(secondSub, firstSubLocation+1)
    input.Substring(firstSubLocation+1,lastSubLocation-firstSubLocation-1)

let getHandsString(input):string=
    getStringBetween(input, ":","\"")

let getHandList(input:string): List<string> = 
    let hands = getHandsString(input)
    List.ofArray(hands.Split(' '))
   


    