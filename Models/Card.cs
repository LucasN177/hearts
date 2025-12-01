using Hearts.Models.Enums;

namespace Hearts.Models;

public class Card
{
    public int Id { get; set; } = 0;
    public Suit Suit { get; set; } = Suit.Default;
    public string Value { get; set; } = "";
    public int Punkte { get; set; } = 0;
    
    public Player PlayedBy { get; set; }
}