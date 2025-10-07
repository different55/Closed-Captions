using System;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
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
        
        harmony = new Harmony(Mod.Info.ModID);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        Patch_ClientMain_StartPlaying.capi = capi;
        
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
        // Unknown condition yoinked without understanding from SubtitlesMod
        var player = capi.World.Player;
        if (player == null) return;
        
        var sound = loadedSound.Params;
        
        // Ignore music
        if (sound.SoundType == EnumSoundType.Music) return;

        var path = location.Path;
        var id = path.StartsWith("sounds/") && path.EndsWith(".ogg") ? path.Substring(7, path.Length - 7 - 4) : path;
        var lastChar = id.ToCharArray(id.Length - 1, 1)[0];
        if (lastChar >= '0' && lastChar <= '9')
            id = id.Substring(0, id.Length - 1);
        
        var name = Lang.GetIfExists("captions:" + id);
        // Ignore specified sounds
        if (name == "") return;
        // Unnamed sounds
        if (name == null) name = id;
        
        if (sound.Position.IsZero)
        {
            captions?.captionsList?.AddSound(name, 0, sound.Volume);
            return;
        }
        
        var playerPos = player.Entity.Pos.AsBlockPos;
        var dx = sound.Position.X - playerPos.X;
        var dy = sound.Position.Y - playerPos.Y;
        var dz = sound.Position.Z - playerPos.Z;
        var dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        
        // Ignore sounds that are out of range.
        if (dist > sound.Range * RANGE_THRESHOLD) return;
        
        // Make close sounds directionless.
        var yaw = (dist < 1.5) ? Math.Atan2(dz, dx) : Double.NaN;
        
        captions?.captionsList?.AddSound(name, yaw, sound.Volume);
    }
}