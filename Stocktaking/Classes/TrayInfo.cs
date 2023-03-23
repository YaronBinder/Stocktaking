namespace Stocktaking.Classes;
public class TrayInfo
{
    public TrayInfo(string count, bool isNoTray)
    {
        Count = count;
        IsNoTray = isNoTray;
    }

    public string Count { get; set; }
    public bool IsNoTray { get; set; }
}