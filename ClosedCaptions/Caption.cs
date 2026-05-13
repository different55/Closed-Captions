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
    private ICoreClientAPI _api => CaptionsSystem._api;
    public long LastHeard;
    public double Decay => (_api.ElapsedMilliseconds - LastHeard) / 1000.0;
    public long FirstHeard;
    public double Age => (_api.ElapsedMilliseconds - FirstHeard) / 1000.0;
    public string Name;
    public string Channel = null;
    public int Priority = 1;
    public List<string> Tags = [];
    public Vec3f Position;
    public float Audibility;
    public bool Throttled;
    public List<CaptionedSound> ActiveSounds = [];

    public bool HasTag(string tag) => Tags?.Contains(tag) ?? false;
}