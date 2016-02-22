namespace Desiderius

module PBN =

   let getDealer (s:string) = 
      let seventElement = s.Chars(7)
      match seventElement with
         | 'N' -> Desi.Dealer Desi.Direction.North
         | 'E' -> Desi.Dealer Desi.Direction.East
         | 'S' -> Desi.Dealer Desi.Direction.South
         | 'W' -> Desi.Dealer Desi.Direction.West
   
   let getHandsString (s:string) = 
      let firstPart = s.Split ':' |> Array.toList |> List.rev |> List.head //I want the first element of the list, but List.nth does not chain as the list if the first argument.
      let firstString = firstPart.ToString()
      let secondPart = firstString.Split '\"' |> Array.toList |> List.head
      secondPart.ToString()


   let makeCardFromSuitandRank (s: Desi.Suit) (r: Desi.Rank) =
      Desi.Card (s,r)

   let rankFromChar s = 
      match s with
         | 'A' -> Desi.Rank.A
         | 'K' -> Desi.Rank.K
         | 'Q' -> Desi.Rank.Q
         | 'J' -> Desi.Rank.J
         | 'T' -> Desi.Rank.T
         | '9' -> Desi.Rank.Nine
         | '8' -> Desi.Rank.Eight
         | '7' -> Desi.Rank.Seven
         | '6' -> Desi.Rank.Six
         | '5' -> Desi.Rank.Five
         | '4' -> Desi.Rank.Four
         | '3' -> Desi.Rank.Three
         | '2' -> Desi.Rank.Two


   let toHandfromOneSuit (suit : Desi.Suit) (s:string): Desi.Card List = 
      s.ToCharArray() |> Array.toList |> List.map rankFromChar |> List.map (makeCardFromSuitandRank suit)


   let getElem i L = 
      List.nth L i


   let toHand (oneHand:string):Desi.Card List = 
      match oneHand with
         | "-" ->  List.Empty
         | _ -> let suitList = oneHand.Split '.' |> Array.toList
                let suitOne = getElem 0 suitList |> (toHandfromOneSuit Desi.Suit.Spades)
                let suitTwo = getElem 1 suitList |> (toHandfromOneSuit Desi.Suit.Hearts)
                let suitThree = getElem 2 suitList |> (toHandfromOneSuit Desi.Suit.Diamonds)
                let suitFour = getElem 3 suitList |> (toHandfromOneSuit Desi.Suit.Clubs)
                List.append suitOne (List.append suitTwo (List.append suitThree suitFour))

   let getHandList (s:string):Desi.Hand List = 
      let hands = getHandsString s
      let firstHand = hands.Split ' ' |> Array.toList |> getElem 0 |> toHand
      let secondHand = hands.Split ' ' |> Array.toList |> getElem 1 |> toHand
      let thirdHand = hands.Split ' ' |> Array.toList |> getElem 2 |> toHand
      let fourthHand = hands.Split ' ' |> Array.toList |> getElem 3 |> toHand
           
      [ Desi.Hand (firstHand) 
        Desi.Hand (secondHand) 
        Desi.Hand (thirdHand) 
        Desi.Hand (fourthHand) ]


