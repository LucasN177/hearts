namespace Hearts.Models;

public class Stich
{
    public List<Card> Cards { get; set; } = [];
    public Player WonBy { get; set; } = new ();
}