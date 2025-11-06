using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace ClosedCaptions;

public class Caption
{
    private const double AudibilityThreshold = 0.1;
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
        id = id.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

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
        var audibility = (1 - (dist / sound.Range)) * sound.Volume;
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
            LastHeard = _api.ElapsedMilliseconds,
            Position = sound.Position,
            Audibility = audibility,
        });
    }

    private static void AddCaption(Caption newCaption)
    {
        // Refresh existing slot if it's already present.
        for (var i = 0; i < Captions.Count; i++) //var oldCaption in Captions)
        {
            var oldCaption = Captions[i];
            
            // Only update this caption if it has the same name or if it's on the same channel.
            if (oldCaption.Name != newCaption.Name && oldCaption.Channel != newCaption.Channel)
                continue;
            
            oldCaption.LastHeard = newCaption.LastHeard;

            // If this caption has a higher priority, or the same priority but is louder, replace it.
            if (newCaption.Priority > oldCaption.Priority ||
                (newCaption.Priority == oldCaption.Priority && newCaption.Audibility > oldCaption.Audibility))
            {
                Captions[i] = newCaption;
            }
            
            return;
        }
        
        _api.Logger.Debug("[CAPTION] New caption: " + newCaption.Name + " in channel " + newCaption.Channel + " with tags " + string.Join(", ", newCaption.Tags));
        
        // No existing caption, add a new one.
        Captions.Add(newCaption);
    }
    
    public long LastHeard;
    public double Age => (_api.ElapsedMilliseconds-LastHeard) / 1000.0;
    public string Name;
    public string Channel = null;
    public int Priority = 1;
    public List<string> Tags = [];
    public Vec3f Position;
    public float Audibility;
    
    public bool HasTag(string tag) => Tags?.Contains(tag) ?? false;
}