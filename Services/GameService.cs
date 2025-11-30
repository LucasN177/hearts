using Hearts.Models;
using Hearts.Models.Enums;

namespace Hearts.Services;

public class GameService(PlayService playService)
{
    public int PlayerCount { get; set; }
    public Player PlayerToPlay { get; set; }
    public List<Player> Players { get; set; } = [];
    public Dictionary<string, Player> PlayersDictionary = new Dictionary<string, Player>();
    public List<Card> Cards { get; set; } = new List<Card>();
    public List<Card> ShuffledCards { get; set; } = null!;
    public List<Card> AvailableCards { get; set; } = new List<Card>();
    public int StichCount { get; set; } = 0;
     
    public Stich? CurrentStich { get; set; }

    public List<Stich> Stiche { get; set; } = new List<Stich>();
    
    public async Task EndRound()
    {
        //Todo: Hier Logik für Beenden der Runde!
    }

    public void CalculateStich(Card firstPlayedCard)
    {
        if(CurrentStich == null)
            return;
        var cards = CurrentStich.Cards.Where(x => x.Suit == firstPlayedCard.Suit).ToList();
        if (cards.Count == 0)
        {
            cards.Add(firstPlayedCard);
        }
        cards.Sort((x, y) => x.Id.CompareTo(y.Id));
        var playerToReceive = cards.Last().PlayedBy;
        playerToReceive.Stiche.Add(CurrentStich);
        PlayerToPlay = playerToReceive;
        StichCount++;
        AddPoints(CurrentStich, playerToReceive);
        Stiche.Add(CurrentStich);
        CurrentStich = null;
    }
    
    public async Task<Card> PlayCard(Player player, Difficulty difficulty)
    {
        if (player != Players[1] && player != Players[2] && player != Players[3]) return new Card();
        var card = await playService.PlayerPlay(player, difficulty, CurrentStich);
        player.CurrentCard = card;
        player.Cards.Remove(card);
        return card;
    }

    private void AddPoints(Stich stich, Player playerToReceive)
    {
        var punkte = 0;
        foreach (var card in stich.Cards)
        {
            punkte += card.Punkte;
        }
        playerToReceive.Punkte += punkte;
    }

    #region Init

    public void Init()
    {
        
        var diamonds = GenerateCards(8, "diamonds", 0);
        var hearts = GenerateCards(8, "hearts", 8);
        var spades = GenerateCards(8, "spades", 16);
        var clubs = GenerateCards(8, "clubs", 24);
        
        Cards.AddRange(clubs);
        Cards.AddRange(spades);
        Cards.AddRange(hearts);
        Cards.AddRange(diamonds);

        AvailableCards.AddRange(Cards);
        
        ShuffleCards();
        
        var cardsPlayer1 = Cards.Take(8).ToList();
        foreach (var card in cardsPlayer1)
        {
            Cards.Remove(card);
        }
        
        var cardsPlayer2 = Cards.Take(8).ToList();
        foreach (var card in cardsPlayer2)
        {
            Cards.Remove(card);
        }
        var cardsPlayer3 = Cards.Take(8).ToList();
        foreach (var card in cardsPlayer3)
        {
            Cards.Remove(card);
        }
        
        var cardsPlayer4 = Cards.Take(8).ToList();
        foreach (var card in cardsPlayer4)
        {
            Cards.Remove(card);
        }
        
        Players.Add(new Player(){Name = "You",  Cards = cardsPlayer1});
        Players.Add(new Player(){Name = "Player2",  Cards = cardsPlayer2});
        Players.Add(new Player(){Name = "Player3",  Cards = cardsPlayer3});
        Players.Add(new Player(){Name = "Player4",  Cards = cardsPlayer4});
        
        ChooseBeginningPlayer();
        foreach (var card in cardsPlayer1)
        {
            card.PlayedBy = Players[0];
        }
        foreach (var card in cardsPlayer2)
        {
            card.PlayedBy = Players[1];
        }
        foreach (var card in cardsPlayer3)
        {
            card.PlayedBy = Players[2];
        }
        foreach (var card in cardsPlayer4)
        {
            card.PlayedBy = Players[3];
        }
    }
    
    private static List<Card> GenerateCards(int count, string suit, int startId)
    {
        var values = new[] { "7", "8", "9", "10", "B", "D", "K", "A" };
        var cards = new List<Card>();

        for (int i = 0; i < count; i++)
        {
            var punkte = 0;
            if (suit == "hearts")
            {
                punkte = 1;
            }

            if (suit == "spades" && values[i] == "D")
            {
                punkte = 8;
            }
            cards.Add(new Card
            {
                Id = startId + i,
                Suit = suit,
                Value = values[i % values.Length],
                Punkte = punkte
            });
        }

        return cards;
    }
    
    private void ShuffleCards(){
        var random = new Random();
        for (int n = Cards.Count - 1; n > 0; --n)
        {
            var k = random.Next(n + 1);
            
            (Cards[n], Cards[k]) = (Cards[k], Cards[n]);
        }
    }

    private void ChooseBeginningPlayer()
    {
        var random = new Random();
        PlayerToPlay = Players[random.Next(0,3)];
        PlayerToPlay.IsPlaying = true;
    }

    #endregion
}