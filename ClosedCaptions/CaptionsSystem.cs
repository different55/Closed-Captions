using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ClosedCaptions;

public class CaptionsSystem : ModSystem
{
    private static CaptionsDialog _dialog;
    public static CaptionsConfig Config;
    private static ICoreClientAPI _api;
    
    public override void StartClientSide(ICoreClientAPI capi)
    {
        base.StartClientSide(capi);
        _api = capi;
        LoadConfig();
        
        Caption.Initialize(_api);
        
        if (!Config.Enabled) return;
         
        _api.Event.IsPlayerReady += (ref EnumHandling _) =>
        {
            Reload();
            return true;
        };
    }
    
    private static void LoadConfig()
    {
        try
        {
            Config = _api.LoadModConfig<CaptionsConfig>("captions.json");
            if (Config != null) return;
            Config = new CaptionsConfig();
            _api.StoreModConfig(Config, "captions.json");
        }
        catch (Exception e)
        {
            _api.Logger.Error("Could not load config, using defaults.");
            _api.Logger.Error(e.ToString());
            Config = new CaptionsConfig();
        }
    }

    public static void Reload()
    {
        _dialog?.TryClose();
        LoadConfig();
        if (!Config.Enabled) return;
        _dialog = new CaptionsDialog(_api);
        _dialog.TryOpen();
    }
}