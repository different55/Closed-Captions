using System.Collections.Generic;
using Vintagestory.API.Client;

namespace ClosedCaptions;

public class CaptionedSound
{
    public SoundParams Sound;
    public string Channel = null;
    public int Priority = 1;
    public List<string> Tags = [];
}