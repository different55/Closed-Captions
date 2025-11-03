using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Channel = (string Name, int Priority);

namespace ClosedCaptions;

public class Caption
{
    private const double AudibilityThreshold = 0.1;
    private static CaptionsChannelGuide _channelGuide;
    private static ICoreClientAPI _api;
    private static Queue<ILoadedSound> _activeSounds;
    public static List<Caption> Captions = [];

    public static void Initialize(ICoreClientAPI api)
    {
        _api = api;

        _channelGuide = new CaptionsChannelGuide(api);
        
        var field = api.World.GetType().GetField("ActiveSounds", BindingFlags.NonPublic | BindingFlags.Instance);
        _activeSounds = (Queue<ILoadedSound>)field?.GetValue(api.World);
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
        if (name == "") return; // Ignore empty stringed sounds.

        var alertLevel = AlertLevel.Normal;
        if (name.StartsWith('?')) name = name[1..];
        if (name.StartsWith('!'))
        {
            name = name[1..];
            alertLevel = AlertLevel.Warning;
        }
        if (name.StartsWith('+'))
        {
            name = name[1..];
            alertLevel = AlertLevel.Notice;
        }
        if (name.StartsWith('~'))
        {
            name = name[1..];
            alertLevel = AlertLevel.Environmental;
        }

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


        var channel = _channelGuide.GetChannel(id);
        channel.Name ??= name;
        AddCaption(new Caption
        {
            LastHeard = _api.ElapsedMilliseconds,
            Name = name,
            Channel = channel,
            Position = sound.Position,
            Audibility = audibility,
            AlertLevel = alertLevel
        });
    }

    private static void AddCaption(Caption newCaption)
    {
        // Refresh existing slot if it's already present.
        foreach (var oldCaption in Captions)
        {
            // Only update this caption if it has the same name or if it's on the same channel.
            if (oldCaption.Name != newCaption.Name && oldCaption.Channel.Name != newCaption.Channel.Name) continue;
            oldCaption.LastHeard = newCaption.LastHeard;

            if (newCaption.Channel.Priority < oldCaption.Channel.Priority ||
                (newCaption.Channel.Priority == oldCaption.Channel.Priority && newCaption.Audibility > oldCaption.Audibility))
            {
                oldCaption.Name = newCaption.Name;
                oldCaption.Channel = newCaption.Channel;
                oldCaption.Position = newCaption.Position;
                oldCaption.Audibility = newCaption.Audibility;
                oldCaption.AlertLevel = newCaption.AlertLevel;
            }
            
            return;
        }
        
        // No existing caption, add a new one.
        Captions.Add(newCaption);
    }
    
    public long LastHeard;
    public double Age => (_api.ElapsedMilliseconds-LastHeard) / 1000.0;
    public string Name;
    public Channel Channel;
    public Vec3f Position;
    public float Audibility;
    public AlertLevel AlertLevel = AlertLevel.Normal;
}

public enum AlertLevel
{
    Environmental,
    Normal,
    Notice,
    Warning
}