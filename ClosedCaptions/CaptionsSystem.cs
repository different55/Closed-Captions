using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ClosedCaptions;

public class CaptionsSystem : ModSystem
{
    private static CaptionTracker _tracker;
    private static CaptionsDialog _dialog;
    public static CaptionsConfig Config;
    internal static ICoreClientAPI _api;
    private static AssetCategory Category;

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        Category = new AssetCategory("captions", false, EnumAppSide.Client);
    }
    
    public override void StartClientSide(ICoreClientAPI capi)
    {
        base.StartClientSide(capi);
        _api = capi;
        LoadConfig();
        
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
        _tracker = new CaptionTracker(_api);
        _dialog = new CaptionsDialog(_api, _tracker);
        _dialog.TryOpen();
    }
}