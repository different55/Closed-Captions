using System;
using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace ClosedCaptions;

public class Caption
{
    private static double AudibilityThreshold = 0.1;
    private static ICoreClientAPI api;
    private static Queue<ILoadedSound> ActiveSounds;
    public static List<Caption> Captions = [];

    public static void Initialize(ICoreClientAPI api)
    {
        Caption.api = api;
        
        FieldInfo field = api.World.GetType().GetField("ActiveSounds", BindingFlags.NonPublic | BindingFlags.Instance);
        ActiveSounds = (Queue<ILoadedSound>)field.GetValue(api.World);
    }
    
    // Synchronizes the internal caption list with the currently active sounds.
    public static void SyncCaptions()
    {
        // Reset the activeSound count.
        foreach (var caption in Captions)
            caption.activeSounds = 0;

        // Update captions with fresh sound data.
        foreach (var sound in ActiveSounds)
        {
            if (!sound.IsPlaying) continue;
            ProcessSound(sound.Params);
        }
        
        // Prune old captions.
        Captions.RemoveAll(caption => caption.age > CaptionsModSystem.config.Duration);
    }

    private static void ProcessSound(SoundParams sound)
    {
        // Unknown condition yoinked without understanding from SubtitlesMod.
        // Under what conditions would this be null? During startup?
        var player = api.World.Player;
        if (player == null) return;

        // Calculate ID, remove sounds/ prefix, .ogg suffix, and trailing digits.
        var id = sound.Location.Path;
        if (id.StartsWith("sounds/")) id = id[7..];
        if (id.EndsWith(".ogg")) id = id[..^4];
        id = id.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

        var name = Lang.GetIfExists("captions:" + id);
        if (name == null) name = id; // Unnamed sounds just use ID.
        if (name == "")
        {
            return;
        } // Ignore sounds with no name.

        var position = sound.Position;
        var dist = 0.0f;
        // Only calculate distance for sounds that have a position.
        if (!(position == null || position.IsZero))
        {
            dist = sound.Position.DistanceTo(player.Entity.Pos.XYZFloat);
        }

        // Ignore sounds that are out of range.
        if (dist > sound.Range) return;
        
        // Ignore sounds that are out of earshot.
        if ((1 - (dist / sound.Range)) * sound.Volume < AudibilityThreshold) return;
        
        AddSound(name, sound.Position, sound.Volume);
    }

    private static void AddSound(string name, Vec3f position, double volume)
    {
        // Refresh existing slot if it's already present.
        foreach (var caption in Captions)
        {
            if (caption.name != name) continue;

            caption.lastHeard = api.ElapsedMilliseconds;
            caption.activeSounds++;
            caption.position = position;
            caption.volume = volume;
            return;
        }
        
        // No existing caption, add a new one.
        Captions.Add(new Caption
        {
            lastHeard = api.ElapsedMilliseconds,
            name = name,
            position = position,
            volume = volume,
            activeSounds = 1
            
        });
    }
    
    public long lastHeard;
    public double age => (api.ElapsedMilliseconds-lastHeard) / 1000.0; 
    public string name;
    public Vec3f position;
    public double volume;
    public int activeSounds;
}