using Vintagestory.API.Client;
using Cairo;
using Vintagestory.API.MathTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Vintagestory.API.Config;
using System.Reflection;

namespace ClosedCaptions;

public class CaptionsList : GuiElement
{
    private LoadedTexture textTexture;
    private ImageSurface imageSurface;
    private Context ctx;
    
    private CairoFont font;
    
    private CaptionsConfig cfg => CaptionsModSystem.config;
    private bool GrowUp => 
        cfg.Position == EnumDialogArea.LeftBottom ||
        cfg.Position == EnumDialogArea.CenterBottom ||
        cfg.Position == EnumDialogArea.RightBottom ||
        cfg.Position == EnumDialogArea.FixedBottom;

    private Color warning;
    private Color notice;
    
    public class Caption {
        public double age = -1;
        public string name;
        public double textWidth;
        public Vec3f position;
        public double volume;
        public int activeSounds;

        public bool active
        {
            get => age >= 0;
        }
    }

    private Queue<ILoadedSound> ActiveSounds;
    public List<Caption> captions;
    private TextExtents fontMetrics;
    
    private const double AudibilityThreshold = 0.1;
    
    public CaptionsList(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
        textTexture = new LoadedTexture(capi);
        imageSurface = new ImageSurface(
            Format.Argb32,
            cfg.Width+2,
            cfg.Height*cfg.MaxCaptions+2
            );
        ctx = genContext(imageSurface);
        
        font = CairoFont.WhiteMediumText().WithFont(cfg.Font).WithFontSize(cfg.FontSize);
        fontMetrics = font.GetTextExtents("Waves Crash");
        
        captions = [];

        FieldInfo field = api.World.GetType().GetField("ActiveSounds", BindingFlags.NonPublic | BindingFlags.Instance);
        ActiveSounds = (Queue<ILoadedSound>)field.GetValue(api.World);
        
        warning = new Color(cfg.WarningRed, cfg.WarningGreen, cfg.WarningBlue);
        notice = new Color(cfg.NoticeRed, cfg.NoticeGreen, cfg.NoticeBlue);
    }
    
    public override void Dispose() {
        textTexture?.Dispose();
        imageSurface?.Dispose();
        ctx?.Dispose();
    }
    
    public override void ComposeElements(Context ctx, ImageSurface surface) {
        font.SetupContext(ctx);
        Bounds.CalcWorldBounds();
        DrawCaptions(ctx);
        generateTexture(imageSurface, ref textTexture);
    }
    
    public override void RenderInteractiveElements(float deltaTime) {
        Update(deltaTime);
        DrawCaptions(ctx);
        generateTexture(imageSurface, ref textTexture);
        api.Render.Render2DTexturePremultipliedAlpha(textTexture.TextureId, (int)Bounds.renderX, (int)Bounds.renderY, (int)Bounds.InnerWidth, (int)Bounds.InnerHeight);
    }
    private void Update(float deltaTime)
    {
        SyncCaptions();
        // Age captions and prune old captions.
        foreach (var caption in captions)
        {
            caption.age += deltaTime;
            
            // Early out if this sound is still young.
            if (caption.age < cfg.Duration) continue;

            RemoveSound(i);
        }
    }

    // Synchronizes the internal caption list with the currently playing ActiveSounds.
    private void SyncCaptions()
    {
        // Reset the activeSound count.
        foreach (var caption in captions)
            caption.activeSounds = 0;

        foreach (var sound in ActiveSounds)
        {
            if (!sound.IsPlaying) continue;
            ProcessSound(sound.Params);
        }
    }

    private void RemoveSound(int index)
    {
        captions.RemoveAt(index);
    }
    
    private void DrawCaptions(Context ctx)
    {
        font.SetupContext(ctx);
        
        ctx.SetSourceRGB(0, 0, 0);
        ctx.Operator = Operator.Clear;
        ctx.Paint();
        ctx.Operator = Operator.Over;
        
        double y = (GrowUp) ? cfg.Height * cfg.MaxCaptions : -cfg.Height;
        var playerPos = api.World.Player.Entity.Pos;
        
        var midHeight = cfg.Height / 2;
        var arrowHeight = (int)cfg.Height - 8;
        var arrowWidth = (int)cfg.Height/2 - 2;
        
        foreach (var caption in captions)
        {
            if (!caption.active) continue;
            y -= (GrowUp) ? cfg.Height : -cfg.Height;
        
            var brightness = ((1 - ((caption.age - cfg.Duration + cfg.FadeDuration) / cfg.FadeDuration)) * Math.Max(1, caption.volume) / 2 + 0.5);

            var bg = new Color(0, 0, 0, 1);
            var fg = new Color(1, 1, 1, 1);
            var stroke = new Color(0.25, 0.25, 0.25, 1);
            var bgBrightness = (cfg.BackgroundOpacity * 0.3333) + (brightness * cfg.BackgroundOpacity * 0.6667);
            var fgBrightness = (cfg.TextOpacity * 0.5) + (brightness * cfg.TextOpacity * 0.5);

            // Get display name, stripping special characters and adding indicators if ShowSymbols is enabled.
            var soundName = GetDisplayName(caption.name);
            
            // Modify colors for special sound types.
            if (caption.name.StartsWith("!"))
            {
                fg = warning;
                stroke = warning;
                if (cfg.InvertedWarnings)
                {
                    fg = bg;
                    stroke = bg;
                    bg = warning;
                }
            }
            else if (caption.name.StartsWith("+"))
            {
                fg = notice;
                stroke = notice;
            }
            
            // Set alpha values.
            bg.A = bgBrightness;
            fg.A = fgBrightness;
            stroke.A = fg.A;
            
            // Draw background
            ctx.SetSourceColor(bg);
            ctx.Rectangle(2, y+1, cfg.Width-2, cfg.Height-2);
            ctx.Fill();
            // Draw stroke
            ctx.SetSourceColor(stroke);
            ctx.Rectangle(2, y+1, cfg.Width-2, cfg.Height-2);
            ctx.LineWidth = 1.0;
            ctx.Stroke();
            // Draw text
            ctx.SetSourceColor(fg);
            ctx.MoveTo(cfg.Width/2 - (caption.textWidth/2), y + midHeight + (fontMetrics.Height/2 - (fontMetrics.Height + fontMetrics.YBearing)));
            ctx.ShowText(soundName);
            
            if (caption.position == null || caption.position.IsZero) continue;
            
            var dist = caption.position.DistanceTo(playerPos.XYZFloat);
            var yaw = Math.Atan2(caption.position.Z - playerPos.Z, caption.position.X - playerPos.X);
            
            // Ignore sounds that are too close.
            if (dist < 2) continue;

            // Ignore directionality for overlapping sounds.
            if (caption.activeSounds > 1) continue;
            
            // 0 is directly in front of the player
            // ±4 is directly left/right of the player, respectively
            // ±8 is directly behind the player
            var direction = GameMath.Mod((yaw + api.World.Player.CameraYaw) / GameMath.TWOPI * 16 + 4, 16) - 8;
            
            // BEHIND YOU
            if (Math.Abs(direction) > 6)
            {
                ctx.NewPath();
                ctx.Arc(1+cfg.Width*.1, y+midHeight, cfg.Height/8, 0, GameMath.TWOPI);
                ctx.Fill();
                ctx.NewPath();
                ctx.Arc(1+cfg.Width*.9, y+midHeight, cfg.Height/8, 0, GameMath.TWOPI);
                ctx.Fill();
            }
            // >>
            else if (direction > 3)
            {
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.9+arrowWidth), y+midHeight, arrowWidth, arrowHeight);
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.9+arrowWidth*.2), y+midHeight, arrowWidth, arrowHeight);
            }
            // >
            else if (direction > 1)
            {
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.9+arrowWidth*.2), y+midHeight, arrowWidth, arrowHeight);
            }
            // <<
            else if (direction < -3)
            {
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.1-arrowWidth), y+midHeight, -arrowWidth, arrowHeight);
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.1-arrowWidth*.2), y+midHeight, -arrowWidth, arrowHeight);
            }
            // <
            else if (direction < -1)
            {
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.1-arrowWidth*.55), y+midHeight, -arrowWidth, arrowHeight);
            }
        }
    }

    private string GetDisplayName(string soundName)
    {
        if (soundName.StartsWith("?"))
        {
            soundName = soundName.Substring(1);
        }
        if (soundName.StartsWith("!"))
        {
            soundName = soundName.Substring(1);
            if (cfg.ShowSymbols)
                soundName = "! " + soundName + " !";
        }
        else if (soundName.StartsWith("+"))
        {
            soundName = soundName.Substring(1);
            if (cfg.ShowSymbols)
                soundName = "+ " + soundName + " +";
        }
        return soundName;
    }

    public void DrawTriangle(Context ctx, double x, double y, double w, double h)
    {
        ctx.NewPath();
        ctx.MoveTo(x, y);
        ctx.LineTo(x - w, y - h/2);
        ctx.LineTo(x - w, y + h/2);
        ctx.LineTo(x, y);
        ctx.Fill();
    }

    public void ProcessSound(SoundParams sound)
    {
        // Unknown condition yoinked without understanding from SubtitlesMod
        var player = api.World.Player;
        if (player == null) return;
        
        // Ignore music
        if (sound.SoundType == EnumSoundType.Music) return;
        
        // Calculate sound ID
        var path = sound.Location.Path;
        var id = path.StartsWith("sounds/") && path.EndsWith(".ogg") ? path.Substring(7, path.Length - 7 - 4) : path;
        // Strip trailing digit
        var lastChar = id.ToCharArray(id.Length - 1, 1)[0];
        if (lastChar >= '0' && lastChar <= '9')
            id = id.Substring(0, id.Length - 1);
        
        var name = Lang.GetIfExists("captions:" + id);
        // Ignore specified sounds
        if (name == "") return;
        // Unnamed sounds
        if (name == null) name = id;
        
        var position = sound.Position;
        if (position == null || position.IsZero)
        {
            if (sound.Volume > AudibilityThreshold)
                AddSound(name, null, sound.Volume);
            return;
        }

        var playerPos = player.Entity.Pos.AsBlockPos;
        var dx = sound.Position.X - playerPos.X;
        var dy = sound.Position.Y - playerPos.Y;
        var dz = sound.Position.Z - playerPos.Z;
        var dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        
        // Ignore sounds that are out of range.
        if (dist > sound.Range) return;
        
        // Ignore sounds that are out of earshot.
        if (!sound.Location.ToString().StartsWith("captions:weather/wind")) return;
        if ((1 - (dist / sound.Range)) * sound.Volume < AudibilityThreshold) {api.Logger.Debug("[CAPTIONS] {0} out of earshot, distance: {1}, range: {2}, volume: {3}, factor: {4}", sound.Location, dist, sound.Range, sound.Volume, (1 - (dist / sound.Range)) * sound.Volume);
            return;
        }
        else api.Logger.Debug("[CAPTIONS] {0} in earshot, distance: {1}, range: {2}, volume: {3}, factor: {4}", sound.Location, dist, sound.Range, sound.Volume, (1 - (dist / sound.Range)) * sound.Volume);
        
        AddSound(name, sound.Position, sound.Volume);
    }
    
    public void AddSound(string name, Vec3f position, double volume)
    {
        // Refresh existing slot if it's already present. 
        foreach (var caption in captions)
        {
            if (caption.active && caption.name == name)
            {
                caption.age = 0;
                caption.activeSounds++;
                caption.position = position;
                caption.volume = volume;
                return;
            }
        }
     
        // Fill an empty slot.
        foreach (var caption in captions)
        {
            if (caption.active) continue;
            caption.age = 0;
            caption.name = name;
            caption.textWidth = font.GetTextExtents(GetDisplayName(caption.name)).Width;
            caption.position = position;
            caption.volume = volume;
            return;
        }
        
        // Else, recycle the oldest active slot.
        int oldestSound = 0;
        for (var i = 1; i < cfg.MaxCaptions; i++)
        {
            if (captions[i].age > captions[oldestSound].age)
            {
                oldestSound = i;
            }
        }
        RemoveSound(oldestSound);
        AddSound(name, position, volume);
    }
}