using System.Collections.Generic;
using Vintagestory.API.Common;
using System.Linq;
using Channel = (string Name, int Priority);

namespace ClosedCaptions;

public class CaptionsChannelGuide
{
    private Dictionary<string, Channel> channels = null;
    private ICoreAPI api;
    public CaptionsChannelGuide(ICoreAPI api)
    {
        this.api = api;
        ReloadChannels();
    }
    public void ReloadChannels()
    {
        var guides = api.World.AssetManager.GetMany<Dictionary<string, Channel>>(api.Logger, "channelguides/", null);
        channels = new Dictionary<string, Channel>();
        foreach (var g in guides)
        {
            g.Value.ToList().ForEach(c => channels[c.Key] = c.Value);
        }
    }
    public Channel GetChannel(string id)
    {
        return channels.GetValueOrDefault(id);
    }
}