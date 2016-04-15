namespace Desiderius

module helper = 
  let getSuit (c : Desi.Card) : Desi.Suit = 
    match c with
    | Desi.Card(s, r) -> s
  
  let getRank (c : Desi.Card) : Desi.Rank = 
    match c with
    | Desi.Card(s, r) -> r

module Player = 
  type T = 
    { Direction : Desi.Direction
      Hand : Desi.Hand }
  
  let sortCardsbyRank = List.sortBy helper.getRank >> List.rev
  let findSuit x = List.filter (helper.getSuit >> (=) x)
  
  let create d h = 
    { Direction = d
      Hand = h }
  
  //this method describes the actual playing strategy
  //work in progress, first step is to just not break any rules
  //I know this strategy is sort of what a monkey would play
  let nextCard { Direction = direction; Hand = h } (history : List<List<Desi.Card>>) (trump : Desi.Suit) : Desi.Card = 
    match h, history with
    | Desi.Hand hand, [] :: _ -> 
      let orderedCards = sortCardsbyRank hand
      List.nth orderedCards 0
    | Desi.Hand hand, thisTrick :: _ -> 
      match thisTrick with
      | openCard :: t -> 
        let orderedCards = sortCardsbyRank hand
        //first: can we follow suit?
        let openSuit = openCard |> helper.getSuit
        let openRank = openCard |> helper.getRank
        
        let allOpenSuitinOrder = 
          orderedCards
          |> findSuit openSuit
          |> sortCardsbyRank
        match allOpenSuitinOrder with
        | h :: t -> 
          if helper.getRank h > openRank then h
          else List.nth (allOpenSuitinOrder |> List.rev) 0
        | [] -> //no can do? Let's check trumps!
          let allTrumpsinOrder = hand |> findSuit trump
          //if we have a trump, we will play it
          match allTrumpsinOrder with
          | h :: t -> h
          | [] -> List.nth orderedCards 0 //otherwise we will play the highest card we have

module Deal = 
  type T = 
    { Players : Player.T List }
  
  let create players = { Players = players }

module ContractGame = 
  type T = 
    { Trump : Desi.Suit
      Players : Player.T List }
  
  let emptyHistory : List<List<Desi.Card>> = [ [] ]
  let emptyPlayers : Player.T List = []
  
  let create trump players = 
    { Trump = trump
      Players = players }
  
  let rec addToHistory (history : List<List<Desi.Card>>) (card : Desi.Card) = 
    match history with
    | h :: t when List.length h = 0 -> //if this trick is empty we add a card
      [ card ] :: t
    | h :: t when List.length h = 3 -> //add the last card and start a new one*
      [] :: (card :: h) :: t
    | h :: t -> //in all other cases, get the trick and add to it
      (card :: h) :: t
  
  //* this is used to make the play logiv simpler, we can now always get the
  //head of history and thus obtain the latest trick ((8) on every street (8))
  let winningCard { Trump = trump } (trick : List<Desi.Card>) : Desi.Card = 
    let allTrumps = trick |> List.filter (fun x -> helper.getSuit (x) = trump)
    let allTrumpsinOrder = List.sortBy (fun x -> helper.getRank (x)) allTrumps |> List.rev
    match allTrumpsinOrder with
    | h :: t -> h
    | [] -> 
      let openSuit = List.nth trick 0 |> helper.getSuit
      let allOpenSuit = List.filter (fun x -> helper.getSuit (x) = openSuit) trick
      let allOpenSuitinOrder = List.sortBy (fun x -> helper.getRank (x)) allOpenSuit |> List.rev
      List.nth allOpenSuitinOrder 0
