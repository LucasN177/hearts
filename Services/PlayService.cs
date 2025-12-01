using Hearts.Models;
using Hearts.Models.Enums;

namespace Hearts.Services;

public class PlayService
{
    public async Task<Card> PlayerPlay(Player player, Difficulty difficulty, Stich currentStich)
    {
        //Todo: simple algorithm
        if (difficulty == Difficulty.Simple)
        {
            return await PlayCardSimple(player, currentStich);
        }
        //Todo: Ki algorithm
        if(difficulty == Difficulty.Normal)
            return await PlayCardNormal(player, currentStich);
        //Todo: Own algorithm
        return await PlayCardSimple(player, currentStich);
    }

    private async Task<Card> PlayCardSimple(Player player, Stich currentStich)
    {
        var cards = player.Cards;
        var playedCards = currentStich.Cards;
        cards.Sort((x, y) => x.Id.CompareTo(y.Id));
        
        //Spieler spielt aus
        if (playedCards.Count == 0)
        {
            cards.Sort((x, y) => x.Id.CompareTo(y.Id));
            if (cards.Last().Suit != Suit.Clubs)
                return cards.First();
            return cards.Last();
        }
        
        //Spieler bedient
        if (playedCards[0].Suit == Suit.Clubs)
        {
            var playableCards = cards.Where(x => x.Suit == Suit.Clubs);
            if(playableCards.Count() > 0)
                return playableCards.Last();
            return cards.Where(x => x.Suit == Suit.Spades && x.Value == "D").Count() > 0 ? cards.Where(x => x.Suit == Suit.Spades && x.Value == "D").FirstOrDefault() : cards.Last();
        }
        
        if (playedCards[0].Suit == Suit.Spades)
        {
            var playableCards = cards.Where(x => x.Suit == Suit.Spades);
            if(playableCards.Count() > 0)
                return playableCards.First();
            return cards.Where(x => x.Suit == Suit.Spades && x.Value == "D").Count() > 0 ? cards.Where(x => x.Suit == Suit.Spades && x.Value == "D").FirstOrDefault() : cards.Last();
        }
        
        if (playedCards[0].Suit == Suit.Hearts)
        {
            var playableCards = cards.Where(x => x.Suit == Suit.Hearts);
            if(playableCards.Count() > 0)
                return playableCards.First();
            return cards.Where(x => x.Suit == Suit.Spades && x.Value == "D").Count() > 0 ? cards.Where(x => x.Suit == Suit.Spades && x.Value == "D").FirstOrDefault() : cards.Last();
        }
        
        if (playedCards[0].Suit == Suit.Diamonds)
        {
            var playableCards = cards.Where(x => x.Suit == Suit.Diamonds);
            if(playableCards.Count() > 0)
                return playableCards.Last();
            if (cards.Where(x => x.Suit == Suit.Spades && x.Value == "D").Count() > 0)
                return cards.Where(x => x.Suit == Suit.Spades && x.Value == "D").FirstOrDefault();
            return cards.Last();

        }
        
        player.CurrentCard = player.Cards[0];
        return player.Cards[0];
    }

    private async Task<Card> PlayCardNormal(Player player, Stich currentStich)
{
    // Zur Vereinfachung synchron, aber async möglich
    await Task.Yield();

    var hand = player.Cards;

    bool isFirst = currentStich.Cards.Count == 0;

    Card chosen;

    if (isFirst)
    {
        // Herz ist verboten, wenn noch kein Herz "gebrochen" wurde:
        bool heartsBroken = player.Stiche.Any(s => s.Cards.Any(c => c.Suit == Suit.Hearts));

        // Versuche eine Nicht-Herz-Karte zu spielen
        var nonHearts = hand.Where(c => c.Suit != Suit.Hearts).ToList();

        if (nonHearts.Any())
        {
            chosen = nonHearts.OrderBy(c => c.Value).First();
        }
        else
        {
            // Nur Herzen übrig → niedrigste Herz-Karte
            chosen = hand.Where(c => c.Suit == Suit.Hearts)
                         .OrderBy(c => c.Value)
                         .First();
        }
    }
    else
    {
        // Farbe der ersten Karte im Stich
        var leadSuit = currentStich.Cards.First().Suit;

        // Karten, die Farbe bedienen können
        var matching = hand.Where(c => c.Suit == leadSuit).ToList();

        if (matching.Any())
        {
            // Niedrigste Karte der passenden Farbe spielen
            chosen = matching.OrderBy(c => c.Value).First();
        }
        else
        {
            // Farbe kann nicht bedient werden → abwerfen
            // Priorität: hohe Strafen loswerden (Pik Dame, hohe Herzen)
            var queenSpades = hand.Where(c => c.Suit == Suit.Spades && c.Value == "D").ToList();
            if (queenSpades.Any())
                chosen = queenSpades.First();
            else
            {
                var hearts = hand.Where(c => c.Suit == Suit.Hearts).OrderByDescending(c => c.Value).ToList();
                if (hearts.Any())
                    chosen = hearts.First();
                else
                {
                    // sonst höchste Karte insgesamt abwerfen
                    chosen = hand.OrderByDescending(c => c.Value).First();
                }
            }
        }
    }

    // Karte aus Hand nehmen
    player.Cards.Remove(chosen);
    player.CurrentCard = chosen;
    currentStich.Cards.Add(chosen);

    return chosen;
}

    
}