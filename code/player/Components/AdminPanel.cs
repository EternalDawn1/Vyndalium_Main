namespace GeneralGame;
public class AdminPanel : Component
{
    public bool Kick { get; set; }
    public bool Ban { get; set; }
    public bool Mute { get; set; }
    public bool Unmute { get; set; }
    public bool Freeze { get; set; }
    public bool Unfreeze { get; set; }
    public bool Godmode { get; set; }
    public bool Ungodmode { get; set; }
    public ulong SteamID { get; set; }
    public string Reason { get; set; }
    public string Duration { get; set; }
    public string MuteReason { get; set; }
    public string MuteDuration { get; set; }
    public string KickReason { get; set; }

    protected override void OnStart()
    {
        
    }
   
    
    
   

}