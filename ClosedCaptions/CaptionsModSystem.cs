using Vintagestory.API.Client;
using Vintagestory.API.Common;

[assembly: ModInfo("Captions")]

namespace ClosedCaptions;

public class CaptionsModSystem : ModSystem
{
    public CaptionsDialog dialog;
    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        
        api.Event.IsPlayerReady += (ref EnumHandling handling) =>
        {
            dialog = new CaptionsDialog(api);
            dialog.TryOpen();
            return true;
        };
    }
}