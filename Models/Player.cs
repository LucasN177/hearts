namespace Hearts.Models;

public class Player
{
    public string Name { get; set; } = "";
    public List<Card> Cards { get; set; } = new ();

    public List<Stich> Stiche { get; set; } = [];
    
    public Card? CurrentCard { get; set; }
    
    public bool IsPlaying { get; set; }
    
    public int Punkte { get; set; } = 0;
}