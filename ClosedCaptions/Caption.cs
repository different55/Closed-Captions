using System;
using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Channel = (string Name, int Priority);

namespace ClosedCaptions;

public class Caption
{
    private static double AudibilityThreshold = 0.1;
    private static CaptionsChannelGuide _channelGuide;
    private static ICoreClientAPI api;
    private static Queue<ILoadedSound> ActiveSounds;
    public static List<Caption> Captions = [];

    public static void Initialize(ICoreClientAPI api)
    {
        Caption.api = api;

        _channelGuide = new CaptionsChannelGuide(api);
        
        FieldInfo field = api.World.GetType().GetField("ActiveSounds", BindingFlags.NonPublic | BindingFlags.Instance);
        ActiveSounds = (Queue<ILoadedSound>)field.GetValue(api.World);
    }
    
    // Synchronizes the internal caption list with the currently active sounds.
    public static void SyncCaptions()
    {
        // Update captions with fresh sound data.
        foreach (var sound in ActiveSounds)
        {
            if (!sound.IsPlaying) continue;
            ProcessSound(sound.Params);
        }
        
        // Prune old captions.
        Captions.RemoveAll(caption => caption.age > CaptionsSystem.config.Duration);
    }

    private static void ProcessSound(SoundParams sound)
    {
        // Unknown condition yoinked without understanding from SubtitlesMod.
        // Under what conditions would this be null? During startup?
        var player = api.World.Player;
        if (player == null) return;
        
        // Ignore music
        if (sound.SoundType == EnumSoundType.Music) return;

        // Calculate ID, remove sounds/ prefix, .ogg suffix, and trailing digits.
        var id = sound.Location.Path;
        if (id.StartsWith("sounds/")) id = id[7..];
        if (id.EndsWith(".ogg")) id = id[..^4];
        id = id.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

        var name = Lang.GetIfExists("captions:" + id);
        if (name == null) name = id; // Unnamed sounds use ID as fallback.
        if (name == "") return; // Ignore empty stringed sounds.

        var urgency = AlertLevel.Normal;
        if (name.StartsWith('?')) name = name[1..];
        if (name.StartsWith('!'))
        {
            name = name[1..];
            urgency = AlertLevel.Warning;
        }
        if (name.StartsWith('+'))
        {
            name = name[1..];
            urgency = AlertLevel.Notice;
        }
        if (name.StartsWith('~'))
        {
            name = name[1..];
            urgency = AlertLevel.Environmental;
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
        
        AddSound(name, sound, audibility, _channelGuide.GetChannel(id), urgency);
    }

    private static void AddSound(string name, SoundParams sound, float audibility, Channel channel, AlertLevel urgency)
    {
        // Substitute audio description for a channel name.
        if (channel.Name == null) channel.Name = name;
        
        // Refresh existing slot if it's already present.
        foreach (var caption in Captions)
        {
            // We only want to update this caption if it has the same name or if it's on the same channel.
            if (caption.name != name && caption.channel.Name != channel.Name) continue;
            caption.lastHeard = api.ElapsedMilliseconds;

            if (channel.Priority < caption.channel.Priority ||
                (channel.Priority == caption.channel.Priority && audibility > caption.audibility))
            {
                caption.name = name;
                caption.channel = channel;
                caption.position = sound.Position;
                caption.audibility = audibility;
                caption.urgency = urgency;
            }
            
            return;
        }
        
        // No existing caption, add a new one.
        Captions.Add(new Caption
        {
            lastHeard = api.ElapsedMilliseconds,
            name = name,
            channel = channel,
            position = sound.Position,
            audibility = audibility,
            urgency = urgency
        });
    }
    
    public long lastHeard;
    public double age => (api.ElapsedMilliseconds-lastHeard) / 1000.0;
    public string name;
    public Channel channel;
    public Vec3f position;
    public float audibility;
    public AlertLevel urgency = AlertLevel.Normal;

    public enum AlertLevel
    {
        Environmental,
        Normal,
        Notice,
        Warning
    }
}