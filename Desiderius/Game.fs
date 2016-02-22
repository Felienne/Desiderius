namespace Desiderius 

module helper = 

   let getSuit (c: Desi.Card) : Desi.Suit =
      match c with 
      | Desi.Card (s,r) -> s

   let getRank (c: Desi.Card) : Desi.Rank =
      match c with 
      | Desi.Card (s,r) -> r

module Player =

   type T = {Direction : Desi.Direction; Hand : Desi.Hand}

   let create d h = 
      {Direction = d; Hand = h}

   //this method describes the actual playing strategy
   //work in progress, first step is to just not break any rules
   //I know this strategy is sort of what a monkey would play
   let nextCard {Direction = direction; Hand = h} (history : List<List<Desi.Card>>) (trump: Desi.Suit) : Desi.Card = 
      match h with
      | Desi.Hand hand ->

         let orderedCards = List.sortBy (fun x-> helper.getRank(x)) hand |> List.rev
      
         let thisTrick = List.nth history 0

         match thisTrick with
            | [] -> List.nth orderedCards 0 //nothing the in history yet, we are the starting player, and we play our highest card.
            | openCard :: t -> 

            //first: can we follow suit?
            let openSuit = openCard |> helper.getSuit
            
            let allOpenSuitinOrder = List.filter (fun x -> helper.getSuit(x) = openSuit) thisTrick
            let openRank = openCard |> helper.getRank

            match allOpenSuitinOrder with
               | h::t -> if helper.getRank(h) > openRank then h else List.nth (allOpenSuitinOrder|> List.rev) 0
               | [] -> //no can do? Let's check trumps!

               let allTrumpsinOrder = hand |> List.filter (fun x-> helper.getSuit(x) = trump)
      
               //if we have a trump, we will play it
               match allTrumpsinOrder with
                  | h::t -> h
                  | [] -> List.nth orderedCards 0 //otherwise we will play the highest card we have




module Deal = 
   type T = {Players: Player.T List}
   let create players = {Players = players}

module ContractGame = 

   type T = {Trump : Desi.Suit; Players: Player.T List}

   let emptyHistory: List<List<Desi.Card>> = List.empty

   let emptyPlayers : Player.T List = List.empty

   let create trump players = 
      {Trump = trump; Players = players}



   let winningCard {Trump=trump} (trick: List<Desi.Card>):Desi.Card =
      
      let allTrumps = trick |> List.filter (fun x-> helper.getSuit(x) = trump) 
      let allTrumpsinOrder = List.sortBy (fun x-> helper.getRank(x)) allTrumps |> List.rev
         
      match allTrumpsinOrder with
         | h::t -> h
         | [] -> 

         let openSuit = List.nth trick 0 |> helper.getSuit
         let allOpenSuit = List.filter (fun x -> helper.getSuit(x) = openSuit) trick

         let allOpenSuitinOrder = List.sortBy (fun x-> helper.getRank(x)) allOpenSuit |> List.rev

         List.nth allOpenSuitinOrder 0

