using System.Collections.Generic;

public class ResultPanelData : BasePanelData
{
    public int CountDown;
    public bool IsWin;
    public List<Ship> VictoriousPlayers = new List<Ship>();
}