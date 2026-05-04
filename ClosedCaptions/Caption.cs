using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace ClosedCaptions;

public class Caption
{
    private const double AudibilityThreshold = 0.07;
    private static Dictionary<string, LoadedCaptionData> _metadata;
    private static ICoreClientAPI _api;
    private static Queue<ILoadedSound> _activeSounds;
    public static List<Caption> Captions = [];

    public static void Initialize(ICoreClientAPI api)
    {
        _api = api;
        
        ReloadMetadata();
        
        var field = api.World.GetType().GetField("ActiveSounds", BindingFlags.NonPublic | BindingFlags.Instance);
        _activeSounds = (Queue<ILoadedSound>)field?.GetValue(api.World);
    }

    // Reloads caption metadata from assets.
    public static void ReloadMetadata()
    {
        var dataFiles = _api.World.AssetManager.GetMany<Dictionary<string, LoadedCaptionData>>(_api.Logger, "captions/");
        _metadata = new Dictionary<string, LoadedCaptionData>();
        foreach (var dataFile in dataFiles)
            dataFile.Value.ToList().ForEach(i => _metadata[i.Key] = i.Value);
    }

    private static LoadedCaptionData GetData(string id)
    {
        return _metadata.GetValueOrDefault(id) ?? new LoadedCaptionData();
    }
    
    // Synchronizes the internal caption list with the currently active sounds.
    public static void SyncCaptions()
    {
        // Reset priority to keep high priority sounds from getting pinned to the caption
        // beyond the end of their life by lower priority sounds in the same channel.
        // TODO: Let's just stash a copy of every active sound into the caption itself.
        foreach (var caption in Captions)
        {
            caption.Priority = 0;
        }
        
        // Update captions with fresh sound data.
        foreach (var sound in _activeSounds)
        {
            if (!sound.IsPlaying) continue;
            ProcessSound(sound.Params);
        }
        
        // Prune old captions.
        Captions.RemoveAll(caption => caption.Age > CaptionsSystem.Config.Duration);
    }

    private static void ProcessSound(SoundParams sound)
    {
        // Unknown condition yoinked without understanding from SubtitlesMod.
        // Under what conditions would this be null? During startup?
        var player = _api.World.Player;
        if (player == null) return;
        
        // Ignore music
        if (sound.SoundType == EnumSoundType.Music) return;

        // Calculate ID, remove sounds/ prefix, .ogg suffix, and trailing digits.
        var id = sound.Location.Path;
        if (id.StartsWith("sounds/")) id = id[7..];
        if (id.EndsWith(".ogg")) id = id[..^4];
        id = id.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_');

        // Unnamed sounds use ID as fallback.
        var name = Lang.GetIfExists("captions:" + id) ?? id;
        if (name == "") return; // Ignore empty-stringed sounds.

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
        var audibility = (float)Math.Pow((1 - (dist / sound.Range)), 0.5) * sound.Volume;
        if (audibility < AudibilityThreshold) return;
        
        // Ignore configured tags.
        var captionData = GetData(id);
        foreach (var ignoredTag in CaptionsSystem.Config.IgnoredTags)
        {
            if (captionData.Tags?.Contains(ignoredTag) ?? false)
                return;
        }
        
        AddCaption(new Caption
        {
            Name = name,
            Channel = captionData.Channel ?? name,
            Priority = captionData.Priority,
            Tags = captionData.Tags,
            FirstHeard = _api.ElapsedMilliseconds,
            LastHeard = _api.ElapsedMilliseconds,
            Position = sound.Position,
            Audibility = audibility,
            Throttled = CaptionsSystem.Config.ThrottledTags.Any(tag => captionData.Tags?.Contains(tag) ?? false),
        });
    }

    private static void AddCaption(Caption newCaption)
    {
        // Refresh existing slot if it's already present.
        for (var i = 0; i < Captions.Count; i++)
        {
            var oldCaption = Captions[i];
            
            // Only update this caption if it has the same name or if it's on the same channel.
            if (oldCaption.Channel != newCaption.Channel)
                continue;
            
            oldCaption.LastHeard = newCaption.LastHeard;
            newCaption.FirstHeard = oldCaption.FirstHeard;

            // If this caption has a higher priority, or the same priority but is louder, replace it.
            // TODO: With the priority reset hack in SyncCaptions, this is always true.
            if (newCaption.Priority > oldCaption.Priority ||
                (newCaption.Priority == oldCaption.Priority && newCaption.Audibility > oldCaption.Audibility))
            {
                Captions[i] = newCaption;
            }
            
            return;
        }
        
        // No existing caption, add a new one.
        Captions.Add(newCaption);
    }
    
    public long LastHeard;
    public double Age => (_api.ElapsedMilliseconds-LastHeard) / 1000.0;
    public long FirstHeard;
    public double TotalAge => (_api.ElapsedMilliseconds - FirstHeard) / 1000.0;
    public string Name;
    public string Channel = null;
    public int Priority = 1;
    public List<string> Tags = [];
    public Vec3f Position;
    public float Audibility;
    public bool Throttled;
    
    public bool HasTag(string tag) => Tags?.Contains(tag) ?? false;
}