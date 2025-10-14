using System;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;
using HarmonyLib;

[assembly: ModInfo("Captions")]

namespace ClosedCaptions;

public class CaptionsModSystem : ModSystem
{
    private Harmony harmony;
    
    public override void StartClientSide(ICoreClientAPI capi)
    {
        base.StartClientSide(capi);
        
        Patch_ClientMain_StartPlaying.capi = capi;
        harmony = new Harmony(Mod.Info.ModID);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        
        capi.Event.IsPlayerReady += (ref EnumHandling handling) =>
        {
            Patch_ClientMain_StartPlaying.captions = new CaptionsDialog(capi);
            Patch_ClientMain_StartPlaying.captions.TryOpen();
            return true;
        };
    }
}

[HarmonyPatch(typeof(ClientMain))]
[HarmonyPatch("StartPlaying")]
[HarmonyPatch(new Type[] { typeof(ILoadedSound), typeof(AssetLocation) })]
public class Patch_ClientMain_StartPlaying
{
    public static ICoreClientAPI capi;
    public static CaptionsDialog captions;
    public static readonly double RANGE_THRESHOLD = 0.8;

    public static void Prefix(ILoadedSound loadedSound, AssetLocation location)
    {
        captions?.captionsList?.ProcessSound(loadedSound.Params);
    }
}