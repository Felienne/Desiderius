// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
namespace Desiderius
open Dds.Net

module Enum =
    open System
    let cases<'T when 'T :> Enum> = Enum.GetValues typeof<'T> :?> 'T[]

module Generator =  
    let allRanks = List.ofSeq (Enum.cases<Desi.Rank> |> Seq.cast<Desi.Rank>)
    let allBidSuits = [Desi.Suit.Clubs; Desi.Suit.Hearts; Desi.Suit.Diamonds; Desi.Suit.Spades]

    let cartesian xs ys = 
        xs |> List.collect (fun x -> ys |> List.map (fun y -> Desi.Card(x, y)))

    let rnd = System.Random()

    let takeRandomFrom xs n = let insertAt n xs x = Seq.concat [Seq.take n xs; seq [x]; Seq.skip n xs]
                              let randomInsert xs = insertAt (rnd.Next( (Seq.length xs) + 1 )) xs
                              xs |> Seq.fold randomInsert Seq.empty |> Seq.take n

    let oneRandomHand cardSet = List.ofSeq (takeRandomFrom cardSet 13)

    let rec except a b = 
        match a with 
        | [] -> []
        | h :: t -> if List.exists (fun x -> x = h) b then except t b else h :: except t b


    let randomHandplusRemainder cardSet = 
        let oneHand = oneRandomHand cardSet
        (oneHand , except cardSet oneHand)

    let toStr (r:Desi.Rank) = 
        match r with 
        | Desi.Rank.A -> "A"
        | Desi.Rank.K -> "K"
        | Desi.Rank.Q -> "Q"
        | Desi.Rank.J -> "J"
        | Desi.Rank.T -> "T"
        | Desi.Rank.Nine -> "9"
        | Desi.Rank.Eight -> "8"
        | Desi.Rank.Seven -> "7"
        | Desi.Rank.Six -> "6"
        | Desi.Rank.Five -> "5" 
        | Desi.Rank.Four -> "4"
        | Desi.Rank.Three -> "3"
        | Desi.Rank.Two -> "2"


    //from Player
    let findSuit x = List.filter (helper.getSuit >> (=) x)


    // takes a list of cards and a suit and returns all the ranks as a string 
    let stringForSuit cards s = 
        //get the cards of this suit from the list of cards | get just the ranks | then get the string for each
        findSuit s cards |> List.map helper.getRank |> List.map toStr

    let rec interleave s l = 
        match l with 
        | [] -> []
        | [x] -> [x]
        | h :: t -> h :: s :: interleave s t

    //converts 4 hands into a PBN deal string:
    //"[Deal \"N:.63.AKQ987.A9732 A8654.KQ5.T.QJT6 J973.J98742.3.K4 KQT2.AT.J6542.85\"]";
    let oneHandtoPBN (hand:Desi.Card List) = 
        let sortedbyRank = List.sortBy (helper.getRank) hand |> List.rev
        let allLetters = List.map (stringForSuit sortedbyRank) allBidSuits |> interleave ["."] |>  PBN.flatten 

        List.fold (+) "" allLetters

    let fourHandstoPBN (hands: Desi.Card List List) = 
        List.map oneHandtoPBN hands |> interleave " " |> List.fold (+) ""


    [<EntryPoint>]
    let main argv = 
        
        //make a list with (all) 52 cards
        let theWholeDeck = cartesian allBidSuits allRanks 

        //get one random hand and the rest of the deck
        let (hand1, remainder) = randomHandplusRemainder theWholeDeck

        //shuffle the remainder:
        let remainderRand = takeRandomFrom remainder 39

        //get the other three hands:
        let hand2 = remainderRand |> List.ofSeq |> Seq.skip 0  |> Seq.take 13 |> List.ofSeq
        let hand3 = remainderRand |> List.ofSeq |> Seq.skip 13 |> Seq.take 13 |> List.ofSeq
        let hand4 = remainderRand |> List.ofSeq |> Seq.skip 26 |> Seq.take 13 |> List.ofSeq
        
        let hands = [hand1 ; hand2 ; hand3; hand4]

        let forDDS = "W:" + fourHandstoPBN hands

        let dds = Dds.Net.DdsConnect()

        let results = dds.CalculateMakeableContracts forDDS
     
        printfn "%A" argv
        0 // return an integer exit code
