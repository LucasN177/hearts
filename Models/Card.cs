namespace Hearts.Models;

public class Card
{
    public int Id { get; set; } = 0;
    public string Suit { get; set; } = "";
    public string Value { get; set; } = "";
    public int Punkte { get; set; } = 0;
    
    public Player PlayedBy { get; set; }
}