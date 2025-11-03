using System.Collections.Generic;
using Vintagestory.API.Common;
using System.Linq;
using Channel = (string Name, int Priority);

namespace ClosedCaptions;

public class CaptionsChannelGuide
{
    private Dictionary<string, Channel> _channels;
    private ICoreAPI _api;
    public CaptionsChannelGuide(ICoreAPI api)
    {
        _api = api;
        ReloadChannels();
    }
    public void ReloadChannels()
    {
        var guides = _api.World.AssetManager.GetMany<Dictionary<string, Channel>>(_api.Logger, "channelguides/");
        _channels = new Dictionary<string, Channel>();
        foreach (var g in guides)
        {
            g.Value.ToList().ForEach(c => _channels[c.Key] = c.Value);
        }
    }
    public Channel GetChannel(string id)
    {
        return _channels.GetValueOrDefault(id);
    }
}