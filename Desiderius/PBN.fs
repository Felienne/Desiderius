namespace Desiderius

module PBN =

   let getDealer (s:string) = 
      let seventElement = s.Chars(7)
      match seventElement with
         | 'N' -> Desi.Dealer Desi.Direction.North
         | 'E' -> Desi.Dealer Desi.Direction.East
         | 'S' -> Desi.Dealer Desi.Direction.South
         | 'W' -> Desi.Dealer Desi.Direction.West

   let getElem i L = 
      List.nth L i
   
   let getHandsString (s:string) = 
      let afterColon = s.Split ':' |> Array.toList |> getElem 1
      let afterColonString = afterColon.ToString()
      let secondPart = afterColonString.Split '\"' |> Array.toList |> List.head
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


   let toHandfromOneSuit (suit : Desi.Suit, s:string): Desi.Card List = 
      s.ToCharArray() |> Array.toList |> List.map rankFromChar |> List.map (makeCardFromSuitandRank suit)

   let flatten list = List.collect(fun item -> item) list

   let toHand (oneHand:string):Desi.Card List = 
      match oneHand with
         | "-" ->  List.Empty
         | _ -> 
               let suitList = oneHand.Split '.' |> Array.toList                 
               let suits = [Desi.Suit.Spades; Desi.Suit.Hearts ; Desi.Suit.Diamonds ; Desi.Suit.Clubs]

               flatten (List.map toHandfromOneSuit (List.zip suits suitList))
                                         


   let validDeal (fourHands: Desi.Hand List):bool = 
      let cards = List.map Desi.cardsinHand fourHands
 
      List.forall (fun x -> List.length x = 13) cards
      //we probably want to loosen this to allow empty hands too (0 \/ 13) 
      //and we could also add that cards have to be unique

   let getHandList (s:string):Desi.Hand List = 
      let hands = (getHandsString s).Split ' ' |> Array.toList
      let makeHands = List.map toHand hands
     
      List.map Desi.Hand makeHands
      

