using Hearts.Models.Enums;

namespace Hearts.Services;

public class SettingService(GameService gameService)
{
    public Difficulty Player2Difficulty { get; set; } = Difficulty.Easy;
    public Difficulty Player3Difficulty { get; set; } = Difficulty.Easy;
    public Difficulty Player4Difficulty { get; set; } = Difficulty.Easy;
    
    public bool ComponentCardsVisibility {get; set;} = false;
}