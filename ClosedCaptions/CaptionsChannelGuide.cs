using System.Collections.Generic;
using Vintagestory.API.Common;
using System.Linq;
using Channel = (string Name, int Priority)?;

namespace ClosedCaptions;

public class CaptionsChannelGuide
{
    private Dictionary<string, Channel> _channels = null;
    private ICoreAPI api;
    public CaptionsChannelGuide(ICoreAPI api)
    {
        this.api = api;
        ReloadChannels();
    }
    public void ReloadChannels()
    {
        var guides = api.World.AssetManager.GetMany<Dictionary<string, Channel>>(api.Logger, "channelguides/", null);
        _channels = new Dictionary<string, Channel>();
        foreach (var g in guides)
        {
            g.Value.ToList().ForEach(c => _channels[c.Key] = c.Value);
        }
    }
    public Channel GetChannel(string name)
    {
        return _channels.TryGetValue(name, out var channel) ? channel : null;
    }
}