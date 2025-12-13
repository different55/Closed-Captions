using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ClosedCaptions;

public class CaptionsSystem : ModSystem
{
    private static CaptionsDialog _dialog;
    private static SettingsDialog _settings;
    public static CaptionsConfig Config;
    private static ICoreClientAPI _api;
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
        
        Caption.Initialize(_api);
        
        // Initial load of overlay
        if (Config.Enabled)
        {
             _dialog = new CaptionsDialog(_api);
             _dialog.TryOpen();
        }

        // Register Hotkey for Settings
        _api.Input.RegisterHotKey("captionssettings", "Open Captions Settings", GlKeys.C, HotkeyType.GUIOrOtherControls, ctrlPressed: true, shiftPressed: false, altPressed: false);
        _api.Input.SetHotKeyHandler("captionssettings", OnToggleSettings);

        _api.Event.IsPlayerReady += (ref EnumHandling _) =>
        {
            // Maybe ensure dialog is open if enabled
            if (Config.Enabled && (_dialog == null || !_dialog.IsOpened()))
            {
                 Reload();
            }
            return true;
        };
    }

    private static bool OnToggleSettings(KeyCombination comb)
    {
        if (_settings != null && _settings.IsOpened())
        {
            _settings.TryClose();
            _settings = null; // Dispose reference
        }
        else
        {
            _settings = new SettingsDialog(_api);
            _settings.TryOpen();
        }
        return true;
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
        // Reloads the Captions Overlay (and Config)
        // Does NOT auto-open SettingsDialog
        
        _dialog?.TryClose();
        
        LoadConfig();
        
        if (Config.Enabled)
        {
            _dialog = new CaptionsDialog(_api);
            _dialog.TryOpen();
        }
    }
}