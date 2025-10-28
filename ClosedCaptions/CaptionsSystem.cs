using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ClosedCaptions;

public class CaptionsSystem : ModSystem
{
    private static CaptionsDialog dialog;
    public static CaptionsConfig config;
    private static ICoreClientAPI api;
    
    public override void StartClientSide(ICoreClientAPI capi)
    {
        base.StartClientSide(capi);
        api = capi;
        LoadConfig();
        
        if (!config.Enabled) return;
        
        api.Event.IsPlayerReady += (ref EnumHandling handling) =>
        {
            Reload();
            return true;
        };
    }
    
    private static void LoadConfig()
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
            api.Logger.Error("Could not load config, using defaults.");
            api.Logger.Error(e.ToString());
            config = new CaptionsConfig();
        }
    }

    public static void Reload()
    {
        dialog?.TryClose();
        LoadConfig();
        if (!config.Enabled) return;
        dialog = new CaptionsDialog(api);
        dialog.TryOpen();
    }
}