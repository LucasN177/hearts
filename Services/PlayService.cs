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
            if (cards.Last().Suit != "clubs")
                return cards.First();
            return cards.Last();
        }
        
        //Spieler bedient
        if (playedCards[0].Suit == "clubs")
        {
            var playableCards = cards.Where(x => x.Suit == "clubs");
            if(playableCards.Count() > 0)
                return playableCards.Last();
            return cards.Where(x => x.Suit == "spades" && x.Value == "D").Count() > 0 ? cards.Where(x => x.Suit == "spades" && x.Value == "D").FirstOrDefault() : cards.Last();
        }
        
        if (playedCards[0].Suit == "spades")
        {
            var playableCards = cards.Where(x => x.Suit == "spades");
            if(playableCards.Count() > 0)
                return playableCards.First();
            return cards.Where(x => x.Suit == "spades" && x.Value == "D").Count() > 0 ? cards.Where(x => x.Suit == "spades" && x.Value == "D").FirstOrDefault() : cards.Last();
        }
        
        if (playedCards[0].Suit == "hearts")
        {
            var playableCards = cards.Where(x => x.Suit == "hearts");
            if(playableCards.Count() > 0)
                return playableCards.First();
            return cards.Where(x => x.Suit == "spades" && x.Value == "D").Count() > 0 ? cards.Where(x => x.Suit == "spades" && x.Value == "D").FirstOrDefault() : cards.Last();
        }
        
        if (playedCards[0].Suit == "diamonds")
        {
            var playableCards = cards.Where(x => x.Suit == "diamonds");
            if(playableCards.Count() > 0)
                return playableCards.Last();
            if (cards.Where(x => x.Suit == "spades" && x.Value == "D").Count() > 0)
                return cards.Where(x => x.Suit == "spades" && x.Value == "D").FirstOrDefault();
            return cards.Last();

        }
        
        player.CurrentCard = player.Cards[0];
        return player.Cards[0];
    }
    
}