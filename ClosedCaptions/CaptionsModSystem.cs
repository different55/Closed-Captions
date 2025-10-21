using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ClosedCaptions;

public class CaptionsModSystem : ModSystem
{
    public CaptionsDialog dialog;
    public static CaptionsConfig config;
    private ICoreClientAPI api;
    
    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        this.api = api;
        LoadConfig();
        
        if (!config.Enabled) return;
        
        api.Event.IsPlayerReady += (ref EnumHandling handling) =>
        {
            Reload();
            return true;
        };
    }
    
    private void LoadConfig()
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

    public void Reload()
    {
        dialog?.TryClose();
        dialog = new CaptionsDialog(api);
        dialog.TryOpen();
    }
}