using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ClosedCaptions;

public class CaptionsModSystem : ModSystem
{
    public CaptionsDialog dialog;
    public static CaptionsConfig config;
    
    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        LoadConfig(api);
        
        if (!config.Enabled) return;
        
        api.Event.IsPlayerReady += (ref EnumHandling handling) =>
        {
            dialog = new CaptionsDialog(api);
            dialog.TryOpen();
            return true;
        };
    }
    
    private void LoadConfig(ICoreAPI api)
    {
        try
        {
            config = api.LoadModConfig<CaptionsConfig>("captions.json");
            if (config == null)
            {
                config = new CaptionsConfig();
                api.StoreModConfig<CaptionsConfig>(config, "captions.json");
            }
        }
        catch (Exception e)
        {
            Mod.Logger.Error("Could not load config, using defaults.");
            Mod.Logger.Error(e.ToString());
            config = new CaptionsConfig();
        }
    }
}