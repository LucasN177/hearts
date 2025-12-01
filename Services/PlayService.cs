using System.Diagnostics;
using Hearts.Models;
using Hearts.Models.Enums;

namespace Hearts.Services;

public class PlayService
{
    private static readonly Dictionary<string, int> RankMap = new()
    {
        ["7"] = 7,
        ["8"] = 8,
        ["9"] = 9,
        ["10"] = 10,
        ["B"] = 11,
        ["D"] = 12,
        ["K"] = 13,
        ["A"] = 14
    };

// Konfigurierbare Konstante für Shoot-the-Moon-Verhalten, falls du eine feste Umverteilung willst.
// Standardmäßig: wenn ein Spieler in einer Simulation alle Punkte sammelt, bekommen die anderen SCORE_FOR_OTHERS.
    private const int SHOOT_THE_MOON_OTHER_SCORE = 16;
    
    public async Task<Card> PlayerPlay(Player player, Difficulty difficulty, Stich currentStich, List<Card> availableCards, List<Stich> history)
    {
        //Todo: simple algorithm
        if (difficulty == Difficulty.Simple)
        {
            return await PlayCardSimple(player, currentStich);
        }
        //Todo: Ki algorithm
        if(difficulty == Difficulty.Easy)
            return await PlayCardEasy(player, currentStich);
        //Todo: Own algorithm
        if (difficulty == Difficulty.Normal)
            return await PlayCardNormal(player, currentStich, availableCards);
        if (difficulty == Difficulty.Hard)
            return await PlayCardHard(player, currentStich, availableCards, history);
            
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

    private async Task<Card> PlayCardEasy(Player player, Stich currentStich)
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

    private async Task<Card> PlayCardNormal(
    Player player,
    Stich currentStich,
    List<Card> availableCards)
{
    await Task.Yield();

    var hand = player.Cards;
    bool isFirst = currentStich.Cards.Count == 0;

    // Rangfolge der Werte
    Dictionary<string, int> rank = new()
    {
        ["7"] = 7,
        ["8"] = 8,
        ["9"] = 9,
        ["10"] = 10,
        ["B"] = 11,
        ["D"] = 12,
        ["K"] = 13,
        ["A"] = 14
    };

    int GetRank(Card c) => rank[c.Value];

    bool IsHighCard(Card c) => GetRank(c) >= 11;

    bool OpponentsMayShortOn(Suit suit)
        => availableCards.Any(c => c.Suit == suit);

    double EvaluateRisk(Card card, Suit leadSuit, List<Card> trickCards)
    {
        if (card.Suit != leadSuit)
            return 0; // kann Stich nicht gewinnen

        int myRank = GetRank(card);

        int highestInTrick = trickCards
            .Where(c => c.Suit == leadSuit)
            .Select(GetRank)
            .DefaultIfEmpty(0)
            .Max();

        int highestRemaining = availableCards
            .Where(c => c.Suit == leadSuit)
            .Select(GetRank)
            .DefaultIfEmpty(0)
            .Max();

        double risk = 0;

        // ist meine Karte höher als alle im Stich?
        if (myRank > highestInTrick)
            risk += 0.5;

        // ist meine Karte >= höchstem Rest?
        if (myRank >= highestRemaining)
            risk += 0.4;

        // könnten Gegner kurz sein?
        if (OpponentsMayShortOn(leadSuit))
            risk += 0.2;

        return risk;
    }

    Card chosen;

    //
    // 👉 1. Wenn Stich eröffnet wird
    //
    if (isFirst)
    {
        // Suche sichere niedrige Karte, bevorzugt eine Farbe mit vielen remaining cards
        var best = hand
            .GroupBy(c => c.Suit)
            .Select(g => new
            {
                Suit = g.Key,
                LowCard = g.OrderBy(GetRank).First(),
                Remaining = availableCards.Count(c => c.Suit == g.Key)
            })
            .OrderByDescending(x => x.Remaining)
            .ThenBy(x => GetRank(x.LowCard))
            .First();

        chosen = best.LowCard;
    }

    //
    // 👉 2. Farbe bedienen
    //
    else
    {
        var leadSuit = currentStich.Cards.First().Suit;

        var matching = hand.Where(c => c.Suit == leadSuit).ToList();

        if (matching.Any())
        {
            // niedrigstes Risiko suchen
            var rated = matching
                .Select(c => new
                {
                    Card = c,
                    Risk = EvaluateRisk(c, leadSuit, currentStich.Cards)
                })
                .OrderBy(x => x.Risk)
                .ThenBy(x => GetRank(x.Card))
                .ToList();

            chosen = rated.First().Card;
        }
        else
        {
            //
            // 👉 3. Farbe kann nicht bedient werden → abwerfen
            // Risikobasiert: Punktekarten möglichst dann abwerfen, wenn sicher
            //

            bool lastPlayer = currentStich.Cards.Count == 3;

            // Pik-Dame finden (sehr gefährlich)
            Card? queenSpades = hand
                .FirstOrDefault(c => c.Suit == Suit.Spades && c.Value == "D");

            if (queenSpades != null && lastPlayer)
            {
                chosen = queenSpades;
            }
            else
            {
                // Herzen haben Punkte
                var hearts = hand.Where(c => c.Suit == Suit.Hearts).ToList();

                if (hearts.Any() && lastPlayer)
                {
                    // hohe Herzen zuerst loswerden (mehr Punkte)
                    chosen = hearts
                        .OrderByDescending(c => c.Punkte)
                        .ThenByDescending(GetRank)
                        .First();
                }
                else
                {
                    // Unsicherer Moment → neutralste Karte wählen
                    chosen = hand
                        .OrderBy(c =>
                            c.Punkte * 5 +      // Punktelastigkeit erhöhen
                            GetRank(c)          // niedrige Karten bevorzugen
                        )
                        .First();
                }
            }
        }
    }

    // Karte ausspielen
    player.Cards.Remove(chosen);
    player.CurrentCard = chosen;
    currentStich.Cards.Add(chosen);

    return chosen;
}

    
    private async Task<Card> PlayCardHard(
        Player player,
        Stich currentStich,
        List<Card> availableCards,
        List<Stich> stichHistory,
        int monteCarloTimeBudgetMs = 0) // Zeitbudget für Monte-Carlo (in ms), unter 1000ms erlaubt
    {
        // Kurzer async-Punkt – Methode intern arbeitet synchron
        await Task.Yield();

        // Wenn sehr wenig Zeit/Hand -> fallback Heuristik
        // Wir probieren monte carlo zuerst (stark), wenn Zeitbudget > 0
        // 1) sammeln Kandidaten (legale Karten)
        var legalCards = GetLegalPlays(player, currentStich, availableCards, stichHistory);

        // Wenn nur eine Karte -> sofort spielen
        if (legalCards.Count == 1)
        {
            var only = legalCards[0];
            ApplyPlay(player, currentStich, only);
            return only;
        }

        // Heuristik-Bewertungen (schnelle Bewertungs-Grundlage)
        var heuristicScores = legalCards.ToDictionary(
            c => c,
            c => EvaluateCardHeuristicScore(player, currentStich, c, availableCards, stichHistory)
        );

        // Monte-Carlo-Phase: simuliere für jede mögliche Karte (parallelisiert sequenziell hier)
        var bestByMC = await MonteCarloChooseAsync(
            player,
            currentStich,
            legalCards,
            availableCards,
            stichHistory,
            heuristicScores,
            monteCarloTimeBudgetMs
        );

        // Falls MonteCarlo liefert -> wählen, sonst heuristisch besten
        Card chosen = bestByMC ?? heuristicScores.OrderBy(kv => kv.Value).First().Key;

        ApplyPlay(player, currentStich, chosen);
        return chosen;
    }

    #region Helper & Core

private void ApplyPlay(Player player, Stich currentStich, Card chosen)
{
    // Karte aus Hand entfernen und in Stich legen
    player.Cards.RemoveAll(c => c.Id == chosen.Id);
    player.CurrentCard = chosen;
    currentStich.Cards.Add(chosen);
}

// Bestimme legale Züge (unter Beachtung von Farbbedienen, erster Stich Regeln u.ä.)
// Annahme: Hearts-Break-Regel und "kein Punkt im ersten Stich" werden nicht als harte Verbote hier
private List<Card> GetLegalPlays(
    Player player,
    Stich currentStich,
    List<Card> availableCards,
    List<Stich> stichHistory)
{
    var hand = player.Cards;
    bool isFirstCardOfTrick = currentStich.Cards.Count == 0;

    if (isFirstCardOfTrick)
    {
        // Du kannst jede Karte eröffnen; (wenn du exakte zusätzliche Regeln hättest, hier prüfen)
        return new List<Card>(hand);
    }
    else
    {
        var leadSuit = currentStich.Cards.First().Suit;
        var matching = hand.Where(c => c.Suit == leadSuit).ToList();
        if (matching.Any()) return matching;
        return new List<Card>(hand); // Farbe nicht vorhanden -> beliebig abwerfen
    }
}

// Heuristische Bewertung: niedriger = "gute" Karte (avoid points)
private double EvaluateCardHeuristicScore(
    Player player,
    Stich currentStich,
    Card candidate,
    List<Card> availableCards,
    List<Stich> stichHistory)
{
    // Basisfaktoren:
    // - Punkte (Card.Punkte) sind schlecht -> wollen niedrig halten
    // - Wenn Farbe bedient werden kann: Risiko einschätzen -> hohe Ränge riskanter
    // - Wenn man Farbe nicht bedienen kann: opportunistisch Punkte/gefährliche Karten loswerden,
    //   besonders wenn man letzter Spieler dieses Stichs ist (sicherer Abwurf)

    double score = 0.0;
    int rank = RankMap[candidate.Value];

    // Direkte Punkte - vermeiden
    score += candidate.Punkte * 10.0;

    // Wenn man die Farbe bedient (Stich bereits begonnen)
    if (currentStich.Cards.Count > 0)
    {
        var leadSuit = currentStich.Cards.First().Suit;
        if (candidate.Suit == leadSuit)
        {
            // Ist candidate höher als alle bereits im Stich?
            var highestInTrickRank = currentStich.Cards
                .Where(c => c.Suit == leadSuit)
                .Select(c => RankMap[c.Value])
                .DefaultIfEmpty(0)
                .Max();

            if (rank > highestInTrickRank)
            {
                // Risiko, Stich zu gewinnen -> erhöhen
                score += (rank - highestInTrickRank) * 2.0;
            }
            // Berücksichtige, wie viele Karten dieser Farbe noch im Spiel sind (mehr = mehr Unsicherheit)
            int remainingOfSuit = availableCards.Count(c => c.Suit == leadSuit);
            score += Math.Max(0, 10 - remainingOfSuit) * 0.3;
        }
        else
        {
            // Wenn Farbe nicht bedient werden kann (Abwurf)
            // Bevorzuge das Abwerfen von Punktekarten (wenn sicher)
            int cardsInTrick = currentStich.Cards.Count;
            bool lastPlayer = (cardsInTrick == 3);
            if (candidate.Punkte > 0 && lastPlayer)
            {
                score -= candidate.Punkte * 8.0; // sehr positiv (negativ im score => gut)
            }
            else
            {
                // Ungefähr: neutrale Karten bevorzugen
                score += rank * 0.2 + candidate.Punkte * 5.0;
            }
        }
    }
    else
    {
        // Wenn man eröffnet: niedrige Karte, Farbe mit vielen verbleibenden Karten bevorzugen
        int remOfSuit = availableCards.Count(c => c.Suit == candidate.Suit);
        score += rank * 0.3 - remOfSuit * 0.2;
        // Keine Herzen öffnen wenn viele Herzen noch existieren (optional)
        if (candidate.Suit == Suit.Hearts)
            score += 5.0;
    }

    // Zusätzlicher Bias: Pik-Dame sehr gefährlich (großer positiver Score - also vermeiden)
    if (candidate.Suit == Suit.Spades && candidate.Value == "D")
        score += 100.0;

    return score;
}

#endregion

#region MonteCarlo

private async Task<Card?> MonteCarloChooseAsync(
    Player player,
    Stich currentStich,
    List<Card> legalCards,
    List<Card> availableCards,
    List<Stich> stichHistory,
    Dictionary<Card, double> heuristicScores,
    int timeBudgetMs)
{
    if (timeBudgetMs <= 0) return null;

    var sw = Stopwatch.StartNew();
    var rnd = new Random();
    int trials = 0;

    // Monte-Carlo result containers
    var totalPoints = legalCards.ToDictionary(c => c, c => 0.0);
    var countRuns = legalCards.ToDictionary(c => c, c => 0);

    // Build the set of unknown cards to distribute among opponents:
    // availableCards contains all unplayed cards (including player's hand)
    // unknownCards = availableCards - player's hand
    var playerHandIds = new HashSet<int>(player.Cards.Select(c => c.Id));
    var unknownCards = availableCards.Where(c => !playerHandIds.Contains(c.Id)).ToList();

    // quick safety: if unknownCards.Count not divisible by 3 (remaining cards per opponent) it's okay: we'll deal as far as possible
    // Precompute the totalPointsInGame for Shoot-the-Moon detection
    int totalPointPool = availableCards.Sum(c => c.Punkte);

    // Time-limited loop
    while (sw.ElapsedMilliseconds < timeBudgetMs)
    {
        // For each legal starting card, do one playout per trial (or reuse sampled deal)
        // We'll sample a random deal once per trial, then for each candidate simulate the immediate play + remainder.
        var deal = RandomDealOtherHands(unknownCards, rnd);

        // Build simulated players (clone)
        var simPlayers = new List<Player>(4);
        // order of players: we need to know seating. We don't have seating info -> assume player is index 0.
        // Player 0 = current player, 1..3 = opponents
        simPlayers.Add(ClonePlayerWithHand(player, player.Cards)); // our player
        for (int i = 0; i < 3; i++)
        {
            simPlayers.Add(new Player { Name = $"Opp{i+1}", Cards = deal[i].Select(CloneCard).ToList() });
        }

        // Simulate stichHistory and currentStich into simState
        var simTrickHistory = stichHistory.Select(CloneStich).ToList();
        var simCurrentTrick = CloneStich(currentStich); // copy current trick
        // Add the already-played cards (they belong to trick); also remove those from hands if any (shouldn't be)
        // (cards in currentTrick are assumed already removed from hands in real game state)

        // For each candidate card, simulate:
        foreach (var candidate in legalCards)
        {
            // Deep clone players & trick for each simulation to avoid cross contamination
            var simPlayersCopy = simPlayers.Select(ClonePlayer).ToList();
            var simCurrentTrickCopy = CloneStich(simCurrentTrick);
            var simHistoryCopy = simTrickHistory.Select(CloneStich).ToList();

            // Play candidate for player 0
            var candClone = CloneCard(candidate);
            // Remove from player0 hand
            var me = simPlayersCopy[0];
            var found = me.Cards.FirstOrDefault(c => c.Id == candClone.Id);
            if (found != null)
                me.Cards.RemoveAll(c => c.Id == candClone.Id);
            else
            {
                // if not in hand (shouldn't happen) -> try to remove by Value+Suit fallback
                var fallback = me.Cards.FirstOrDefault(c => c.Suit == candClone.Suit && c.Value == candClone.Value);
                if (fallback != null) me.Cards.Remove(fallback);
            }
            simCurrentTrickCopy.Cards.Add(candClone);

            // Determine next player to play: if we assume player0 started trick or not.
            // We assume the current trick's next player is (leadIndex + currentTrickCount) % 4.
            // Because we don't have an absolute seating, assume current player is turn index 0,
            // and the next to play is 1 if currentTrick had zero cards (we played first), else it's
            // (number of cards already in trick) mod 4 (0..3): we interpret positions circularly.
            int leadIndex = 0; // our player is index 0
            // If currentTrick had some cards before (i.e., we are not necessarily the leader), we need to preserve order:
            // For simplicity: if currentTrick originally had cards and our simulation did not start it, we approximate order as:
            int startNextPlayer;
            if (currentStich.Cards.Count == 0)
            {
                // we were lead -> next is player1
                startNextPlayer = 1;
            }
            else
            {
                // currentTrick had N cards before candidate. We must find whose turn is next.
                // We approximate that the first card in currentStich was played by some other player -> so
                // next player index = (indexOfLeader + currentTrickCount) mod 4. We don't know indexOfLeader; assume leader = 0 and current player may be not 0.
                // To keep things simple and robust: after playing candidate we will continue with players 1,2,3 in order, skipping those who already played in the trick.
                startNextPlayer = 1;
            }

            // Now simulate the remainder of the trick and all following tricks until all hands empty.
            double pointsMy = SimulatePlayout(
                simPlayersCopy,
                simCurrentTrickCopy,
                simHistoryCopy,
                startNextPlayer,
                rnd,
                totalPointPool);

            totalPoints[candidate] += pointsMy;
            countRuns[candidate] += 1;
        }

        trials++;
        // allow cancellation if trials large - loop time condition controls termination
    }

    // compute averages
    var averaged = new Dictionary<Card, double>();
    foreach (var c in legalCards)
    {
        if (countRuns[c] > 0)
            averaged[c] = totalPoints[c] / countRuns[c];
        else
            averaged[c] = double.MaxValue;
    }

    // choose min expected points (ties broken by heuristic score)
    var best = averaged.OrderBy(kv => kv.Value)
        .ThenBy(kv => heuristicScores.ContainsKey(kv.Key) ? heuristicScores[kv.Key] : double.MaxValue)
        .FirstOrDefault();

    // If no runs completed, return null
    if (trials == 0) return null;

    return best.Key;
}

// Randomly deal unknownCards among 3 opponents, returning list of 3 lists (for Opp1..Opp3)
private List<List<Card>> RandomDealOtherHands(List<Card> unknownCards, Random rnd)
{
    var cards = unknownCards.Select(CloneCard).ToList();
    // shuffle
    for (int i = cards.Count - 1; i > 0; i--)
    {
        int j = rnd.Next(i + 1);
        var tmp = cards[i];
        cards[i] = cards[j];
        cards[j] = tmp;
    }

    // Deal roughly equally into 3 hands
    var hands = new List<List<Card>> { new(), new(), new() };
    for (int i = 0; i < cards.Count; i++)
    {
        hands[i % 3].Add(CloneCard(cards[i]));
    }
    return hands;
}

// Simulations-Engine: spielt den Rest der Partie zufalls-/heuristikbasiert
// gibt die Anzahl Punkte zurück, die "player index 0" (unsere KI) in der Simulation bekommt
private double SimulatePlayout(
    List<Player> simPlayers, // index 0 = our player; 1-3 opponents
    Stich currentTrick,
    List<Stich> trickHistory,
    int nextPlayerIndex,
    Random rnd,
    int totalPointPool)
{
    // Make local clones (already clones in caller)
    var players = simPlayers;
    var trick = CloneStich(currentTrick);
    var history = trickHistory.Select(CloneStich).ToList();

    // Helper: pick card for a player according to a simple policy (heuristic). Stochastic tie-break.
    Func<int, Stich, Card> pickCardFor = (playerIndex, sTrick) =>
    {
        var p = players[playerIndex];
        var legal = GetLegalPlays(p, sTrick, players.SelectMany(pl => pl.Cards).ToList(), history);
        if (legal.Count == 0)
        {
            // Shouldn't happen
            return CloneCard(p.Cards.First());
        }
        // Evaluate heuristic and pick best (with small randomness)
        var scored = legal.Select(c => new { Card = c, Score = EvaluateCardHeuristicScore(p, sTrick, c, players.SelectMany(pl => pl.Cards).ToList(), history) }).ToList();
        var bestScore = scored.Min(x => x.Score);
        var candidates = scored.Where(x => Math.Abs(x.Score - bestScore) < 1e-6).Select(x => x.Card).ToList();
        // random among best
        return CloneCard(candidates[rnd.Next(candidates.Count)]);
    };

    // Function to play out one trick starting at 'turnIndex' until trick complete (4 cards)
    Action playOneTrick = () =>
    {
        // If some cards are already in trick, continue from nextPlayerIndex
        int turn = nextPlayerIndex % 4;
        // If there are already cards in the trick, skip players who already played
        var alreadyPlayedCount = trick.Cards.Count;
        int played = alreadyPlayedCount;
        // we need to continue in circular order until 4 cards or players have no cards
        while (played < 4)
        {
            // If player's hand empty, skip
            if (players[turn].Cards.Count == 0)
            {
                turn = (turn + 1) % 4;
                continue;
            }
            var cardToPlay = pickCardFor(turn, trick);
            // Remove from hand
            players[turn].Cards.RemoveAll(c => c.Id == cardToPlay.Id);
            // Add to trick
            trick.Cards.Add(CloneCard(cardToPlay));
            played++;
            turn = (turn + 1) % 4;
        }

        // Determine trick winner: highest rank of lead suit
        if (trick.Cards.Count == 0) return;
        var leadSuit = trick.Cards.First().Suit;
        int bestRank = -1;
        int winnerOffset = -1; // 0..3 relative to lead
        for (int i = 0; i < trick.Cards.Count; i++)
        {
            var c = trick.Cards[i];
            if (c.Suit != leadSuit) continue;
            int r = RankMap[c.Value];
            if (r > bestRank)
            {
                bestRank = r;
                winnerOffset = i;
            }
        }
        int winnerIndex = (/*lead player index assumed*/ 0 + winnerOffset) % 4;
        // This simplistic indexing assumes lead was player 0 in the trick; in our simplified simulation we do not keep a full seating mapping.
        // To keep consistent, we instead decide the winner by mapping card owners. We'll attempt to find which player played which card:
        // Try mapping by searching players for card ids (cards are unique)
        int actualWinner = -1;
        for (int pi = 0; pi < players.Count; pi++)
        {
            // If any of the trick cards have been marked as PlayedBy equals the player name we could use it. But CloneCard removed PlayedBy.
            // Alternative: We can approximate winner by checking which player had the highest rank and had played one of these cards in order.
            // Simpler: assign played cards to players in turn order starting from the (nextPlayerIndex at start - number of cards played) mod 4.
        }

        // Simpler robust approach: reconstruct owner order by simulating the play order again:
        // Determine the order in which players played this trick:
        // Find the index of the first player who had capacity to play at start (we used nextPlayerIndex input).
        // But to avoid complex seat tracking, we select winner by scanning trick cards and matching them to which player had them before (difficult).
        // Instead, we'll approximate: assume the play order for trick was players (start) -> (start+1)... and winnerIndex computed above maps to actual player index as:
        int start = nextPlayerIndex % 4;
        int approxWinner = (start + winnerOffset) % 4;
        actualWinner = approxWinner;

        // Assign trick points to winner
        int trickPoints = trick.Cards.Sum(c => c.Punkte);
        // add trick to winner's Stiche (simulate storage)
        players[actualWinner].Stiche.Add(CloneStichWithWinner(trick, players[actualWinner]));

        // Clear trick and set nextPlayerIndex to winner
        trick = new Stich();
        nextPlayerIndex = actualWinner;
    };

    // Play until all players' hands empty
    // A bit simplified: we play tricks while any player has cards
    while (players.Any(p => p.Cards.Count > 0))
    {
        // ensure trick is empty at start of each full trick simulation
        if (trick.Cards.Count > 0)
        {
            // continue trick to completion using current nextPlayerIndex
            playOneTrick();
        }
        else
        {
            // start a new trick: nextPlayerIndex as currently set
            playOneTrick();
        }
    }

    // After simulation complete: compute points of player 0
    int myPoints = players[0].Stiche.Sum(s => s.Cards.Sum(c => c.Punkte));

    // Check for shooting the moon: if some other player collected totalPointPool (all points) => they "shot the moon"
    bool someoneShot = false;
    foreach (var p in players)
    {
        int pts = p.Stiche.Sum(s => s.Cards.Sum(c => c.Punkte));
        if (pts >= totalPointPool)
        {
            // shooter detected -> apply special scoring: for our estimation we treat shooter as making others receive SHOOT_THE_MOON_OTHER_SCORE points
            someoneShot = true;
            if (p == players[0])
            {
                // we shot -> in typical hearts we'd get 0 and others get points, but for our metric we want points we personally will get -> 0
                myPoints = 0;
            }
            else
            {
                // someone else shot -> we/others get SHOOT_THE_MOON_OTHER_SCORE
                myPoints = SHOOT_THE_MOON_OTHER_SCORE;
            }
            break;
        }
    }

    return myPoints;
}

#endregion

#region Cloners

private Player ClonePlayer(Player p)
{
    return new Player
    {
        Name = p.Name,
        Cards = p.Cards.Select(CloneCard).ToList(),
        Stiche = p.Stiche.Select(CloneStich).ToList(),
        CurrentCard = p.CurrentCard != null ? CloneCard(p.CurrentCard) : null,
        IsPlaying = p.IsPlaying,
        Punkte = p.Punkte
    };
}

private Player ClonePlayerWithHand(Player basePlayer, List<Card> hand)
{
    return new Player
    {
        Name = basePlayer.Name,
        Cards = hand.Select(CloneCard).ToList(),
        Stiche = basePlayer.Stiche.Select(CloneStich).ToList(),
        CurrentCard = basePlayer.CurrentCard != null ? CloneCard(basePlayer.CurrentCard) : null,
        IsPlaying = basePlayer.IsPlaying,
        Punkte = basePlayer.Punkte
    };
}

private Card CloneCard(Card c)
{
    return new Card
    {
        Id = c.Id,
        Suit = c.Suit,
        Value = c.Value,
        Punkte = c.Punkte,
        PlayedBy = c.PlayedBy
    };
}

private Stich CloneStich(Stich s)
{
    return new Stich
    {
        Cards = s.Cards.Select(CloneCard).ToList()
        // WonBy will be set when adding to player's Stiche in simulation
    };
}

private Stich CloneStichWithWinner(Stich s, Player winner)
{
    var cs = new Stich
    {
        Cards = s.Cards.Select(CloneCard).ToList(),
        WonBy = winner
    };
    return cs;
}

#endregion
}